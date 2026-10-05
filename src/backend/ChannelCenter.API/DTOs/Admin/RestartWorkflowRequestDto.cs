using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public sealed class RestartWorkflowRequestDto
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}
