using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.DoctorScheduling;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.DoctorScheduling;

[ApiController]
[Route("api/doctor-scheduling/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IDoctorSchedulingService _service;

    public ConsultationsController(IDoctorSchedulingService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultationDto>> GetConsultationById(int id)
    {
        var consultation = await _service.GetConsultationByIdAsync(id);
        if (consultation == null) return NotFound(new { message = $"Consultation record with ID {id} not found." });
        return Ok(consultation);
    }

    [HttpGet("appointment/{appointmentId}")]
    public async Task<ActionResult<ConsultationDto>> GetConsultationByAppointmentId(int appointmentId)
    {
        var consultation = await _service.GetConsultationByAppointmentIdAsync(appointmentId);
        if (consultation == null) return NotFound(new { message = $"Consultation record for appointment {appointmentId} not found." });
        return Ok(consultation);
    }

    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> CreateConsultation([FromBody] CreateConsultationDto dto)
    {
        var created = await _service.CreateConsultationAsync(dto);
        return CreatedAtAction(nameof(GetConsultationById), new { id = created.Id }, created);
    }

    [HttpPatch("{id}/attendance")]
    public async Task<ActionResult<ConsultationDto>> UpdateAttendanceStatus(int id, [FromBody] UpdateAttendanceStatusDto dto)
    {
        var updated = await _service.UpdateAttendanceStatusAsync(id, dto.AttendanceStatus);
        if (updated == null) return NotFound(new { message = $"Consultation record with ID {id} not found." });
        return Ok(updated);
    }
}
