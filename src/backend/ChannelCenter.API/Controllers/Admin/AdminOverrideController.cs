using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Services.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/admin/overrides")]
[Authorize(Roles = "Admin")]
public class AdminOverrideController : ControllerBase
{
    private readonly IAdminOverrideService _overrideService;

    [ActivatorUtilitiesConstructor]
    public AdminOverrideController(IAdminOverrideService overrideService)
    {
        _overrideService = overrideService;
    }

    // Convenience constructor for unit tests
    public AdminOverrideController(ApplicationDbContext context)
        : this(new AdminOverrideService(context))
    {
    }

    // POST: api/admin/overrides/workflows/{id}/cancel
    [HttpPost("workflows/{id}/cancel")]
    public async Task<IActionResult> CancelWorkflow(int id, [FromBody] OverrideCancelRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var claimValue = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int adminUserId = int.TryParse(claimValue, out var parsedId) ? parsedId : 1;

        var (success, errorMessage, workflowId) = await _overrideService.CancelWorkflowAsync(id, request, adminUserId);

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
            message = "Workflow successfully cancelled.", 
            workflowId = workflowId 
        });
    }

    // POST: api/admin/overrides/workflows/{id}/reassign
    [HttpPost("workflows/{id}/reassign")]
    public async Task<IActionResult> ReassignWorkflow(int id, [FromBody] OverrideReassignRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var claimValue = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int adminUserId = int.TryParse(claimValue, out var parsedId) ? parsedId : 1;

        var (success, errorMessage, workflowId) = await _overrideService.ReassignWorkflowAsync(id, request, adminUserId);

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
            message = $"Workflow bypassed and successfully reassigned to Doctor {request.TargetDoctorId}.",
            workflowId = workflowId
        });
    }
}