using ChannelCenter.API.DTOs.SafetyAuditor;

namespace ChannelCenter.API.Services.SafetyAuditor;

public interface ISafetyAuditorReadService
{
    Task<AppointmentAuditContextDto?> GetAppointmentContextAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);
}
