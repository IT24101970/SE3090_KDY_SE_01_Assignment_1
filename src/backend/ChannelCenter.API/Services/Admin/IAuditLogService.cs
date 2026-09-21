using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Services.Admin;

public interface IAuditLogService
{
    Task<PagedResult<WorkflowAuditResponseDto>> GetAuditLogsAsync(AuditLogFilterDto filter);
    Task<WorkflowAuditResponseDto> CreateAuditLogAsync(CreateAuditLogDto request);
}
