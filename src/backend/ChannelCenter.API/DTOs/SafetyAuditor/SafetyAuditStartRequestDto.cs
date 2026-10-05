using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.SafetyAuditor;

public class SafetyAuditStartRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Objective { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string ContractVersion { get; set; } = "safety-audit.v1";

    [MaxLength(100)]
    public string SourceAgent { get; set; } = "ChannelCenter.API";

    public int? AppointmentId { get; set; }

    public Dictionary<string, object?> Proposal { get; set; } = new();
}
