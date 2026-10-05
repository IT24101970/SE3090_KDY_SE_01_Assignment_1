import sys
import os
from typing import Dict, Any

# Ensure src/ai-service is in sys.path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..')))

from app.models.triage_schemas import TriageAssessmentRequest, TriageAssessmentResponse
from app.tools.triage_tools import (
    query_medical_knowledge_base,
    compute_severity_index,
    match_specialty_by_diagnosis
)

class SymptomTriageAgent:
    """
    Dedicated AI Agent: Symptom Triage & Specialist Matcher Agent
    Parses patient-reported symptoms, queries medical knowledge base, computes severity index,
    and matches directly to clinical specialty string with complete reasoning trace.
    """
    def __init__(self):
        self.name = "Symptom Triage & Specialist Matcher Agent"

    def process_triage(self, request: TriageAssessmentRequest) -> TriageAssessmentResponse:
        # Step 1: Tool 1 - QueryMedicalKnowledgeBase
        keywords = [item.symptom_keyword for item in request.symptom_list]
        if not keywords and request.raw_symptoms:
            keywords = request.raw_symptoms.split()

        kb_result = query_medical_knowledge_base(keywords)

        # Step 2: Tool 2 - ComputeSeverityIndex
        symptom_dicts = [item.model_dump() for item in request.symptom_list]
        severity_result = compute_severity_index(symptom_dicts, request.raw_symptoms)

        # Step 3: Tool 3 - MatchSpecialtyByDiagnosis
        specialty_info = match_specialty_by_diagnosis(kb_result["category"])
        recommended_specialty_name = specialty_info["name"]

        # Step 4: Build Reasoning Trace
        reasoning_trace = (
            f"[{self.name}] "
            f"1. QueryMedicalKnowledgeBase: Symptom keywords {keywords} matched category '{kb_result['category']}' ({kb_result['condition']}). "
            f"2. ComputeSeverityIndex: Max severity rating {severity_result['max_severity_rating']}/10, emergency red flags = {severity_result['emergency_red_flags']}. Computed urgency score {severity_result['urgency_score']}/100 -> '{severity_result['urgency_level']}'. "
            f"3. MatchSpecialtyByDiagnosis: Category '{kb_result['category']}' mapped directly to specialty '{recommended_specialty_name}'."
        )

        return TriageAssessmentResponse(
            urgency_score=severity_result["urgency_score"],
            urgency_level=severity_result["urgency_level"],
            recommended_specialty=recommended_specialty_name,
            reasoning_trace=reasoning_trace,
            matched_condition=kb_result["condition"]
        )
