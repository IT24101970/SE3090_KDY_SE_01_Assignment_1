from fastapi import APIRouter, HTTPException, Path, Body
from typing import Dict, Any
from pydantic import BaseModel

from .schemas import OptimizationInput, WorkflowState
from .agent import ScheduleOptimizationAgent, WORKFLOW_DB

router = APIRouter(prefix="/api/agent/doctor-scheduling", tags=["Doctor Scheduling AI Agent"])
agent_engine = ScheduleOptimizationAgent()

class RejectRequest(BaseModel):
    reason: str = "Rejected by authorized staff"

@router.post("/optimize", response_model=WorkflowState)
async def run_schedule_optimization(input_data: OptimizationInput):
    """
    Triggers the Schedule & Capacity Optimization Agent workflow:
    1. Multi-step plan execution
    2. Runs allow-listed tools (GetDoctorSchedules, CheckRoomAvailability, CalculateDoctorWorkload)
    3. Deterministic business rule validation
    4. Computes conflict-free score & recommended option
    5. Pauses in PendingApproval state for Human-in-the-Loop review.
    """
    try:
        workflow_state = await agent_engine.execute_optimization_workflow(input_data)
        return workflow_state
    except Exception as ex:
        raise HTTPException(status_code=500, detail=f"Agent workflow failure: {str(ex)}")

@router.get("/workflows/{workflow_id}", response_model=WorkflowState)
async def get_workflow_state(workflow_id: str = Path(..., description="ID of the persistent workflow")):
    """Retrieves shared workflow state and auditable execution logs."""
    if workflow_id not in WORKFLOW_DB:
        raise HTTPException(status_code=404, detail=f"Workflow {workflow_id} not found.")
    return WORKFLOW_DB[workflow_id]

@router.post("/workflows/{workflow_id}/approve", response_model=WorkflowState)
async def approve_workflow(workflow_id: str):
    """Human-in-the-Loop Action: Approve recommended schedule option."""
    updated_state = ScheduleOptimizationAgent.approve_workflow(workflow_id)
    if not updated_state:
        raise HTTPException(status_code=404, detail=f"Workflow {workflow_id} not found.")
    return updated_state

@router.post("/workflows/{workflow_id}/reject", response_model=WorkflowState)
async def reject_workflow(workflow_id: str, body: RejectRequest = Body(...)):
    """Human-in-the-Loop Action: Reject recommended schedule option with reason."""
    updated_state = ScheduleOptimizationAgent.reject_workflow(workflow_id, body.reason)
    if not updated_state:
        raise HTTPException(status_code=404, detail=f"Workflow {workflow_id} not found.")
    return updated_state
