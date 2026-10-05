using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Patient;
using ChannelCenter.API.Services.Patient;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET: api/patients
    // Supports query params: ?searchTerm=john&gender=Male&bloodGroup=O+&page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] PatientFilterDto filter)
    {
        var result = await _patientService.GetPatientsAsync(filter);
        return Ok(result);
    }

    // GET: api/patients/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPatientById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound(new { message = $"Patient with ID {id} not found." });
        }
        return Ok(patient);
    }

    // GET: api/patients/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetPatientByUserId(int userId)
    {
        var patient = await _patientService.GetPatientByUserIdAsync(userId);
        if (patient == null)
        {
            return NotFound(new { message = $"Patient profile for User ID {userId} not found." });
        }
        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _patientService.CreatePatientAsync(dto);
        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return CreatedAtAction(nameof(GetPatientById), new { id = data!.Id }, data);
    }

    // PUT: api/patients/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, errorMessage, data) = await _patientService.UpdatePatientAsync(id, dto);
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

    // DELETE: api/patients/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var (success, errorMessage) = await _patientService.DeletePatientAsync(id);
        if (!success)
        {
            if (errorMessage != null && errorMessage.Contains("not found"))
            {
                return NotFound(new { message = errorMessage });
            }
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = $"Patient with ID {id} was successfully deleted." });
    }
}
