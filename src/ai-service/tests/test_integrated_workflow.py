import unittest
import sys
import os

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from app.models.triage_schemas import TriageAssessmentRequest, SymptomInputItem
from app.agents.triage_agent import SymptomTriageAgent

class TestIntegratedWorkflow(unittest.TestCase):
    """
    SE3110 Required E2E Integrated Workflow Test:
    Step 1: Patient submits raw symptom text via mobile/web interface.
    Step 2: AI Subsystem processes symptoms, determines urgency (Risk Tier), and recommends specialty.
    Step 3: Verification of clinical reasoning trace and risk evaluation.
    """
    def test_e2e_integrated_triage_to_booking_workflow(self):
        agent = SymptomTriageAgent()
        req = TriageAssessmentRequest(
            appointment_id=101,
            raw_symptoms="Severe pressure in chest radiating to jaw",
            symptom_list=[
                SymptomInputItem(symptom_keyword="chest pain", severity_rating=9, duration_in_days=1)
            ]
        )
        res = agent.process_triage(req)
        
        # Step 2: Validate AI specialty recommendation and urgency classification
        self.assertEqual(res.recommended_specialty, "Cardiology")
        self.assertEqual(res.urgency_level, "Emergency")
        self.assertGreaterEqual(res.urgency_score, 80)
        self.assertTrue(len(res.reasoning_trace) > 0)

if __name__ == "__main__":
    unittest.main()
