using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Services.DoctorScheduling;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.DoctorScheduling;

[ApiController]
[Route("api/doctor-scheduling/[controller]")]
public class DoctorSchedulesController : ControllerBase
{
    private readonly IDoctorSchedulingService _service;

    public DoctorSchedulesController(IDoctorSchedulingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorScheduleDto>>> GetSchedules([FromQuery] int? doctorId, [FromQuery] DateTime? date)
    {
        var schedules = await _service.GetSchedulesAsync(doctorId, date);
        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorScheduleDto>> GetScheduleById(int id)
    {
        var schedule = await _service.GetScheduleByIdAsync(id);
        if (schedule == null) return NotFound(new { message = $"Schedule with ID {id} not found." });
        return Ok(schedule);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorScheduleDto>> CreateSchedule([FromBody] CreateDoctorScheduleDto dto)
    {
        try
        {
            var created = await _service.CreateScheduleAsync(dto);
            return CreatedAtAction(nameof(GetScheduleById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DoctorScheduleDto>> UpdateSchedule(int id, [FromBody] UpdateDoctorScheduleDto dto)
    {
        try
        {
            var updated = await _service.UpdateScheduleAsync(id, dto);
            if (updated == null) return NotFound(new { message = $"Schedule with ID {id} not found." });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var success = await _service.DeleteScheduleAsync(id);
        if (!success) return NotFound(new { message = $"Schedule with ID {id} not found." });
        return NoContent();
    }

    // ── Student 2: Agentic AI Subsystem Integration ────────────────────────────────
    [HttpGet("pending-appointments")]
    public async Task<IActionResult> GetPendingAppointments()
    {
        var list = await _service.GetPendingAppointmentsAsync();
        return Ok(list);
    }

    [HttpGet("triage-assessment/{appointmentId}")]

    public async Task<IActionResult> GetTriageAssessment(int appointmentId)
    {
        var assessment = await _service.GetTriageAssessmentForAppointmentAsync(appointmentId);
        if (assessment == null) return NotFound(new { message = $"No triage assessment found for appointment #{appointmentId}" });
        return Ok(assessment);
    }

    [HttpPost("ai-optimize")]

    public async Task<IActionResult> OptimizeScheduleWithAi([FromBody] object inputDto)
    {
        try
        {
            var result = await _service.OptimizeScheduleWithAiAsync(inputDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"AI Agent workflow failed: {ex.Message}" });
        }
    }

    [HttpPost("ai-approve/{workflowId}")]
    public async Task<ActionResult<DoctorScheduleDto>> ApproveAiScheduleWorkflow(string workflowId)
    {
        try
        {
            var created = await _service.ApproveAiScheduleWorkflowAsync(workflowId);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Approval failed: {ex.Message}" });
        }
    }
}

