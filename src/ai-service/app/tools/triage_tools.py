from typing import List, Dict, Any

MEDICAL_KNOWLEDGE_BASE = [
    {
        "category": "Cardiovascular",
        "keywords": ["chest pain", "shortness of breath", "palpitations", "arm numbness", "heart pain", "cardiac"],
        "condition": "Possible Angina / Acute Coronary Syndrome",
        "default_specialty_name": "Cardiology"
    },
    {
        "category": "Cutaneous",
        "keywords": ["rash", "skin lesion", "itching", "acne", "eczema", "dermatitis", "hives"],
        "condition": "Cutaneous / Dermatological Inflammation",
        "default_specialty_name": "Dermatology"
    },
    {
        "category": "Neurological",
        "keywords": ["headache", "migraine", "dizziness", "seizure", "numbness", "tingling", "stroke"],
        "condition": "Neurological Dysregulation / Migraine Syndrome",
        "default_specialty_name": "Neurology"
    },
    {
        "category": "Musculoskeletal",
        "keywords": ["joint pain", "knee pain", "back pain", "fracture", "bone", "sprain", "ligament"],
        "condition": "Musculoskeletal / Joint Disorder",
        "default_specialty_name": "Orthopedics"
    },
    {
        "category": "Gastrointestinal",
        "keywords": ["stomach pain", "nausea", "vomiting", "acid reflux", "diarrhea", "abdominal pain"],
        "condition": "Gastrointestinal Disorder",
        "default_specialty_name": "Gastroenterology"
    }
]

def query_medical_knowledge_base(symptom_keywords: List[str]) -> Dict[str, Any]:
    """
    Allow-Listed Tool 1: QueryMedicalKnowledgeBase(symptomKeywords)
    Queries internal medical knowledge repository to identify condition category and matched clinical diagnosis.
    """
    keywords_lower = [k.lower().strip() for k in symptom_keywords]
    
    for entry in MEDICAL_KNOWLEDGE_BASE:
        for kw in entry["keywords"]:
            if any(kw in user_kw for user_kw in keywords_lower):
                return {
                    "matched": True,
                    "category": entry["category"],
                    "condition": entry["condition"],
                    "recommended_specialty_name": entry["default_specialty_name"]
                }
                
    return {
        "matched": False,
        "category": "General",
        "condition": "Unspecified Primary Clinical Complaint",
        "recommended_specialty_name": "General Medicine"
    }

def compute_severity_index(symptom_list: List[Dict[str, Any]], raw_symptoms: str = "") -> Dict[str, Any]:
    """
    Allow-Listed Tool 2: ComputeSeverityIndex(symptomList, duration)
    Calculates weighted severity index (0-100) and urgency classification (Low, Medium, High, Emergency).
    """
    max_rating = 1
    total_duration = 1

    for item in symptom_list:
        rating = item.get("severity_rating", 1) or 1
        duration = item.get("duration_in_days", 1) or 1
        if rating > max_rating:
            max_rating = rating
        if duration > total_duration:
            total_duration = duration

    raw_lower = raw_symptoms.lower()
    has_emergency_red_flags = any(red in raw_lower for red in ["chest pain", "shortness of breath", "unconscious", "severe bleeding", "stroke"])

    if has_emergency_red_flags:
        urgency_score = max(95, max_rating * 10)
    else:
        urgency_score = min(100, max_rating * 10)

    if urgency_score >= 80:
        urgency_level = "Emergency"
    elif urgency_score >= 60:
        urgency_level = "High"
    elif urgency_score >= 30:
        urgency_level = "Medium"
    else:
        urgency_level = "Low"

    return {
        "urgency_score": urgency_score,
        "urgency_level": urgency_level,
        "max_severity_rating": max_rating,
        "total_duration": total_duration,
        "emergency_red_flags": has_emergency_red_flags
    }

def match_specialty_by_diagnosis(condition_category: str) -> Dict[str, Any]:
    """
    Allow-Listed Tool 3: MatchSpecialtyByDiagnosis(conditionCategory)
    Maps diagnosis condition category directly to specialty name.
    """
    mapping = {
        "Cardiovascular": {"id": 1, "name": "Cardiology", "description": "Heart & Vascular Care"},
        "Cutaneous": {"id": 2, "name": "Dermatology", "description": "Skin & Cutaneous Health"},
        "Neurological": {"id": 3, "name": "Neurology", "description": "Brain & Nervous System"},
        "Musculoskeletal": {"id": 4, "name": "Orthopedics", "description": "Bones & Joint Care"},
        "Gastrointestinal": {"id": 5, "name": "Gastroenterology", "description": "Digestive System"},
        "General": {"id": 6, "name": "General Medicine", "description": "General Primary Care"}
    }
    
    return mapping.get(condition_category, mapping["General"])
