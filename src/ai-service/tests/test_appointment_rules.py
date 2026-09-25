from datetime import datetime, timedelta, timezone

from app.agents.roles import evaluate_appointment_context


def context(
    urgency: str,
    specialty: str,
    recommended: str,
    appointment_date: datetime,
    symptoms: str = "symptoms",
    available: bool = True,
) -> dict:
    return {
        "appointmentDate": appointment_date.isoformat(),
        "triage": {
            "urgencyLevel": urgency,
            "recommendedSpecialty": recommended,
            "rawSymptoms": symptoms,
        },
        "doctor": {"specialtyName": specialty},
        "schedule": {"isAvailable": available},
    }


def test_safe_high_specialty_match_is_allowed():
    violations = evaluate_appointment_context(
        context(
            "High",
            "Cardiology",
            "Cardiology",
            datetime.now(timezone.utc) + timedelta(hours=4),
        )
    )
    assert violations == []


def test_emergency_outside_same_day_window_is_paused():
    violations = evaluate_appointment_context(
        context(
            "Emergency",
            "Cardiology",
            "Cardiology",
            datetime.now(timezone.utc) + timedelta(days=1),
        )
    )
    assert any("SAFETY-DATE-001" in item for item in violations)


def test_low_common_cold_with_dentistry_is_paused():
    violations = evaluate_appointment_context(
        context(
            "Low",
            "Dentistry",
            "General Medicine",
            datetime.now(timezone.utc) + timedelta(days=20),
            symptoms="common cold and cough",
        )
    )
    assert any("SAFETY-SPECIALTY-003" in item for item in violations)


def test_medium_related_specialty_is_allowed():
    violations = evaluate_appointment_context(
        context(
            "Medium",
            "Internal Medicine",
            "Cardiology",
            datetime.now(timezone.utc) + timedelta(days=2),
        )
    )
    assert violations == []


def test_missing_schedule_is_a_hard_violation():
    appointment = context(
        "High",
        "Cardiology",
        "Cardiology",
        datetime.now(timezone.utc) + timedelta(hours=2),
    )
    appointment["schedule"] = None
    violations = evaluate_appointment_context(appointment)
    assert any("SAFETY-SCHEDULE-001" in item for item in violations)
