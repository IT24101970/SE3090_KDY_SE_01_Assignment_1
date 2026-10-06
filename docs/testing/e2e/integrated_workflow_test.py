import pytest
import sys
from pathlib import Path

# Add ai-service to path
AI_SERVICE_DIR = Path(__file__).resolve().parents[3] / "src" / "ai-service"
sys.path.insert(0, str(AI_SERVICE_DIR))

from app.services.triage_agent import process_triage_request
from app.services.appointment_rules import evaluate_booking_rules

def test_e2e_integrated_triage_to_booking_workflow():
    """
    SE3110 Required E2E Integrated Workflow Test:
    Step 1: Patient submits raw symptom text via mobile/web interface.
    Step 2: AI Subsystem processes symptoms, determines urgency (Risk Tier), and recommends specialty.
    Step 3: Business rules evaluate doctor schedule availability and lock appointment slot.
    Step 4: Output structure is validated for API consumption.
    """
    # Step 1: Input payload
    raw_symptom = "Experiencing sudden blurriness in left eye with severe orbital headache"
    patient_id = 42

    # Step 2: AI Processing & Specialty Routing
    triage_result = process_triage_request(
        symptoms_text=raw_symptom,
        patient_id=patient_id,
        age=45,
        gender="Male"
    )

    assert triage_result is not None
    assert triage_result["recommended_specialty"] in ["Ophthalmology", "Neurology"]
    assert triage_result["risk_tier"] in [1, 2, 3]

    # Step 3: Business Rule & Slot Evaluation
    booking_validation = evaluate_booking_rules(
        specialty=triage_result["recommended_specialty"],
        risk_tier=triage_result["risk_tier"],
        requested_date="2026-10-15"
    )

    assert booking_validation["allowed"] is True
    assert booking_validation["requires_clinical_review"] == (triage_result["risk_tier"] == 1)

    print("\n✅ Integrated E2E Workflow Completed Successfully!")
