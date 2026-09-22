using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.Services.Appointment;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // Convenience constructor for tests
    public AppointmentsController(ApplicationDbContext context)
        : this(new AppointmentService(context))
    {
    }

    // GET: api/appointments
    // Query params: ?patientId=1&doctorId=2&status=Pending&startDate=2026-09-01&upcomingOnly=true&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] AppointmentFilterDto filter)
    {
        var result = await _appointmentService.GetAppointmentsAsync(filter);
        return Ok(result);
    }

    // GET: api/appointments/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        var appt = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appt == null)
        {
            return NotFound(new { message = $"Appointment with ID {id} not found." });
        }
        return Ok(appt);
    }

    // GET: api/appointments/slots
    // Query params: ?specialtyId=1&doctorId=2&date=2026-09-10&availableOnly=true
    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] ChannelSlotFilterDto filter)
    {
        var slots = await _appointmentService.GetAvailableChannelSlotsAsync(filter);
        return Ok(slots);
    }

    // GET: api/appointments/patient/{patientId}/history
    [HttpGet("patient/{patientId:int}/history")]
    public async Task<IActionResult> GetPatientHistory(int patientId, [FromQuery] AppointmentFilterDto filter)
    {
        var history = await _appointmentService.GetPatientAppointmentHistoryAsync(patientId, filter);
        return Ok(history);
    }

    // POST: api/appointments
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _appointmentService.CreateAppointmentAsync(dto);
        if (!success)
        {
            if (errorMessage != null && errorMessage.Contains("does not exist"))
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return CreatedAtAction(nameof(GetAppointmentById), new { id = data!.Id }, data);
    }

    // PATCH: api/appointments/{id}/status
    // Body: { "status": "Confirmed" } or { "status": "Completed" } or { "status": "Cancelled", "cancelReason": "Patient requested" }
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _appointmentService.UpdateAppointmentStatusAsync(id, dto);
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

    // POST: api/appointments/{id}/cancel
    // Body: { "cancelReason": "Patient unable to attend" }
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id, [FromBody] CancelAppointmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _appointmentService.CancelAppointmentAsync(id, dto);
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
}
