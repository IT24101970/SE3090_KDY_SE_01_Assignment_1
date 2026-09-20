using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.Models;
using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/admin/workflows")]
//[Route("api/[controller]")]
public class AgentWorkflowsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AgentWorkflowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/admin/workflows (optionally filter by ?status=PausedForApproval)
    [HttpGet]
    public async Task<IActionResult> GetWorkflows([FromQuery] WorkflowStatus? status = null)
    {
        var query = _context.AgentWorkflows.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        // Direct projection: only selects columns needed, avoiding loading AuditLogs into memory
        var workflowDtos = await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new AgentWorkflowResponseDto
            {
                Id = w.Id,
                Objective = w.Objective,
                Status = w.Status,
                RequiresHumanApproval = w.RequiresHumanApproval,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();

        return Ok(workflowDtos);
    }

    // Kept for backward compatibility
    [HttpGet("all")]
    public Task<IActionResult> GetAllWorkflows([FromQuery] WorkflowStatus? status = null) => GetWorkflows(status);
    
    // GET: api/admin/workflows/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflowById(int id)
    {
        var workflow = await _context.AgentWorkflows
            .AsNoTracking()
            .Include(w => w.AuditLogs)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workflow == null) 
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        var workflowDto = new AgentWorkflowResponseDto
        {
            Id = workflow.Id,
            Objective = workflow.Objective,
            Status = workflow.Status,
            RequiresHumanApproval = workflow.RequiresHumanApproval,
            CreatedAt = workflow.CreatedAt,
            AuditLogs = workflow.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new WorkflowAuditResponseDto
                {
                    Id = a.Id,
                    AgentName = a.AgentName,
                    ToolCalled = a.ToolCalled,
                    ToolOutput = a.ToolOutput,
                    CreatedAt = a.CreatedAt
                }).ToList()
        };

        return Ok(workflowDto);
    }
    
    // POST: api/admin/workflows/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveWorkflow(int id, [FromBody] WorkflowApprovalRequestDto request)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);
    
        if (workflow == null) 
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        // Validate state transitions: Cannot approve or reject already finished workflows
        if (workflow.Status == WorkflowStatus.Completed || workflow.Status == WorkflowStatus.Terminated)
        {
            return BadRequest(new { message = $"Cannot make approval decisions on a workflow that is already {workflow.Status}." });
        }

        // Resolve AdminUserId safely from request or database
        int adminUserId = request.AdminUserId ?? 1;
        if (!request.AdminUserId.HasValue)
        {
            var adminUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Role == UserRole.Admin)
                            ?? await _context.Users.AsNoTracking().FirstOrDefaultAsync();
            if (adminUser != null)
            {
                adminUserId = adminUser.Id;
            }
        }

        // 1. Create the new approval log entry
        var approval = new AdminApproval
        {
            WorkflowId = id,
            AdminUserId = adminUserId,
            Decision = request.Decision,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AdminApprovals.Add(approval);

        // 2. Transition workflow state based on decision
        if (request.Decision == ApprovalDecision.Approved)
        {
            workflow.Status = WorkflowStatus.Running; 
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

        return Ok(responseDto);
    }
}

