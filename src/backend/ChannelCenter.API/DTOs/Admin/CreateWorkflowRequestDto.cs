using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class CreateWorkflowRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Objective { get; set; } = string.Empty;

    public bool RequiresHumanApproval { get; set; } = false;

    [MaxLength(128)]
    public string? CorrelationId { get; set; }

    [MaxLength(32)]
    public string ContractVersion { get; set; } = "safety-audit.v1";
}
