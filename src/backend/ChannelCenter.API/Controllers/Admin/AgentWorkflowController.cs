using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/admin/workflows")]
[Authorize(Roles = "Admin")]
public class AgentWorkflowsController : ControllerBase
{
    private readonly IAgentWorkflowService _workflowService;
    
    public AgentWorkflowsController(IAgentWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    // Convenience constructor for tests utilizing in-memory DbContext directly
    // public AgentWorkflowsController(ApplicationDbContext context)
    //     : this(new AgentWorkflowService(context))
    // {
    // }

    // GET: api/admin/workflows (optionally filter by ?status=PausedForApproval)
    [HttpGet]
    public async Task<IActionResult> GetWorkflows([FromQuery] WorkflowStatus? status = null)
    {
        var workflows = await _workflowService.GetWorkflowsAsync(status);
        return Ok(workflows);
    }

    // Kept for backward compatibility
    [HttpGet("all")]
    public Task<IActionResult> GetAllWorkflows([FromQuery] WorkflowStatus? status = null) => GetWorkflows(status);
    
    // GET: api/admin/workflows/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflowById(int id)
    {
        var workflow = await _workflowService.GetWorkflowByIdAsync(id);

        if (workflow == null) 
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        return Ok(workflow);
    }

    // POST: api/admin/workflows
    [HttpPost]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var workflow = await _workflowService.CreateWorkflowAsync(request);
        return CreatedAtAction(nameof(GetWorkflowById), new { id = workflow.Id }, workflow);
    }

    // POST: api/admin/workflows/{id}/pause
    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseWorkflow(int id, [FromBody] PauseWorkflowRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage) = await _workflowService.PauseWorkflowAsync(id, request);
        if (!success)
        {
            if (errorMessage != null && errorMessage.Contains("not found"))
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new 
        { 
            message = "Workflow successfully paused for human review.", 
            workflowId = id 
        });
    }
    
    // POST: api/admin/workflows/{id}/approve
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveWorkflow(int id, [FromBody] WorkflowApprovalRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract Admin User ID from JWT Claim, fallback to request/default for dev/testing
        var claimValue = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int adminUserId = int.TryParse(claimValue, out var parsedId) ? parsedId : (request.AdminUserId ?? 1);

        var (success, errorMessage, response) = await _workflowService.ApproveWorkflowAsync(id, request, adminUserId);

        if (!success)
        {
            if (errorMessage != null && errorMessage.Contains("not found"))
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(response);
    }
}
