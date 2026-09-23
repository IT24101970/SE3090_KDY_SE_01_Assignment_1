using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Admin;

public class AgentWorkflowResponseDto
{
    public int Id { get; set; }
    public string Objective { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } 
    public bool RequiresHumanApproval { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string ContractVersion { get; set; } = string.Empty;
    public string? RiskLevel { get; set; }
    public string? PlanSummary { get; set; }
    public string? ValidationSummary { get; set; }
    public string? FinalOutcome { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SafeFailedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Nested list of the specific audit logs for this single workflow
    public List<WorkflowAuditResponseDto> AuditLogs { get; set; } = new();
}