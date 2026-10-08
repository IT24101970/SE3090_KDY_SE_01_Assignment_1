import hmac

from fastapi import APIRouter, Depends, Header, HTTPException, status

from app.config import Settings, get_settings
from app.graphs.safety_graph import run_audit
from app.models.contracts import SafetyAuditRequest, SafetyAuditResponse
from app.agents.triage_agent import SymptomTriageAgent
from app.models.triage_schemas import TriageAssessmentRequest, TriageAssessmentResponse
from app.tools.registry import ToolRegistry
from app.DoctorScheduling.routes import router as doctor_scheduling_router

router = APIRouter()
router.include_router(doctor_scheduling_router)



def _authorized(provided_key: str | None, settings: Settings) -> None:
    if settings.internal_service_key:
        if not provided_key or not hmac.compare_digest(provided_key, settings.internal_service_key):
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED,
                detail="internal authentication required",
            )


@router.get("/")
async def root() -> dict[str, str]:
    return {
        "service": "ChannelCenter AI Subsystem",
        "status": "online",
        "version": "1.0.0"
    }


@router.get("/health/live")
async def live() -> dict[str, str]:
    return {"status": "ok"}


@router.get("/health/ready")
async def ready() -> dict[str, str]:
    return {"status": "ready"}


@router.post("/internal/v1/safety-audits/{workflow_id}", response_model=SafetyAuditResponse)
async def safety_audit(
    workflow_id: int,
    request: SafetyAuditRequest,
    x_internal_service_key: str | None = Header(default=None),
    settings: Settings = Depends(get_settings),
) -> SafetyAuditResponse:
    _authorized(x_internal_service_key, settings)
    if workflow_id != request.workflow_id:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="workflow id mismatch")
    return await run_audit(request, ToolRegistry(settings))


@router.post("/internal/v1/triage/assess", response_model=TriageAssessmentResponse)
async def process_triage(
    request: TriageAssessmentRequest,
    x_internal_service_key: str | None = Header(default=None),
    settings: Settings = Depends(get_settings),
) -> TriageAssessmentResponse:
    _authorized(x_internal_service_key, settings)
    agent = SymptomTriageAgent()
    return agent.process_triage(request)

