using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class CreateAuditLogDto
{
    [Required]
    public int WorkflowId { get; set; }

    [Required]
    [MaxLength(100)]
    public string AgentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ToolCalled { get; set; } = string.Empty;

    public string ToolOutput { get; set; } = "{}";
}
