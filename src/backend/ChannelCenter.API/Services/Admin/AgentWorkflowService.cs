using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.SafetyAuditor;

namespace ChannelCenter.API.Services.Admin;

public class AgentWorkflowService : IAgentWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly ISafetyAuditorService? _safetyAuditorService;

    public AgentWorkflowService(
        ApplicationDbContext context,
        ISafetyAuditorService? safetyAuditorService = null)
    {
        _context = context;
        _safetyAuditorService = safetyAuditorService;
    }

    public async Task<IEnumerable<AgentWorkflowResponseDto>> GetWorkflowsAsync(WorkflowStatus? status = null)
    {
        var query = _context.AgentWorkflows.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new AgentWorkflowResponseDto
            {
                Id = w.Id,
                Objective = w.Objective,
                Status = w.Status,
                RequiresHumanApproval = w.RequiresHumanApproval,
                CorrelationId = w.CorrelationId,
                ContractVersion = w.ContractVersion,
                RiskLevel = w.RiskLevel,
                PlanSummary = w.PlanSummary,
                ValidationSummary = w.ValidationSummary,
                FinalOutcome = w.FinalOutcome,
                ErrorCode = w.ErrorCode,
                ErrorMessage = w.ErrorMessage,
                AppointmentId = w.AppointmentId,
                CompletedAt = w.CompletedAt,
                SafeFailedAt = w.SafeFailedAt,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AgentWorkflowResponseDto?> GetWorkflowByIdAsync(int id)
    {
        var workflow = await _context.AgentWorkflows
            .AsNoTracking()
            .Include(w => w.AuditLogs)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
        {
            return null;
        }

        return new AgentWorkflowResponseDto
        {
            Id = workflow.Id,
            Objective = workflow.Objective,
            Status = workflow.Status,
            RequiresHumanApproval = workflow.RequiresHumanApproval,
            CorrelationId = workflow.CorrelationId,
            ContractVersion = workflow.ContractVersion,
            RiskLevel = workflow.RiskLevel,
            PlanSummary = workflow.PlanSummary,
            ValidationSummary = workflow.ValidationSummary,
            FinalOutcome = workflow.FinalOutcome,
            ErrorCode = workflow.ErrorCode,
            ErrorMessage = workflow.ErrorMessage,
            AppointmentId = workflow.AppointmentId,
            CompletedAt = workflow.CompletedAt,
            SafeFailedAt = workflow.SafeFailedAt,
            CreatedAt = workflow.CreatedAt,
            AuditLogs = workflow.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new WorkflowAuditResponseDto
                {
                    Id = a.Id,
                    AgentName = a.AgentName,
                    ToolCalled = a.ToolCalled,
                    ToolOutput = a.ToolOutput,
                    StepName = a.StepName,
                    CorrelationId = a.CorrelationId,
                    ContractVersion = a.ContractVersion,
                    Outcome = a.Outcome,
                    DurationMs = a.DurationMs,
                    CreatedAt = a.CreatedAt
                }).ToList()
        };
    }

    public async Task<AgentWorkflowResponseDto> CreateWorkflowAsync(CreateWorkflowRequestDto request)
    {
        var workflow = new AgentWorkflow
        {
            Objective = request.Objective,
            Status = WorkflowStatus.Running,
            RequiresHumanApproval = request.RequiresHumanApproval,
            CorrelationId = string.IsNullOrWhiteSpace(request.CorrelationId)
                ? Guid.NewGuid().ToString("N")
                : request.CorrelationId,
            ContractVersion = request.ContractVersion,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AgentWorkflows.Add(workflow);
        await _context.SaveChangesAsync();

        return new AgentWorkflowResponseDto
        {
            Id = workflow.Id,
            Objective = workflow.Objective,
            Status = workflow.Status,
            RequiresHumanApproval = workflow.RequiresHumanApproval,
            CorrelationId = workflow.CorrelationId,
            ContractVersion = workflow.ContractVersion,
            CreatedAt = workflow.CreatedAt
        };
    }

    public async Task<(bool Success, string? ErrorMessage, WorkflowApprovalResponseDto? Response)> ApproveWorkflowAsync(
        int id, WorkflowApprovalRequestDto request, int adminUserId)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return (false, $"Workflow with ID {id} not found.", null);
        }

        if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Terminated or WorkflowStatus.SafeFailed)
        {
            return (false, $"Cannot make approval decisions on a workflow that is already {workflow.Status}.", null);
        }

        var approval = new AdminApproval
        {
            WorkflowId = id,
            AdminUserId = adminUserId,
            Decision = request.Decision,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AdminApprovals.Add(approval);

        if (request.Decision == ApprovalDecision.Approved)
        {
            workflow.Status = WorkflowStatus.Completed;
            workflow.CompletedAt = DateTime.UtcNow;
            workflow.RequiresHumanApproval = false;
        }
        else if (request.Decision == ApprovalDecision.Rejected)
        {
            workflow.Status = WorkflowStatus.Terminated;
        }
        else if (request.Decision == ApprovalDecision.Revised)
        {
            workflow.Status = WorkflowStatus.PausedForApproval;
        }

        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var responseDto = new WorkflowApprovalResponseDto
        {
            Id = approval.Id,
            WorkflowId = workflow.Id,
            Decision = approval.Decision,
            UpdatedWorkflowStatus = workflow.Status,
            ProcessedAt = approval.CreatedAt
        };

        return (true, null, responseDto);
    }

    public async Task<(bool Success, string? ErrorMessage)> PauseWorkflowAsync(int id, PauseWorkflowRequestDto request)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);

        if (workflow == null)
        {
            return (false, $"Workflow with ID {id} not found.");
        }

        if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Terminated or WorkflowStatus.SafeFailed)
        {
            return (false, $"Cannot pause a workflow that is already {workflow.Status}.");
        }

        workflow.Status = WorkflowStatus.PausedForApproval;
        workflow.RequiresHumanApproval = true;
        workflow.CorrelationId = request.CorrelationId ?? workflow.CorrelationId;
        workflow.ContractVersion = request.ContractVersion;
        workflow.RiskLevel = request.RiskLevel;
        workflow.UpdatedAt = DateTime.UtcNow;

        var alreadyRecorded = request.CorrelationId != null &&
            await _context.AuditLogs.AnyAsync(a =>
                a.WorkflowId == workflow.Id &&
                a.CorrelationId == request.CorrelationId &&
                a.ToolCalled == "SafetyAuditor_PauseAction");

        if (alreadyRecorded)
        {
            return (true, null);
        }

        var auditLog = new AuditLog
        {
            WorkflowId = workflow.Id,
            AgentName = request.AgentName,
            ToolCalled = "SafetyAuditor_PauseAction",
            ToolOutput = JsonSerializer.Serialize(new
            {
                action = "SafetyAuditor_PauseAction",
                workflowId = workflow.Id,
                reason = request.Reason,
                violation = request.ValidationViolation,
                timestamp = DateTime.UtcNow
            }),
            CorrelationId = request.CorrelationId,
            ContractVersion = request.ContractVersion,
            StepName = "PauseOrComplete",
            Outcome = "PausedForApproval",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, AgentWorkflowResponseDto? Response)> RestartWorkflowAsync(
        int id,
        RestartWorkflowRequestDto request,
        int adminUserId)
    {
        var workflow = await _context.AgentWorkflows
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null)
        {
            return (false, $"Workflow with ID {id} not found.", null);
        }

        if (workflow.Status != WorkflowStatus.PausedForApproval)
        {
            return (false, "Only paused workflows can be restarted.", null);
        }

        workflow.Status = WorkflowStatus.Running;
        workflow.RequiresHumanApproval = false;
        workflow.ErrorCode = null;
        workflow.ErrorMessage = null;
        workflow.CompletedAt = null;
        workflow.SafeFailedAt = null;
        workflow.CorrelationId = $"{workflow.CorrelationId}-restart-{Guid.NewGuid():N}";
        workflow.UpdatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(new AuditLog
        {
            WorkflowId = workflow.Id,
            AgentName = "Admin",
            ToolCalled = "Admin_RestartWorkflow",
            StepName = "Restart",
            CorrelationId = workflow.CorrelationId,
            ContractVersion = workflow.ContractVersion,
            Outcome = $"Restarted by admin {adminUserId}",
            ToolOutput = JsonSerializer.Serialize(new { request.Reason }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        if (_safetyAuditorService != null && workflow.AppointmentId.HasValue)
        {
            var auditRequest = new SafetyAuditStartRequestDto
            {
                Objective = workflow.Objective,
                CorrelationId = workflow.CorrelationId,
                ContractVersion = workflow.ContractVersion,
                SourceAgent = "Admin.RestartWorkflow",
                AppointmentId = workflow.AppointmentId,
                Proposal = new Dictionary<string, object?>
                {
                    ["action"] = "review_appointment",
                    ["appointmentId"] = workflow.AppointmentId.Value,
                    ["evidence"] = new[] { $"appointment:{workflow.AppointmentId.Value}" }
                }
            };
            await _safetyAuditorService.StartAsync(workflow.Id, auditRequest);
        }

        return (true, null, await GetWorkflowByIdAsync(id));
    }
}
