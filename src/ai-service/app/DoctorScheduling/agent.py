import uuid
from datetime import datetime, timedelta
from typing import Dict, Any, List, Optional
import httpx

from .schemas import (
    OptimizationInput,
    WorkflowState,
    PlanStep,
    ToolCallLog,
    ValidationRuleResult,
    RecommendedOption,
    ApprovalStatus
)
from .tools import ScheduleTools, SPECIALTY_DOCTOR_MAP

WORKFLOW_DB: Dict[str, WorkflowState] = {}

class ScheduleOptimizationAgent:
    """
    Dedicated Agentic AI for Student 2: Schedule & Capacity Optimization Agent
    Domain: Doctor Scheduling & Consultation Management (Component B)
    Reads TriageAssessment (UrgencyScore & RecommendedSpecialty) and assigns Appointment to DoctorSchedule with priority.
    """

    def __init__(self):
        self.tools = ScheduleTools()

    async def execute_optimization_workflow(self, input_data: OptimizationInput) -> WorkflowState:
        workflow_id = f"wf-sched-opt-{datetime.utcnow().strftime('%Y%m%d%H%M%S')}-{uuid.uuid4().hex[:6]}"
        date_str = input_data.desired_window_start.strftime("%Y-%m-%d")
        appt_id = input_data.appointment_id or 1

        # 1. Multi-Step Execution Plan
        plan_steps = [
            PlanStep(step_number=1, title="Fetch Triage Assessment & Urgency Score", description=f"Execute GetTriageAssessmentForAppointment for Appointment #{appt_id}.", status="Executing"),
            PlanStep(step_number=2, title="Match Recommended Specialty & Doctor Schedules", description="Execute GetDoctorSchedulesBySpecialty to find active specialist schedules.", status="Pending"),
            PlanStep(step_number=3, title="Calculate Workload & Assign Priority Rank", description="Execute CalculateDoctorWorkloadAndPriority to rank placement based on urgency score.", status="Pending"),
            PlanStep(step_number=4, title="Check Room Availability", description="Execute CheckRoomAvailability tool for consultation room allocation.", status="Pending"),
            PlanStep(step_number=5, title="Assign Appointment to Schedule & Pause for Human Approval", description="Select optimal slot, generate assignment, and pause for human verification.", status="Pending")
        ]

        tool_logs: List[ToolCallLog] = []
        validations: List[ValidationRuleResult] = []

        # --- STEP 1: Tool 1 - GetTriageAssessmentForAppointment ---
        triage_tool_res = await self.tools.get_triage_assessment_for_appointment(appt_id)
        tool_logs.append(ToolCallLog(
            tool_name="GetTriageAssessmentForAppointment",
            input_args={"appointmentId": appt_id},
            output=triage_tool_res,
            execution_time_ms=triage_tool_res.get("duration_ms", 10.0),
            status="Success"
        ))
        plan_steps[0].status = "Completed"
        plan_steps[1].status = "Executing"

        specialty = triage_tool_res.get("recommended_specialty") or input_data.recommended_specialty or "Cardiology"
        urgency_score = triage_tool_res.get("urgency_score", input_data.urgency_score)
        urgency_level = triage_tool_res.get("urgency_level", input_data.urgency_level)

        # --- STEP 2: Tool 2 - GetDoctorSchedulesBySpecialty ---
        spec_tool_res = await self.tools.get_doctor_schedules_by_specialty(specialty, date_str)
        tool_logs.append(ToolCallLog(
            tool_name="GetDoctorSchedulesBySpecialty",
            input_args={"specialty": specialty, "date": date_str},
            output=spec_tool_res,
            execution_time_ms=spec_tool_res.get("duration_ms", 15.0),
            status="Success"
        ))
        plan_steps[1].status = "Completed"
        plan_steps[2].status = "Executing"

        matching_docs = spec_tool_res.get("matching_doctors", [])
        if matching_docs:
            selected_doc = matching_docs[0]
        else:
            spec_k = specialty.lower().strip()
            selected_doc = SPECIALTY_DOCTOR_MAP.get(spec_k, SPECIALTY_DOCTOR_MAP["cardiology"])
        if input_data.target_doctor_id:
            for d in matching_docs:
                if d.get("id") == input_data.target_doctor_id:
                    selected_doc = d
                    break
        
        doc_id = selected_doc.get("id", 1)
        doc_name = selected_doc.get("doctorName", f"Doctor #{doc_id}")
        doc_spec = selected_doc.get("specialtyName", specialty)

        # --- STEP 3: Tool 4 - CalculateDoctorWorkloadAndPriority ---
        workload_tool_res = await self.tools.calculate_doctor_workload_and_priority(doc_id, urgency_score)
        tool_logs.append(ToolCallLog(
            tool_name="CalculateDoctorWorkloadAndPriority",
            input_args={"doctorId": doc_id, "urgencyScore": urgency_score},
            output=workload_tool_res,
            execution_time_ms=workload_tool_res.get("duration_ms", 12.0),
            status="Success"
        ))
        plan_steps[2].status = "Completed"
        plan_steps[3].status = "Executing"

        target_room_id = input_data.preferred_room_id if input_data.preferred_room_id else 1
        rec_start = input_data.desired_window_start
        duration_mins = input_data.estimated_duration_minutes or 120
        rec_end = input_data.desired_window_end if input_data.desired_window_end else rec_start + timedelta(minutes=duration_mins)

        # Schedule Check: Search for active Doctor Schedules in DB matching specialty
        existing_scheds = spec_tool_res.get("existing_schedules", [])
        spec_k = specialty.lower().strip()

        matching_scheds = []
        for s in existing_scheds:
            s_spec = (s.get("specialtyName") or "").lower()
            s_doc_id = s.get("doctorId")
            if spec_k in s_spec or s_spec in spec_k or s_doc_id == doc_id:
                matching_scheds.append(s)

        matched_sched = None
        if matching_scheds:
            try:
                matching_scheds.sort(key=lambda s: datetime.fromisoformat(s.get("startTime").replace("Z", "+00:00")))
            except Exception:
                pass
            matched_sched = matching_scheds[0]
        elif len(existing_scheds) > 0:
            matched_sched = existing_scheds[0]

        has_active_schedule = matched_sched is not None

        if matched_sched:
            doc_id = matched_sched.get("doctorId", doc_id)
            doc_name = matched_sched.get("doctorName", doc_name)
            doc_spec = matched_sched.get("specialtyName", doc_spec)
            target_room_id = matched_sched.get("roomId", target_room_id)
            try:
                rec_start = datetime.fromisoformat(matched_sched.get("startTime").replace("Z", "+00:00"))
                rec_end = datetime.fromisoformat(matched_sched.get("endTime").replace("Z", "+00:00"))
            except Exception:
                pass

        # --- STEP 4: Tool 3 - CheckRoomAvailability ---
        room_tool_res = await self.tools.check_room_availability(
            target_room_id,
            rec_start.isoformat(),
            rec_end.isoformat()
        )
        tool_logs.append(ToolCallLog(
            tool_name="CheckRoomAvailability",
            input_args={"roomId": target_room_id, "startTime": rec_start.isoformat(), "endTime": rec_end.isoformat()},
            output=room_tool_res,
            execution_time_ms=room_tool_res.get("duration_ms", 18.0),
            status="Success"
        ))
        plan_steps[3].status = "Completed"
        plan_steps[4].status = "Executing"

        is_slot_free = room_tool_res.get("is_slot_free", True)

        # --- STEP 5: Deterministic Business Rule Validation ---
        # Rule 1: DoctorScheduleAvailable
        validations.append(ValidationRuleResult(
            rule_name="DoctorScheduleAvailable",
            passed=has_active_schedule,
            details=f"Active doctor schedule session found in database for {doc_name} ({doc_spec})." if has_active_schedule else f"No active doctor schedule session exists in database for specialty '{specialty}'. Please add a schedule session.",
            severity="Error"
        ))

        # Rule 2: SpecialtyMatch
        is_spec_match = specialty.lower() in doc_spec.lower() or doc_spec.lower() in specialty.lower()
        validations.append(ValidationRuleResult(
            rule_name="SpecialtyMatch",
            passed=is_spec_match,
            details=f"Doctor specialty ({doc_spec}) matches Triage Assessment recommended specialty ({specialty}).",
            severity="Error"
        ))

        # Rule 3: PriorityPlacementRank
        priority_rank = workload_tool_res.get("priority_rank", "Priority Level 1")
        validations.append(ValidationRuleResult(
            rule_name="PriorityPlacementRank",
            passed=True,
            details=f"Appointment #{appt_id} assigned under '{priority_rank}' based on Triage Urgency Score {urgency_score}.",
            severity="Warning"
        ))

        plan_steps[4].status = "Completed"

        all_passed = all(v.passed for v in validations if v.severity == "Error")
        score = 98.5 if all_passed else 0.0

        recommended_option = None
        if has_active_schedule and all_passed:
            recommended_option = RecommendedOption(
                appointment_id=appt_id,
                doctor_id=doc_id,
                doctor_name=doc_name,
                specialty_name=doc_spec,
                schedule_id=matched_sched.get("id", 101) if matched_sched else 101,
                recommended_start_time=rec_start,
                recommended_end_time=rec_end,
                allocated_room_id=target_room_id,
                allocated_room_name=room_tool_res.get("room_name", f"Room {target_room_id}"),
                floor=room_tool_res.get("floor", "1st Floor"),
                max_patients=input_data.max_patients,
                urgency_level=urgency_level,
                priority_rank=priority_rank,
                conflict_free_score=score
            )

        workflow_state = WorkflowState(
            workflow_id=workflow_id,
            objective=f"Assign Appointment #{appt_id} to {doc_name} ({specialty}) [Urgency: {urgency_level} - Score: {urgency_score}]",
            created_at=datetime.utcnow(),
            current_step="AwaitingHumanApproval" if all_passed else "ValidationFailed",
            plan=plan_steps,
            tool_execution_logs=tool_logs,
            deterministic_validations=validations,
            recommended_option=recommended_option,
            approval_status=ApprovalStatus.PENDING_APPROVAL if all_passed else ApprovalStatus.FAILED
        )

        WORKFLOW_DB[workflow_id] = workflow_state
        return workflow_state

    @staticmethod
    def approve_workflow(workflow_id: str) -> Optional[WorkflowState]:
        if workflow_id not in WORKFLOW_DB:
            return None
        state = WORKFLOW_DB[workflow_id]
        if state.approval_status != ApprovalStatus.PENDING_APPROVAL:
            return state
        state.approval_status = ApprovalStatus.APPROVED
        state.current_step = "ApprovedByStaff"
        return state

    @staticmethod
    def reject_workflow(workflow_id: str, reason: str = "Rejected by authorized user") -> Optional[WorkflowState]:
        if workflow_id not in WORKFLOW_DB:
            return None
        state = WORKFLOW_DB[workflow_id]
        state.approval_status = ApprovalStatus.REJECTED
        state.rejection_reason = reason
        state.current_step = "RejectedByStaff"
        return state
