import hmac

from fastapi import APIRouter, Depends, Header, HTTPException, status

from app.config import Settings, get_settings
from app.graphs.safety_graph import run_audit
from app.models.contracts import SafetyAuditRequest, SafetyAuditResponse
from app.tools.registry import ToolRegistry

router = APIRouter()


def _authorized(provided_key: str | None, settings: Settings) -> None:
    if not settings.internal_service_key or not provided_key or not hmac.compare_digest(
        provided_key, settings.internal_service_key
    ):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="internal authentication required",
        )


@router.get("/health/live")
async def live() -> dict[str, str]:
    return {"status": "ok"}


@router.get("/health/ready")
async def ready(settings: Settings = Depends(get_settings)) -> dict[str, str]:
    if not settings.internal_service_key:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="internal key is not configured",
        )
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
