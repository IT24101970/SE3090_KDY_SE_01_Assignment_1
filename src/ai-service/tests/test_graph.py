import pytest

from app.config import Settings
from app.graphs.safety_graph import run_audit
from app.models.contracts import AuditStatus, SafetyAuditRequest
from app.tools.registry import ToolRegistry


class FakeTools(ToolRegistry):
    async def call(self, name, payload):
        if name == "PauseWorkflow":
            return {"workflowId": payload.workflow_id, "status": "PausedForApproval"}, "fake pause"
        return {"valid": True}, "validated"


def request(proposal):
    return SafetyAuditRequest(
        workflowId=7,
        objective="Review triage proposal",
        proposal=proposal,
        sourceAgent="TestAgent",
        correlationId="corr-7",
        contractVersion="safety-audit.v1",
    )


@pytest.mark.asyncio
async def test_safe_proposal_completes_without_model():
    result = await run_audit(
        request({"action": "review", "urgency": "low", "evidence": ["case-1"]}),
        FakeTools(Settings()),
    )
    assert result.status == AuditStatus.COMPLETED
    assert [step.name for step in result.plan] == ["Plan", "Validate", "RiskAssess", "PauseOrComplete"]
    assert result.requires_approval is False


@pytest.mark.asyncio
async def test_emergency_proposal_pauses():
    result = await run_audit(
        request({"action": "review", "urgency": "emergency", "evidence": ["case-1"]}),
        FakeTools(Settings()),
    )
    assert result.status == AuditStatus.PAUSED_FOR_APPROVAL
    assert result.risk_level.value == "Emergency"


@pytest.mark.asyncio
async def test_missing_evidence_is_paused():
    result = await run_audit(
        request({"action": "review", "urgency": "low"}),
        FakeTools(Settings()),
    )
    assert result.status == AuditStatus.PAUSED_FOR_APPROVAL
    assert any("SAFETY-EVIDENCE-002" in violation for violation in result.violations)
