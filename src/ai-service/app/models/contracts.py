import json
import re
from datetime import datetime, timezone
from enum import Enum
from typing import Any

from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator


PROMPT_INJECTION_PATTERN = re.compile(
    r"(?:ignore\s+(?:all\s+)?previous|disregard\s+(?:all\s+)?prior|system\s+message|developer\s+message|reveal\s+(?:the\s+)?prompt|bypass\s+(?:the\s+)?safety)",
    re.IGNORECASE,
)


class AuditStatus(str, Enum):
    COMPLETED = "Completed"
    PAUSED_FOR_APPROVAL = "PausedForApproval"
    SAFE_FAILED = "SafeFailed"


class RiskLevel(str, Enum):
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
    EMERGENCY = "Emergency"


class StrictModel(BaseModel):
    model_config = ConfigDict(extra="forbid", str_strip_whitespace=True)


def _strings(value: Any):
    if isinstance(value, str):
        yield value
    elif isinstance(value, dict):
        for key, item in value.items():
            yield from _strings(key)
            yield from _strings(item)
    elif isinstance(value, (list, tuple)):
        for item in value:
            yield from _strings(item)


def contains_prompt_injection(value: Any) -> bool:
    return any(PROMPT_INJECTION_PATTERN.search(text) for text in _strings(value))


class SafetyAuditRequest(StrictModel):
    workflow_id: int = Field(alias="workflowId", gt=0)
    objective: str = Field(min_length=1, max_length=255)
    proposal: dict[str, Any] = Field(default_factory=dict)
    source_agent: str = Field(default="ChannelCenter.API", alias="sourceAgent", max_length=100)
    correlation_id: str = Field(alias="correlationId", min_length=1, max_length=128)
    contract_version: str = Field(default="safety-audit.v1", alias="contractVersion", max_length=32)
    appointment_id: int | None = Field(default=None, alias="appointmentId", gt=0)

    model_config = ConfigDict(extra="forbid", populate_by_name=True, str_strip_whitespace=True)

    @field_validator("proposal")
    @classmethod
    def bound_proposal_size(cls, value: dict[str, Any]) -> dict[str, Any]:
        if len(json.dumps(value, separators=(",", ":"), default=str)) > 16_000:
            raise ValueError("proposal exceeds the maximum input size")
        return value

    @model_validator(mode="after")
    def reject_prompt_injection(self):
        if contains_prompt_injection(self.proposal) or contains_prompt_injection(self.objective):
            raise ValueError("prompt injection content is not accepted")
        return self


class TriageAuditContext(StrictModel):
    id: int = 0
    urgency_level: str = Field(default="", alias="urgencyLevel")
    recommended_specialty: str = Field(default="", alias="recommendedSpecialty")
    raw_symptoms: str = Field(default="", alias="rawSymptoms")

    model_config = ConfigDict(populate_by_name=True, extra="forbid")


class DoctorAuditContext(StrictModel):
    id: int = 0
    specialty_id: int = Field(default=0, alias="specialtyId")
    specialty_name: str = Field(default="", alias="specialtyName")

    model_config = ConfigDict(populate_by_name=True, extra="forbid")


class ScheduleAuditContext(StrictModel):
    id: int = 0
    doctor_id: int = Field(default=0, alias="doctorId")
    start_time: datetime | None = Field(default=None, alias="startTime")
    end_time: datetime | None = Field(default=None, alias="endTime")
    max_patients: int = Field(default=0, alias="maxPatients")
    booked_patients: int = Field(default=0, alias="bookedPatients")
    is_available: bool = Field(default=False, alias="isAvailable")

    model_config = ConfigDict(populate_by_name=True, extra="forbid")


class AppointmentAuditContext(StrictModel):
    appointment_id: int = Field(alias="appointmentId", gt=0)
    patient_id: int = Field(default=0, alias="patientId")
    doctor_id: int = Field(default=0, alias="doctorId")
    schedule_id: int = Field(default=0, alias="scheduleId")
    appointment_date: datetime | None = Field(default=None, alias="appointmentDate")
    appointment_status: str = Field(default="", alias="appointmentStatus")
    reason_for_visit: str = Field(default="", alias="reasonForVisit")
    triage: TriageAuditContext | None = None
    doctor: DoctorAuditContext | None = None
    schedule: ScheduleAuditContext | None = None

    model_config = ConfigDict(populate_by_name=True, extra="forbid")


class PlanStep(StrictModel):
    name: str = Field(max_length=64)
    role: str = Field(max_length=64)
    tool: str = Field(max_length=100)
    purpose: str = Field(max_length=300)


class ToolCall(StrictModel):
    name: str = Field(max_length=100)
    status: str = Field(max_length=32)
    summary: str = Field(max_length=500)


class AuditStep(StrictModel):
    name: str = Field(max_length=64)
    status: str = Field(max_length=32)
    summary: str = Field(max_length=500)
    duration_ms: int = Field(default=0, alias="durationMs", ge=0, le=60_000)

    model_config = ConfigDict(extra="forbid", populate_by_name=True, str_strip_whitespace=True)


class AuditError(StrictModel):
    code: str = Field(max_length=64)
    message: str = Field(max_length=500)


class SafetyAuditResponse(StrictModel):
    workflow_id: int = Field(alias="workflowId", gt=0)
    correlation_id: str = Field(alias="correlationId", max_length=128)
    contract_version: str = Field(default="safety-audit.v1", alias="contractVersion", max_length=32)
    status: AuditStatus
    risk_level: RiskLevel = Field(alias="riskLevel")
    requires_approval: bool = Field(alias="requiresApproval")
    violations: list[str] = Field(default_factory=list, max_length=20)
    validated_output: dict[str, Any] = Field(default_factory=dict, alias="validatedOutput")
    plan: list[PlanStep] = Field(default_factory=list, max_length=4)
    validation_summary: str = Field(default="", alias="validationSummary", max_length=2_000)
    final_outcome: str | None = Field(default=None, alias="finalOutcome", max_length=2_000)
    steps: list[AuditStep] = Field(default_factory=list, max_length=4)
    tool_calls: list[ToolCall] = Field(default_factory=list, alias="toolCalls", max_length=8)
    error: AuditError | None = None
    completed_at: datetime | None = Field(default=None, alias="completedAt")

    model_config = ConfigDict(
        extra="forbid",
        populate_by_name=True,
    )

    @field_validator("validated_output")
    @classmethod
    def bound_output_size(cls, value: dict[str, Any]) -> dict[str, Any]:
        if len(json.dumps(value, separators=(",", ":"), default=str)) > 16_000:
            raise ValueError("validated output exceeds the maximum output size")
        return value


def utc_now() -> datetime:
    return datetime.now(timezone.utc)
