using ChannelCenter.API.DTOs.SafetyAuditor;

namespace ChannelCenter.API.Services.SafetyAuditor;

public interface ISafetyAuditorService
{
    Task<SafetyAuditResponseDto> StartAsync(
        int workflowId,
        SafetyAuditStartRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SafetyAuditResponseDto> ApplyCallbackAsync(
        int workflowId,
        SafetyAuditResponseDto response,
        CancellationToken cancellationToken = default);
}
