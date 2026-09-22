using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.DoctorScheduling;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.DoctorScheduling;

[ApiController]
[Route("api/doctor-scheduling/[controller]")]
public class DoctorLeavesController : ControllerBase
{
    private readonly IDoctorSchedulingService _service;

    public DoctorLeavesController(IDoctorSchedulingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorLeaveDto>>> GetLeaves([FromQuery] int? doctorId, [FromQuery] LeaveStatus? status)
    {
        var leaves = await _service.GetLeavesAsync(doctorId, status);
        return Ok(leaves);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorLeaveDto>> GetLeaveById(int id)
    {
        var leave = await _service.GetLeaveByIdAsync(id);
        if (leave == null) return NotFound(new { message = $"Leave record with ID {id} not found." });
        return Ok(leave);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorLeaveDto>> CreateLeave([FromBody] CreateDoctorLeaveDto dto)
    {
        var created = await _service.CreateLeaveAsync(dto);
        return CreatedAtAction(nameof(GetLeaveById), new { id = created.Id }, created);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<DoctorLeaveDto>> UpdateLeaveStatus(int id, [FromBody] UpdateDoctorLeaveStatusDto dto)
    {
        var updated = await _service.UpdateLeaveStatusAsync(id, dto.Status);
        if (updated == null) return NotFound(new { message = $"Leave record with ID {id} not found." });
        return Ok(updated);
    }
}
