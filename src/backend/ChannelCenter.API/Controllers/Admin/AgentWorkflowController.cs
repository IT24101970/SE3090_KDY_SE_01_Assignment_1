using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.Models;
using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public class AgentWorkflowsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public AgentWorkflowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/workflows
    [HttpGet]
    public async Task<IActionResult> GetWorkflows()
    {
        return Ok(new {message = "Workflows endpoint is working!"});
    }

    // GET: api/workflows/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllWorkflows()
    {
        var workflows = await _context.AgentWorkflows
            .Include(w => w.AuditLogs)
            .ToListAsync();

        var workflowDtos = workflows.Select(w => new AgentWorkflowResponseDto
        {
            Id = w.Id,
            Objective = w.Objective,
            Status = w.Status,
            RequiresHumanApproval = w.RequiresHumanApproval,
            CreatedAt = w.CreatedAt,
            // AuditLogs = w.AuditLogs.Select(a => new WorkflowAuditResponseDto
            // {
            //     Id = a.Id,
            //     AgentName = a.AgentName,
            //     ToolCalled = a.ToolCalled,
            //     ToolOutput = a.ToolOutput,
            //     CreatedAt = a.CreatedAt
            // }).ToList()
        }).ToList();

        return Ok(workflowDtos);
    }
    
    // GET: api/workflows/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflowById(int id)
    {
        var workflow = await _context.AgentWorkflows
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
            AuditLogs = workflow.AuditLogs.Select(a => new WorkflowAuditResponseDto
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
    
    // POST: api/workflows/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveWorkflow(int id, [FromBody] WorkflowApprovalRequestDto request)
    {
        var workflow = await _context.AgentWorkflows.FindAsync(id);
    
        if (workflow == null) 
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        // 1. Create the new approval log entry
        var approval = new AdminApproval
        {
            WorkflowId = id,
            AdminUserId = 1, // TODO: Replace with actual admin user ID from authentication context
            Decision = request.Decision,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AdminApprovals.Add(approval);

        if (request.Decision == ApprovalDecision.Approved)
        {
            workflow.Status = WorkflowStatus.Running; 
        }
        else if (request.Decision == ApprovalDecision.Rejected)
        {
            workflow.Status = WorkflowStatus.Terminated; 
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

