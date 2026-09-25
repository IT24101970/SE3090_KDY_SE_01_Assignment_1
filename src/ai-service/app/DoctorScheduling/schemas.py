from datetime import datetime
from typing import List, Optional
from enum import Enum
from pydantic import BaseModel, Field

class ApprovalStatus(str, Enum):
    PENDING_APPROVAL = "PendingApproval"
    APPROVED = "Approved"
    REJECTED = "Rejected"
    EXECUTED = "Executed"
    FAILED = "Failed"

class OptimizationInput(BaseModel):
    appointment_id: Optional[int] = Field(default=None, description="ID of the pending appointment created by Student 1")
    triage_assessment_id: Optional[int] = Field(default=None, description="ID of the triage assessment created by Student 3")
    target_doctor_id: Optional[int] = Field(default=None, description="Optional target doctor ID")
    recommended_specialty: Optional[str] = Field(default="Cardiology", description="Specialty recommended by triage agent")
    urgency_score: int = Field(default=75, description="Urgency score (0-100) from triage assessment")
    urgency_level: str = Field(default="High", description="Urgency level (Low, Medium, High, Emergency)")
    desired_window_start: datetime = Field(default_factory=datetime.utcnow, description="Start of target window")
    desired_window_end: Optional[datetime] = Field(default=None, description="End of target window")
    estimated_duration_minutes: int = Field(default=120, description="Session duration in minutes")
    max_patients: int = Field(default=15, description="Max patient limit")
    preferred_room_id: Optional[int] = Field(default=None, description="Preferred room ID")

class PlanStep(BaseModel):
    step_number: int
    title: str
    description: str
    status: str = "Pending"  # Pending, Executing, Completed, Failed

class ToolCallLog(BaseModel):
    tool_name: str
    input_args: dict
    output: dict
    execution_time_ms: float
    status: str = "Success"  # Success, Error

class ValidationRuleResult(BaseModel):
    rule_name: str
    passed: bool
    details: str
    severity: str = "Error"  # Error, Warning

class RecommendedOption(BaseModel):
    appointment_id: Optional[int]
    doctor_id: int
    doctor_name: str
    specialty_name: str
    schedule_id: int
    recommended_start_time: datetime
    recommended_end_time: datetime
    allocated_room_id: int
    allocated_room_name: str
    floor: str
    max_patients: int
    urgency_level: str
    priority_rank: str
    conflict_free_score: float  # 0.0 to 100.0

class WorkflowState(BaseModel):
    workflow_id: str
    objective: str
    created_at: datetime
    current_step: str
    plan: List[PlanStep]
    tool_execution_logs: List[ToolCallLog]
    deterministic_validations: List[ValidationRuleResult]
    recommended_option: Optional[RecommendedOption] = None
    approval_status: ApprovalStatus = ApprovalStatus.PENDING_APPROVAL
    rejection_reason: Optional[str] = None
    executed_schedule_id: Optional[int] = None
    executed_consultation_id: Optional[int] = None
