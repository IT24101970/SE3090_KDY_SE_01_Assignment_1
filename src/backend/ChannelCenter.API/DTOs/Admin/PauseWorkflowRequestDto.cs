using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class PauseWorkflowRequestDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AgentName { get; set; } = "SafetyAuditor";

    [MaxLength(128)]
    public string? CorrelationId { get; set; }

    [MaxLength(32)]
    public string ContractVersion { get; set; } = "safety-audit.v1";

    [MaxLength(32)]
    public string? RiskLevel { get; set; }

    [MaxLength(500)]
    public string? ValidationViolation { get; set; }
}
