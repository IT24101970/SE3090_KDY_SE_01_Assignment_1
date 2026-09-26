using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.Services.IntakeAgent;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/intake-agent")]
public class IntakeAgentController : ControllerBase
{
    private readonly IIntakeAgentService _intakeAgentService;

    public IntakeAgentController(IIntakeAgentService intakeAgentService)
    {
        _intakeAgentService = intakeAgentService;
    }

    // POST: api/intake-agent/process
    // Primary agent orchestrator endpoint: Transforms unstructured patient text into a structured execution package
    [HttpPost("process")]
    public async Task<IActionResult> ProcessIntake([FromBody] IntakeProcessRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _intakeAgentService.ProcessIntakeAsync(request);
        if (!success)
        {
            if (errorMessage != null && errorMessage.Contains("not found"))
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(data);
    }

    // POST: api/intake-agent/tools/patient-history
    // Allow-Listed Tool 1: GetPatientHistory(patientId)
    [HttpPost("tools/patient-history")]
    public async Task<IActionResult> ToolGetPatientHistory([FromBody] ToolPatientIdRequest request)
    {
        if (request.PatientId <= 0)
        {
            return BadRequest(new { message = "A valid patientId is required." });
        }

        var (success, errorMessage, resultJson) = await _intakeAgentService.ToolGetPatientHistoryAsync(request.PatientId);
        if (!success)
        {
            return NotFound(new { message = errorMessage });
        }

        return Content(resultJson, "application/json");
    }

    // POST: api/intake-agent/tools/format-summary
    // Allow-Listed Tool 2: FormatIntakeSummary(rawText)
    [HttpPost("tools/format-summary")]
    public async Task<IActionResult> ToolFormatIntakeSummary([FromBody] ToolRawTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawText))
        {
            return BadRequest(new { message = "rawText is required." });
        }

        var (success, errorMessage, resultJson) = await _intakeAgentService.ToolFormatIntakeSummaryAsync(request.RawText);
        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Content(resultJson, "application/json");
    }

    // POST: api/intake-agent/tools/validate-eligibility
    // Allow-Listed Tool 3: ValidatePatientEligibility(patientId)
    [HttpPost("tools/validate-eligibility")]
    public async Task<IActionResult> ToolValidatePatientEligibility([FromBody] ToolPatientIdRequest request)
    {
        if (request.PatientId <= 0)
        {
            return BadRequest(new { message = "A valid patientId is required." });
        }

        var (_, _, resultJson) = await _intakeAgentService.ToolValidatePatientEligibilityAsync(request.PatientId);
        return Content(resultJson, "application/json");
    }

    // GET: api/intake-agent/logs/{patientId}
    // Inspect agent execution and tool audit traces for a specific patient
    [HttpGet("logs/{patientId:int}")]
    public async Task<IActionResult> GetLogsByPatient(int patientId)
    {
        var logs = await _intakeAgentService.GetLogsByPatientIdAsync(patientId);
        return Ok(logs);
    }

    // GET: api/intake-agent/logs?page=1&pageSize=20
    [HttpGet("logs")]
    public async Task<IActionResult> GetAllLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var logs = await _intakeAgentService.GetAllLogsAsync(page, pageSize);
        return Ok(logs);
    }
}

public class ToolPatientIdRequest
{
    public int PatientId { get; set; }
}

public class ToolRawTextRequest
{
    public string RawText { get; set; } = string.Empty;
}