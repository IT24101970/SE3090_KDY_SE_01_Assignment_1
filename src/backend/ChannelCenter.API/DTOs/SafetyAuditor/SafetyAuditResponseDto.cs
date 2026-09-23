using System.ComponentModel.DataAnnotations;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.SafetyAuditor;

public class SafetyAuditResponseDto
{
    public int WorkflowId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string ContractVersion { get; set; } = "safety-audit.v1";
    public WorkflowStatus Status { get; set; }
    public string RiskLevel { get; set; } = "Unknown";
    public bool RequiresApproval { get; set; }
    public List<string> Violations { get; set; } = new();
    public Dictionary<string, object?> ValidatedOutput { get; set; } = new();
    public List<SafetyAuditStepDto> Steps { get; set; } = new();
    public List<SafetyAuditToolCallDto> ToolCalls { get; set; } = new();
    public List<SafetyAuditPlanStepDto> Plan { get; set; } = new();
    public string? ValidationSummary { get; set; }
    public string? FinalOutcome { get; set; }
    public SafetyAuditErrorDto? Error { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class SafetyAuditStepDto
{
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    public int DurationMs { get; set; }
}

public class SafetyAuditPlanStepDto
{
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Tool { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Purpose { get; set; } = string.Empty;
}

public class SafetyAuditToolCallDto
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
}

public class SafetyAuditErrorDto
{
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;
}
