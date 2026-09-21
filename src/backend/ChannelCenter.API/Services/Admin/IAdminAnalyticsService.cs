using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Services.Admin;

public interface IAdminAnalyticsService
{
    Task<AnalyticsOverviewDto> GetOverviewAsync();
    Task<IEnumerable<DailyAppointmentVolumeDto>> GetDailyAppointmentVolumesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<TriageUrgencyRatioDto>> GetTriageRatiosAsync();
    Task<IEnumerable<DoctorWorkloadDto>> GetDoctorWorkloadsAsync();
    Task<AiSafetyMetricsDto> GetAiSafetyMetricsAsync();
}
