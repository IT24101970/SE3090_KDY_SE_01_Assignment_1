from typing import List, Optional, Dict, Any

try:
    from pydantic import BaseModel, Field
    HAS_PYDANTIC = True
except ImportError:
    HAS_PYDANTIC = False
    class BaseModel:
        def __init__(self, **kwargs):
            for k, v in kwargs.items():
                setattr(self, k, v)
        def model_dump(self) -> Dict[str, Any]:
            return {k: v for k, v in self.__dict__.items()}
    def Field(*args, **kwargs):
        return kwargs.get('default', None)

class SymptomInputItem(BaseModel):
    symptom_keyword: str
    severity_rating: Optional[int] = 1
    duration_in_days: Optional[int] = 1

    def __init__(self, symptom_keyword: str, severity_rating: int = 1, duration_in_days: int = 1, **kwargs):
        if HAS_PYDANTIC:
            super().__init__(symptom_keyword=symptom_keyword, severity_rating=severity_rating, duration_in_days=duration_in_days, **kwargs)
        else:
            self.symptom_keyword = symptom_keyword
            self.severity_rating = severity_rating
            self.duration_in_days = duration_in_days

class TriageAssessmentRequest(BaseModel):
    appointment_id: int
    raw_symptoms: str = ""
    symptom_list: List[SymptomInputItem] = []

    def __init__(self, appointment_id: int, raw_symptoms: str = "", symptom_list: List[SymptomInputItem] = None, **kwargs):
        if HAS_PYDANTIC:
            super().__init__(appointment_id=appointment_id, raw_symptoms=raw_symptoms, symptom_list=symptom_list or [], **kwargs)
        else:
            self.appointment_id = appointment_id
            self.raw_symptoms = raw_symptoms
            self.symptom_list = symptom_list or []

class TriageAssessmentResponse(BaseModel):
    urgency_score: int
    urgency_level: str
    recommended_specialty: str
    reasoning_trace: str
    matched_condition: str

    def __init__(self, urgency_score: int, urgency_level: str, recommended_specialty: str, reasoning_trace: str, matched_condition: str, **kwargs):
        if HAS_PYDANTIC:
            super().__init__(urgency_score=urgency_score, urgency_level=urgency_level, recommended_specialty=recommended_specialty, reasoning_trace=reasoning_trace, matched_condition=matched_condition, **kwargs)
        else:
            self.urgency_score = urgency_score
            self.urgency_level = urgency_level
            self.recommended_specialty = recommended_specialty
            self.reasoning_trace = reasoning_trace
            self.matched_condition = matched_condition
