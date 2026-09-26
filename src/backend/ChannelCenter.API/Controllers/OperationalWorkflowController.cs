using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Admin;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize]
public class OperationalWorkflowController : ControllerBase
{
    private readonly IAgentWorkflowService _workflowService;

    public OperationalWorkflowController(IAgentWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Operational endpoint to get emergency & high-risk workflows for patients, staff, and doctors.
    /// Accessible to any authenticated user.
    /// </summary>
    [HttpGet("emergency")]
    public async Task<IActionResult> GetEmergencyWorkflows()
    {
        var allWorkflows = await _workflowService.GetWorkflowsAsync();
        
        // Filter workflows with high/emergency risk or paused for approval/safety issues
        var emergencyWorkflows = allWorkflows.Where(w =>
            string.Equals(w.RiskLevel, "Emergency", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(w.RiskLevel, "High", StringComparison.OrdinalIgnoreCase) ||
            w.Status == WorkflowStatus.PausedForApproval ||
            w.Status == WorkflowStatus.SafeFailed
        ).OrderByDescending(w => w.CreatedAt);

        return Ok(emergencyWorkflows);
    }

    /// <summary>
    /// Operational endpoint to inspect status of a single workflow.
    /// Accessible to any authenticated user.
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetWorkflowStatus(int id)
    {
        var workflow = await _workflowService.GetWorkflowByIdAsync(id);
        if (workflow == null)
        {
            return NotFound(new { message = $"Workflow with ID {id} not found." });
        }

        return Ok(workflow);
    }
}
