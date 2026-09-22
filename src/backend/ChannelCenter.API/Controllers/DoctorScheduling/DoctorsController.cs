using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Services.DoctorScheduling;
using Microsoft.AspNetCore.Mvc;

namespace ChannelCenter.API.Controllers.DoctorScheduling;

[ApiController]
[Route("api/doctor-scheduling/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorSchedulingService _service;

    public DoctorsController(IDoctorSchedulingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
    {
        var doctors = await _service.GetDoctorsAsync();
        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorDto>> GetDoctorById(int id)
    {
        var doctor = await _service.GetDoctorByIdAsync(id);
        if (doctor == null) return NotFound(new { message = $"Doctor with ID {id} not found." });
        return Ok(doctor);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorDto>> CreateDoctor([FromBody] CreateDoctorDto dto)
    {
        try
        {
            var created = await _service.CreateDoctorAsync(dto);
            return CreatedAtAction(nameof(GetDoctorById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DoctorDto>> UpdateDoctor(int id, [FromBody] UpdateDoctorDto dto)
    {
        var updated = await _service.UpdateDoctorAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Doctor with ID {id} not found." });
        return Ok(updated);
    }
}
