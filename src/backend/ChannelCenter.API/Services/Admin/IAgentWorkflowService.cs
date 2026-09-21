using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Admin;

public interface IAgentWorkflowService
{
    Task<IEnumerable<AgentWorkflowResponseDto>> GetWorkflowsAsync(WorkflowStatus? status = null);
    Task<AgentWorkflowResponseDto?> GetWorkflowByIdAsync(int id);
    Task<AgentWorkflowResponseDto> CreateWorkflowAsync(CreateWorkflowRequestDto request);
    Task<(bool Success, string? ErrorMessage, WorkflowApprovalResponseDto? Response)> ApproveWorkflowAsync(int id, WorkflowApprovalRequestDto request, int adminUserId);
    Task<(bool Success, string? ErrorMessage)> PauseWorkflowAsync(int id, PauseWorkflowRequestDto request);
}
