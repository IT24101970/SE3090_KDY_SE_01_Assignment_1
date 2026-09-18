using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.Models;
using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public class AdminOverrideController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminOverrideController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/adminOverrides/workflows/{id}/cancel
    [HttpPost("workflows/{id}/cancel")]
    public async Task<IActionResult> CancelWorkflow(int id, [FromBody] OverrideCancelRequestDto request)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        if (workflow.Status == WorkflowStatus.Completed)
        {
            return BadRequest(new { message = "Cannot cancel an already completed workflow." });
        }

        // Log the manual cancellation as an audit entry
        var auditEntry = new AuditLog
        {
            Id = workflow.Id,
            AgentName = "SystemAdmin",
            ToolCalled = "ManualOverride_Cancel",
            ToolOutput = $"Admin cancelled workflow. Reason: {request.Reason}",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.AuditLogs.Add(auditEntry);

        // Terminate the workflow
        workflow.Status = WorkflowStatus.Terminated;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = "Workflow successfully cancelled.", 
            workflowId = workflow.Id, 
            status = workflow.Status 
        });
    }

    // POST: api/adminOverrides/workflows/{id}/reassign
    [HttpPost("workflows/{id}/reassign")]
    public async Task<IActionResult> ReassignWorkflow(int id, [FromBody] OverrideReassignRequestDto request)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        var auditEntry = new AuditLog
        {
            Id = workflow.Id,
            AgentName = "SystemAdmin",
            ToolCalled = "ManualOverride_Reassign",
            ToolOutput = $"Admin reassigned to Doctor ID {request.TargetDoctorId}. Reason: {request.Reason}",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.AuditLogs.Add(auditEntry);

        // Mark the AI workflow as completed/bypassed since a human is taking over
        workflow.Status = WorkflowStatus.Completed;
        workflow.UpdatedAt = DateTime.UtcNow;

        // Note: In a full system, you would also create a new record in the Appointments/Triage 
        // table here linking the patient to request.TargetDoctorId.

        await _context.SaveChangesAsync();

        return Ok(new { 
            message = $"Workflow bypassed and successfully reassigned to Doctor {request.TargetDoctorId}.",
            workflowId = workflow.Id
        });
    }
}