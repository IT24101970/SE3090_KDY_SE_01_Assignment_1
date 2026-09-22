using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.DTOs.Common;

namespace ChannelCenter.API.Services.Appointment;

public interface IAppointmentService
{
    Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> CreateAppointmentAsync(CreateAppointmentDto dto);
    Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id);
    Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(AppointmentFilterDto filter);
    Task<PagedResult<AppointmentResponseDto>> GetPatientAppointmentHistoryAsync(int patientId, AppointmentFilterDto filter);
    Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> UpdateAppointmentStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> CancelAppointmentAsync(int id, CancelAppointmentDto dto);
    Task<List<ChannelSlotDto>> GetAvailableChannelSlotsAsync(ChannelSlotFilterDto filter);
}
