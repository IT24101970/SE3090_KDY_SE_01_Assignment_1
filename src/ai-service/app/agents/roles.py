from typing import Any

from app.models.contracts import AuditStep, RiskLevel, SafetyAuditRequest, contains_prompt_injection


HIGH_IMPACT_ACTIONS = frozenset(
    {
        "cancel_appointment",
        "reassign_doctor",
        "prescribe",
        "discharge",
        "delete_patient_record",
        "admin_override",
    }
)


def make_plan() -> list[dict[str, str]]:
    return [
        {"name": "Plan", "role": "Planner", "tool": "ValidateBusinessRules", "purpose": "Create the bounded, four-role safety audit plan."},
        {"name": "Validate", "role": "EvidenceValidator", "tool": "ValidateBusinessRules", "purpose": "Validate proposal shape, evidence and allowed values."},
        {"name": "RiskAssess", "role": "RiskClassifier", "tool": "ValidateBusinessRules", "purpose": "Classify urgency and high-impact action risk deterministically."},
        {"name": "PauseOrComplete", "role": "SafetyAuditor", "tool": "PauseWorkflow", "purpose": "Pause unsafe work or return a concise validated outcome."},
    ]


def validate_proposal(request: SafetyAuditRequest) -> tuple[list[str], dict[str, Any], AuditStep]:
    proposal = request.proposal
    violations: list[str] = []
    evidence = proposal.get("evidence")
    action = proposal.get("action")
    urgency = str(proposal.get("urgency", "low")).lower()

    if not isinstance(action, str) or not action.strip():
        violations.append("SAFETY-EVIDENCE-001: action is required")
    if not isinstance(evidence, list) or not evidence or not all(isinstance(item, str) and item.strip() for item in evidence):
        violations.append("SAFETY-EVIDENCE-002: at least one evidence reference is required")
    if urgency not in {"low", "medium", "high", "emergency"}:
        violations.append("SAFETY-SCHEMA-001: urgency must be low, medium, high or emergency")
    if contains_prompt_injection(proposal):
        violations.append("SAFETY-INJECTION-001: prompt injection content rejected")

    validated = {
        "action": action.strip() if isinstance(action, str) else None,
        "urgency": urgency,
        "evidence": evidence[:10] if isinstance(evidence, list) else [],
    }
    return violations, validated, AuditStep(
        name="Validate",
        status="Passed" if not violations else "Failed",
        summary="Proposal evidence and schema checks completed." if not violations else "; ".join(violations),
    )


def assess_risk(proposal: dict[str, Any], violations: list[str]) -> tuple[RiskLevel, AuditStep]:
    urgency = str(proposal.get("urgency", "low")).lower()
    action = str(proposal.get("action", "")).lower()
    high_impact = bool(proposal.get("high_impact", False)) or action in HIGH_IMPACT_ACTIONS
    if urgency == "emergency":
        risk = RiskLevel.EMERGENCY
    elif violations or high_impact or urgency == "high":
        risk = RiskLevel.HIGH
    elif urgency == "medium":
        risk = RiskLevel.MEDIUM
    else:
        risk = RiskLevel.LOW
    return risk, AuditStep(
        name="RiskAssess",
        status="Completed",
        summary=f"Deterministic risk classification: {risk.value}.",
    )
