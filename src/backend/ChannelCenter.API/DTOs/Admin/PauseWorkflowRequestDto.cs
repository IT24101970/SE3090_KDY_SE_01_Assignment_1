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

    public string? ValidationViolation { get; set; }
}
