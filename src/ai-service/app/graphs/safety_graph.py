from typing import Any, TypedDict

from langgraph.graph import END, START, StateGraph

from app.agents.roles import assess_risk, make_plan, validate_proposal
from app.models.contracts import (
    AuditError,
    AuditStatus,
    AuditStep,
    PlanStep,
    RiskLevel,
    SafetyAuditRequest,
    SafetyAuditResponse,
    ToolCall,
    utc_now,
)
from app.tools.registry import PauseWorkflowInput, ToolRegistry


class GraphState(TypedDict, total=False):
    request: SafetyAuditRequest
    plan: list[PlanStep]
    validated_output: dict[str, Any]
    violations: list[str]
    risk_level: RiskLevel
    steps: list[AuditStep]
    tool_calls: list[ToolCall]
    status: AuditStatus
    requires_approval: bool
    validation_summary: str
    final_outcome: str | None
    error: AuditError | None


def _step(items: list[AuditStep], item: AuditStep) -> list[AuditStep]:
    return [*items, item]


def _plan(state: GraphState) -> dict[str, Any]:
    return {
        "plan": [PlanStep.model_validate(item) for item in make_plan()],
        "steps": _step(state.get("steps", []), AuditStep(
            name="Plan", status="Completed", summary="Bounded plan created with four distinct roles."
        )),
    }


def _validate(state: GraphState) -> dict[str, Any]:
    violations, validated, step = validate_proposal(state["request"])
    return {
        "validated_output": validated,
        "violations": violations,
        "validation_summary": step.summary,
        "steps": _step(state.get("steps", []), step),
    }


def _risk_assess(state: GraphState) -> dict[str, Any]:
    risk, step = assess_risk(state["request"].proposal, state.get("violations", []))
    return {"risk_level": risk, "steps": _step(state.get("steps", []), step)}


async def _pause_or_complete(state: GraphState, tools: ToolRegistry) -> dict[str, Any]:
    request = state["request"]
    risk = state.get("risk_level", RiskLevel.HIGH)
    violations = state.get("violations", [])
    should_pause = bool(violations) or risk in {RiskLevel.HIGH, RiskLevel.EMERGENCY}
    tool_calls = list(state.get("tool_calls", []))

    if should_pause:
        try:
            _, summary = await tools.call(
                "PauseWorkflow",
                PauseWorkflowInput(
                    workflowId=request.workflow_id,
                    reason="Unsafe or high-impact proposal requires authorized human review.",
                    validationViolation="; ".join(violations)[:500] or None,
                    correlationId=request.correlation_id,
                    riskLevel=risk.value,
                ),
            )
            tool_calls.append(ToolCall(name="PauseWorkflow", status="Completed", summary=summary))
            return {
                "status": AuditStatus.PAUSED_FOR_APPROVAL,
                "requires_approval": True,
                "tool_calls": tool_calls,
                "final_outcome": "Paused before any proposed action was executed.",
            }
        except Exception as error:
            tool_calls.append(ToolCall(name="PauseWorkflow", status="Failed", summary="Pause tool failed safely."))
            return {
                "status": AuditStatus.SAFE_FAILED,
                "requires_approval": False,
                "tool_calls": tool_calls,
                "final_outcome": "No proposed action was executed.",
                "error": AuditError(code="pause_tool_failed", message=str(error)[:500]),
            }

    return {
        "status": AuditStatus.COMPLETED,
        "requires_approval": False,
        "tool_calls": tool_calls,
        "final_outcome": "Proposal passed deterministic safety checks; no action was executed by the auditor.",
    }


def build_graph(tools: ToolRegistry):
    async def terminal_node(state: GraphState) -> dict[str, Any]:
        return await _pause_or_complete(state, tools)

    graph = StateGraph(GraphState)
    graph.add_node("Plan", _plan)
    graph.add_node("Validate", _validate)
    graph.add_node("RiskAssess", _risk_assess)
    graph.add_node("PauseOrComplete", terminal_node)
    graph.add_edge(START, "Plan")
    graph.add_edge("Plan", "Validate")
    graph.add_edge("Validate", "RiskAssess")
    graph.add_edge("RiskAssess", "PauseOrComplete")
    graph.add_edge("PauseOrComplete", END)
    return graph.compile()


async def run_audit(request: SafetyAuditRequest, tools: ToolRegistry) -> SafetyAuditResponse:
    result = await build_graph(tools).ainvoke({"request": request, "steps": [], "tool_calls": []})
    return SafetyAuditResponse(
        workflowId=request.workflow_id,
        correlationId=request.correlation_id,
        contractVersion=request.contract_version,
        status=result["status"],
        riskLevel=result.get("risk_level", RiskLevel.HIGH),
        requiresApproval=result.get("requires_approval", False),
        violations=result.get("violations", []),
        validatedOutput=result.get("validated_output", {}),
        plan=result.get("plan", []),
        validationSummary=result.get("validation_summary", ""),
        finalOutcome=result.get("final_outcome"),
        steps=result.get("steps", []),
        toolCalls=result.get("tool_calls", []),
        error=result.get("error"),
        completedAt=utc_now(),
    )
