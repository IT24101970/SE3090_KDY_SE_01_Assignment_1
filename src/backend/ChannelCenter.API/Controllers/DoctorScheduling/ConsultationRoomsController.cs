using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Services.DoctorScheduling;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.DoctorScheduling;

[ApiController]
[Route("api/doctor-scheduling/[controller]")]
public class ConsultationRoomsController : ControllerBase
{
    private readonly IDoctorSchedulingService _service;

    public ConsultationRoomsController(IDoctorSchedulingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsultationRoomDto>>> GetRooms()
    {
        var rooms = await _service.GetRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultationRoomDto>> GetRoomById(int id)
    {
        var room = await _service.GetRoomByIdAsync(id);
        if (room == null) return NotFound(new { message = $"Room with ID {id} not found." });
        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<ConsultationRoomDto>> CreateRoom([FromBody] CreateConsultationRoomDto dto)
    {
        var created = await _service.CreateRoomAsync(dto);
        return CreatedAtAction(nameof(GetRoomById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ConsultationRoomDto>> UpdateRoom(int id, [FromBody] UpdateConsultationRoomDto dto)
    {
        var updated = await _service.UpdateRoomAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Room with ID {id} not found." });
        return Ok(updated);
    }

    [HttpGet("{id}/availability")]
    public async Task<ActionResult<bool>> CheckRoomAvailability(int id, [FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromQuery] int? excludeScheduleId = null)
    {
        var isAvailable = await _service.CheckRoomAvailabilityAsync(id, startTime, endTime, excludeScheduleId);
        return Ok(new { roomId = id, startTime, endTime, isAvailable });
    }
}
