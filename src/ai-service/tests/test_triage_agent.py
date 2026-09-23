import sys
import os
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from app.models.triage_schemas import TriageAssessmentRequest, SymptomInputItem
from app.agents.triage_agent import SymptomTriageAgent
from app.tools.triage_tools import (
    query_medical_knowledge_base,
    compute_severity_index,
    match_specialty_by_diagnosis
)

class TestTriageAgent(unittest.TestCase):
    def test_query_medical_knowledge_base_chest_pain(self):
        res = query_medical_knowledge_base(["chest pain", "shortness of breath"])
        self.assertTrue(res["matched"])
        self.assertEqual(res["category"], "Cardiovascular")
        self.assertEqual(res["recommended_specialty_name"], "Cardiology")

    def test_compute_severity_index_emergency(self):
        symptoms = [{"symptom_keyword": "chest pain", "severity_rating": 9, "duration_in_days": 1}]
        res = compute_severity_index(symptoms, raw_symptoms="chest pain with left arm pain")
        self.assertEqual(res["urgency_level"], "Emergency")
        self.assertGreaterEqual(res["urgency_score"], 80)

    def test_match_specialty_by_diagnosis(self):
        res = match_specialty_by_diagnosis("Cutaneous")
        self.assertEqual(res["name"], "Dermatology")
        self.assertEqual(res["id"], 2)

    def test_symptom_triage_agent_full_workflow(self):
        agent = SymptomTriageAgent()
        req = TriageAssessmentRequest(
            appointment_id=42,
            raw_symptoms="Severe skin rash with intense itching on forearms",
            symptom_list=[
                SymptomInputItem(symptom_keyword="skin rash", severity_rating=5, duration_in_days=3)
            ]
        )
        res = agent.process_triage(req)
        self.assertEqual(res.recommended_specialty, "Dermatology")
        self.assertIn(res.urgency_level, ["Medium", "High", "Emergency"])
        self.assertIn("Symptom Triage & Specialist Matcher Agent", res.reasoning_trace)

if __name__ == "__main__":
    unittest.main()
