using ChannelCenter.API.DTOs.Triage;
using ChannelCenter.API.Services.Triage;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.Triage;

[ApiController]
[Route("api/[controller]")]
public class TriageController : ControllerBase
{
    private readonly ITriageService _triageService;

    public TriageController(ITriageService triageService)
    {
        _triageService = triageService;
    }

    // ── Questionnaires ─────────────────────────────────────────────────────────

    /// <summary>
    /// Submits a pre-consultation intake questionnaire from Flutter frontend.
    /// </summary>
    [HttpPost("questionnaires")]
    public async Task<ActionResult<QuestionnaireResponseDto>> SubmitQuestionnaire([FromBody] CreateQuestionnaireDto dto)
    {
        var result = await _triageService.SubmitQuestionnaireAsync(dto);
        return CreatedAtAction(nameof(GetQuestionnaireById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a pre-consultation questionnaire by ID.
    /// </summary>
    [HttpGet("questionnaires/{id:int}")]
    public async Task<ActionResult<QuestionnaireResponseDto>> GetQuestionnaireById(int id)
    {
        var result = await _triageService.GetQuestionnaireByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Questionnaire with ID {id} not found." });
        return Ok(result);
    }

    // ── Triage Assessments ─────────────────────────────────────────────────────

    /// <summary>
    /// Processes patient symptoms to compute urgency scores and match specialties.
    /// </summary>
    [HttpPost("assessments")]
    public async Task<ActionResult<TriageAssessmentDto>> ProcessTriage([FromBody] ProcessTriageDto dto)
    {
        var result = await _triageService.ProcessTriageAsync(dto);
        return CreatedAtAction(nameof(GetTriageAssessmentById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retrieves a specific triage assessment by ID.
    /// </summary>
    [HttpGet("assessments/{id:int}")]
    public async Task<ActionResult<TriageAssessmentDto>> GetTriageAssessmentById(int id)
    {
        var result = await _triageService.GetTriageAssessmentByIdAsync(id);
        if (result == null) return NotFound(new { message = $"Triage assessment with ID {id} not found." });
        return Ok(result);
    }

    /// <summary>
    /// Gets triage assessment history for a specific appointment ID.
    /// </summary>
    [HttpGet("appointment/{appointmentId:int}/history")]
    public async Task<ActionResult<IEnumerable<TriageAssessmentDto>>> GetTriageHistoryByAppointment(int appointmentId)
    {
        var history = await _triageService.GetTriageHistoryByAppointmentAsync(appointmentId);
        return Ok(history);
    }

    // ── Specialties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Lists all available medical specialties for assignment and referral.
    /// </summary>
    [HttpGet("specialties")]
    public async Task<ActionResult<IEnumerable<string>>> GetSpecialties()
    {
        var specialties = await _triageService.GetSpecialtiesAsync();
        return Ok(specialties);
    }

    // ── Referrals ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a manual or automated referral to a target medical specialty.
    /// </summary>
    [HttpPost("referrals")]
    public async Task<ActionResult<ReferralDto>> CreateReferral([FromBody] CreateReferralDto dto)
    {
        var result = await _triageService.CreateReferralAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of a referral (e.g., Generated -> Reviewed -> Assigned).
    /// </summary>
    [HttpPatch("referrals/{id:int}/status")]
    public async Task<ActionResult<ReferralDto>> UpdateReferralStatus(int id, [FromBody] UpdateReferralStatusDto dto)
    {
        var updated = await _triageService.UpdateReferralStatusAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Referral with ID {id} not found." });
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves all referrals for clinical staff review dashboard.
    /// </summary>
    [HttpGet("referrals")]
    public async Task<ActionResult<IEnumerable<ReferralDto>>> GetAllReferrals()
    {
        var referrals = await _triageService.GetAllReferralsAsync();
        return Ok(referrals);
    }
}