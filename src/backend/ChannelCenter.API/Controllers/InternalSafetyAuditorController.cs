using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Services.Admin;
using ChannelCenter.API.Services.SafetyAuditor;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/internal/v1/safety-auditor")]
public class InternalSafetyAuditorController : ControllerBase
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";
    private readonly ISafetyAuditorService _safetyAuditorService;
    private readonly IAgentWorkflowService _workflowService;
    private readonly ISafetyAuditorReadService? _readService;
    private readonly SafetyAuditorOptions _options;

    [ActivatorUtilitiesConstructor]
    public InternalSafetyAuditorController(
        ISafetyAuditorService safetyAuditorService,
        IAgentWorkflowService workflowService,
        ISafetyAuditorReadService readService,
        IOptions<SafetyAuditorOptions> options)
    {
        _safetyAuditorService = safetyAuditorService;
        _workflowService = workflowService;
        _readService = readService;
        _options = options.Value;
    }

    public InternalSafetyAuditorController(
        ISafetyAuditorService safetyAuditorService,
        IAgentWorkflowService workflowService,
        IOptions<SafetyAuditorOptions> options)
        : this(safetyAuditorService, workflowService, null!, options)
    {
    }

    [HttpGet("appointments/{appointmentId:int}/context")]
    public async Task<IActionResult> GetAppointmentContext(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (_readService == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var context = await _readService.GetAppointmentContextAsync(appointmentId, cancellationToken);
        return context == null ? NotFound() : Ok(context);
    }

    [HttpPost("workflows/{workflowId:int}/start")]
    public async Task<IActionResult> Start(
        int workflowId,
        [FromBody] SafetyAuditStartRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var response = await _safetyAuditorService.StartAsync(workflowId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("workflows/{workflowId:int}/callback")]
    public async Task<IActionResult> Callback(
        int workflowId,
        [FromBody] SafetyAuditCallbackDto response,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var result = await _safetyAuditorService.ApplyCallbackAsync(
            workflowId,
            response,
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("workflows/{workflowId:int}/pause")]
    public async Task<IActionResult> Pause(
        int workflowId,
        [FromBody] PauseWorkflowRequestDto request)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var (success, error) = await _workflowService.PauseWorkflowAsync(workflowId, request);
        if (!success)
        {
            return BadRequest(new { message = error });
        }

        return Ok(new { workflowId, status = "PausedForApproval" });
    }

    private bool IsAuthorized()
    {
        if (string.IsNullOrWhiteSpace(_options.InternalServiceKey) ||
            !Request.Headers.TryGetValue(ServiceKeyHeader, out var provided))
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(_options.InternalServiceKey);
        var actual = Encoding.UTF8.GetBytes(provided.ToString());
        return expected.Length == actual.Length &&
               CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
