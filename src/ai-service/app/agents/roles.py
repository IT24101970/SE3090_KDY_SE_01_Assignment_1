from datetime import datetime, timedelta, timezone
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

RELATED_SPECIALTIES = {
    "cardiology": {"cardiology", "cardiovascular medicine", "internal medicine"},
    "internal medicine": {"internal medicine", "general medicine", "family medicine"},
    "general medicine": {"general medicine", "internal medicine", "family medicine"},
    "family medicine": {"family medicine", "general medicine", "internal medicine"},
    "neurology": {"neurology", "neuroscience", "neuro medicine"},
    "pediatrics": {"pediatrics", "paediatrics", "child medicine"},
    "dermatology": {"dermatology", "skin medicine"},
    "orthopedics": {"orthopedics", "orthopaedics", "trauma"},
    "ent": {"ent", "otolaryngology"},
}


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
    context = proposal.get("context")
    context_error = proposal.get("contextError")

    if not isinstance(action, str) or not action.strip():
        violations.append("SAFETY-EVIDENCE-001: action is required")
    if not isinstance(evidence, list) or not evidence or not all(isinstance(item, str) and item.strip() for item in evidence):
        violations.append("SAFETY-EVIDENCE-002: at least one evidence reference is required")
    if urgency not in {"low", "medium", "high", "emergency"}:
        violations.append("SAFETY-SCHEMA-001: urgency must be low, medium, high or emergency")
    if contains_prompt_injection(proposal):
        violations.append("SAFETY-INJECTION-001: prompt injection content rejected")
    if isinstance(context, dict):
        violations.extend(evaluate_appointment_context(context))
    if context_error:
        violations.append("SAFETY-CONTEXT-001: persisted appointment context could not be read")

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


def _value(value: Any, *names: str) -> Any:
    if not isinstance(value, dict):
        return None
    for name in names:
        if name in value:
            return value[name]
    return None


def _normalise(value: Any) -> str:
    return " ".join(str(value or "").lower().replace("-", " ").split())


def _specialty_matches(recommended: str, actual: str) -> bool:
    recommended = _normalise(recommended)
    actual = _normalise(actual)
    if not recommended or not actual:
        return False
    if recommended == actual or recommended in actual or actual in recommended:
        return True
    return actual in RELATED_SPECIALTIES.get(recommended, set())


def evaluate_appointment_context(context: dict[str, Any]) -> list[str]:
    """Evaluate persisted appointment facts without a model or database connection."""
    violations: list[str] = []
    appointment_date = _value(context, "appointmentDate", "appointment_date")
    triage = _value(context, "triage") or {}
    doctor = _value(context, "doctor") or {}
    schedule = _value(context, "schedule")
    urgency = _normalise(_value(triage, "urgencyLevel", "urgency_level"))
    recommended = _value(triage, "recommendedSpecialty", "recommended_specialty")
    actual = _value(doctor, "specialtyName", "specialty_name")
    symptoms = _normalise(_value(triage, "rawSymptoms", "raw_symptoms"))

    if not doctor:
        violations.append("SAFETY-APPOINTMENT-001: assigned doctor is missing")
    if not triage:
        violations.append("SAFETY-TRIAGE-001: triage assessment is missing")
    if not schedule:
        violations.append("SAFETY-SCHEDULE-001: assigned schedule is missing")
    elif not bool(_value(schedule, "isAvailable", "is_available")):
        violations.append("SAFETY-SCHEDULE-002: assigned schedule is unavailable")

    if urgency in {"high", "emergency"} and not _specialty_matches(recommended, actual):
        violations.append(
            "SAFETY-SPECIALTY-001: high or emergency patient is assigned to an unrelated specialty"
        )
    elif urgency == "medium" and not _specialty_matches(recommended, actual):
        violations.append(
            "SAFETY-SPECIALTY-002: medium urgency patient is not assigned to a clinically related specialty"
        )
    elif urgency == "low":
        clearly_unrelated = (
            "dentistry" in _normalise(actual)
            and any(token in symptoms for token in ("cold", "cough", "fever", "flu", "sore throat"))
        )
        if clearly_unrelated:
            violations.append(
                "SAFETY-SPECIALTY-003: common respiratory symptoms are assigned to dentistry"
            )

    if appointment_date:
        try:
            scheduled = datetime.fromisoformat(str(appointment_date).replace("Z", "+00:00"))
            if scheduled.tzinfo is None:
                scheduled = scheduled.replace(tzinfo=timezone.utc)
            now = datetime.now(timezone.utc)
            if scheduled < now:
                violations.append("SAFETY-DATE-006: appointment is scheduled in the past")
            if schedule:
                start = _value(schedule, "startTime", "start_time")
                end = _value(schedule, "endTime", "end_time")
                if start and end:
                    schedule_start = datetime.fromisoformat(str(start).replace("Z", "+00:00"))
                    schedule_end = datetime.fromisoformat(str(end).replace("Z", "+00:00"))
                    if schedule_start.tzinfo is None:
                        schedule_start = schedule_start.replace(tzinfo=timezone.utc)
                    if schedule_end.tzinfo is None:
                        schedule_end = schedule_end.replace(tzinfo=timezone.utc)
                    if not schedule_start <= scheduled <= schedule_end:
                        violations.append(
                            "SAFETY-SCHEDULE-003: appointment is outside the assigned schedule"
                        )
            if urgency == "emergency" and scheduled.date() != now.date():
                violations.append("SAFETY-DATE-001: emergency appointment is not scheduled the same day")
            elif urgency == "high" and scheduled > now + timedelta(hours=24):
                violations.append("SAFETY-DATE-002: high urgency appointment exceeds the 24 hour window")
            elif urgency == "medium" and scheduled > now + timedelta(days=3):
                violations.append("SAFETY-DATE-003: medium urgency appointment exceeds the three day window")
        except (TypeError, ValueError):
            violations.append("SAFETY-DATE-004: appointment date is invalid")
    elif urgency in {"high", "emergency", "medium"}:
        violations.append("SAFETY-DATE-005: appointment date is missing")

    return violations


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
