using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Services.Admin;

public interface IAdminOverrideService
{
    Task<(bool Success, string? ErrorMessage, int? WorkflowId)> CancelWorkflowAsync(int id, OverrideCancelRequestDto request, int adminUserId);
    Task<(bool Success, string? ErrorMessage, int? WorkflowId)> ReassignWorkflowAsync(int id, OverrideReassignRequestDto request, int adminUserId);
}
