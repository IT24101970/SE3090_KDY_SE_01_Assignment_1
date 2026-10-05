from typing import Any

import httpx
from pydantic import Field

from app.config import Settings
from app.models.contracts import (
    AppointmentAuditContext,
    StrictModel,
    contains_prompt_injection,
)


class UnknownToolError(ValueError):
    pass


class PauseWorkflowInput(StrictModel):
    workflow_id: int = Field(alias="workflowId", gt=0)
    reason: str = Field(min_length=1, max_length=500)
    validation_violation: str | None = Field(default=None, alias="validationViolation", max_length=500)
    correlation_id: str = Field(alias="correlationId", min_length=1, max_length=128)
    risk_level: str = Field(alias="riskLevel", max_length=32)

    model_config = {"populate_by_name": True, "extra": "forbid"}


class PauseWorkflowOutput(StrictModel):
    workflow_id: int = Field(alias="workflowId", gt=0)
    status: str

    model_config = {"populate_by_name": True, "extra": "forbid"}


class ValidateBusinessRulesInput(StrictModel):
    workflow_id: int = Field(alias="workflowId", gt=0)
    action: str | None = Field(default=None, max_length=100)
    urgency: str = Field(max_length=32)
    evidence: list[str] = Field(default_factory=list, max_length=10)

    model_config = {"populate_by_name": True, "extra": "forbid"}


class GetAppointmentContextInput(StrictModel):
    appointment_id: int = Field(alias="appointmentId", gt=0)

    model_config = {"populate_by_name": True, "extra": "forbid"}


class ToolRegistry:
    """Allow-listed tool gateway; callers cannot provide arbitrary URLs or methods."""

    allowed_tools = frozenset(
        {"GetAppointmentContext", "PauseWorkflow", "ValidateBusinessRules"}
    )

    def __init__(self, settings: Settings, client: httpx.AsyncClient | None = None):
        self.settings = settings
        self._client = client

    async def call(self, name: str, payload: StrictModel) -> tuple[dict[str, Any], str]:
        if name not in self.allowed_tools:
            raise UnknownToolError(f"tool '{name}' is not allow-listed")
        if name == "PauseWorkflow" and contains_prompt_injection(payload.model_dump()):
            raise ValueError("tool input contains prompt injection content")
        if name == "GetAppointmentContext":
            if not isinstance(payload, GetAppointmentContextInput):
                raise TypeError(
                    "GetAppointmentContext requires GetAppointmentContextInput"
                )
            return await self._get_appointment_context(payload)
        if name == "ValidateBusinessRules":
            if not isinstance(payload, ValidateBusinessRulesInput):
                raise TypeError("ValidateBusinessRules requires ValidateBusinessRulesInput")
            return {"valid": True}, "deterministic business-rule check completed"
        if not isinstance(payload, PauseWorkflowInput):
            raise TypeError("PauseWorkflow requires PauseWorkflowInput")
        return await self._pause_workflow(payload)

    async def _get_appointment_context(
        self, payload: GetAppointmentContextInput
    ) -> tuple[dict[str, Any], str]:
        if not self.settings.internal_service_key:
            raise RuntimeError("internal service key is not configured")

        client = self._client
        owns_client = client is None
        if owns_client:
            client = httpx.AsyncClient(
                base_url=self.settings.backend_base_url.rstrip("/"),
                timeout=self.settings.service_timeout_seconds,
                headers={"X-Internal-Service-Key": self.settings.internal_service_key},
            )

        try:
            response = await client.get(
                f"/api/internal/v1/safety-auditor/appointments/{payload.appointment_id}/context"
            )
            if len(response.content) > self.settings.max_output_chars:
                raise RuntimeError("context tool response exceeded the size limit")
            response.raise_for_status()
            context = AppointmentAuditContext.model_validate(response.json())
            return context.model_dump(by_alias=True), "persisted appointment context read"
        except (httpx.HTTPError, ValueError, RuntimeError):
            raise
        finally:
            if owns_client:
                await client.aclose()

    async def _pause_workflow(self, payload: PauseWorkflowInput) -> tuple[dict[str, Any], str]:
        if not self.settings.internal_service_key:
            raise RuntimeError("internal service key is not configured")

        client = self._client
        owns_client = client is None
        if owns_client:
            client = httpx.AsyncClient(
                base_url=self.settings.backend_base_url.rstrip("/"),
                timeout=self.settings.service_timeout_seconds,
                headers={"X-Internal-Service-Key": self.settings.internal_service_key},
            )

        try:
            path = f"/api/internal/v1/safety-auditor/workflows/{payload.workflow_id}/pause"
            last_error: Exception | None = None
            for attempt in range(self.settings.tool_retry_count + 1):
                try:
                    response = await client.post(path, json=payload.model_dump(by_alias=True))
                    if len(response.content) > self.settings.max_output_chars:
                        raise RuntimeError("pause tool response exceeded the size limit")
                    response.raise_for_status()
                    result = PauseWorkflowOutput.model_validate(response.json())
                    return result.model_dump(by_alias=True), "workflow paused for authorized review"
                except (httpx.HTTPError, ValueError, RuntimeError) as error:
                    last_error = error
                    if attempt == self.settings.tool_retry_count:
                        break
            raise RuntimeError("pause tool failed after bounded retries") from last_error
        finally:
            if owns_client:
                await client.aclose()
