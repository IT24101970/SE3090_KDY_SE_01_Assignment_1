using System.Text.Json;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Admin;

public class AdminOverrideService : IAdminOverrideService
{
    private readonly ApplicationDbContext _context;

    public AdminOverrideService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, int? WorkflowId)> CancelWorkflowAsync(
        int id, OverrideCancelRequestDto request, int adminUserId)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return (false, $"Workflow with ID {id} not found.", null);
        }

        if (workflow.Status == WorkflowStatus.Completed || workflow.Status == WorkflowStatus.Terminated)
        {
            return (false, $"Cannot cancel a workflow that is already {workflow.Status}.", null);
        }

        var auditEntry = new AuditLog
        {
            WorkflowId = workflow.Id,
            AgentName = "SystemAdmin",
            ToolCalled = "ManualOverride_Cancel",
            ToolOutput = JsonSerializer.Serialize(new
            {
                action = "ManualOverride_Cancel",
                workflowId = workflow.Id,
                adminUserId = adminUserId,
                reason = request.Reason,
                timestamp = DateTime.UtcNow
            }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditEntry);

        workflow.Status = WorkflowStatus.Terminated;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, null, workflow.Id);
    }

    public async Task<(bool Success, string? ErrorMessage, int? WorkflowId)> ReassignWorkflowAsync(
        int id, OverrideReassignRequestDto request, int adminUserId)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return (false, $"Workflow with ID {id} not found.", null);
        }

        if (workflow.Status == WorkflowStatus.Completed)
        {
            return (false, "Cannot reassign an already completed workflow.", null);
        }

        var auditEntry = new AuditLog
        {
            WorkflowId = workflow.Id,
            AgentName = "SystemAdmin",
            ToolCalled = "ManualOverride_Reassign",
            ToolOutput = JsonSerializer.Serialize(new
            {
                action = "ManualOverride_Reassign",
                workflowId = workflow.Id,
                adminUserId = adminUserId,
                targetDoctorId = request.TargetDoctorId,
                reason = request.Reason,
                timestamp = DateTime.UtcNow
            }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditEntry);

        workflow.Status = WorkflowStatus.Completed;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, null, workflow.Id);
    }
}
