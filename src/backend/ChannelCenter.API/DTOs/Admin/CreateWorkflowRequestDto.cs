using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class CreateWorkflowRequestDto
{
    [Required]
    [MaxLength(255)]
    public string Objective { get; set; } = string.Empty;

    public bool RequiresHumanApproval { get; set; } = false;
}
