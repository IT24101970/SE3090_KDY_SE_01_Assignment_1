using ChannelCenter.API.DTOs.Common;
using ChannelCenter.API.DTOs.Patient;

namespace ChannelCenter.API.Services.Patient;

public interface IPatientService
{
    Task<PagedResult<PatientResponseDto>> GetPatientsAsync(PatientFilterDto filter);
    Task<PatientResponseDto?> GetPatientByIdAsync(int id);
    Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId);
    Task<(bool Success, string? ErrorMessage, PatientResponseDto? Data)> CreatePatientAsync(CreatePatientDto dto, int? userId = null);
    Task<(bool Success, string? ErrorMessage, PatientResponseDto? Data)> UpdatePatientAsync(int id, UpdatePatientDto dto);
    Task<(bool Success, string? ErrorMessage)> DeletePatientAsync(int id);
}
