//using ChannelCenter.API.DTOs.Common;

using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.DTOs.IntakeAgent;

namespace ChannelCenter.API.Services.IntakeAgent;

public interface IIntakeAgentService
{
    // Orchestrator method
    Task<(bool Success, string? ErrorMessage, IntakeStructuredResponseDto? Data)> ProcessIntakeAsync(IntakeProcessRequestDto request);

    // Allow-Listed Tool 1: GetPatientHistory(patientId)
    Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolGetPatientHistoryAsync(int patientId);

    // Allow-Listed Tool 2: FormatIntakeSummary(rawText)
    Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolFormatIntakeSummaryAsync(string rawText);

    // Allow-Listed Tool 3: ValidatePatientEligibility(patientId)
    Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolValidatePatientEligibilityAsync(int patientId);

    // Observability & Auditing
    Task<List<IntakeAgentLogDto>> GetLogsByPatientIdAsync(int patientId);
    Task<PagedResult<IntakeAgentLogDto>> GetAllLogsAsync(int page = 1, int pageSize = 20);
}
