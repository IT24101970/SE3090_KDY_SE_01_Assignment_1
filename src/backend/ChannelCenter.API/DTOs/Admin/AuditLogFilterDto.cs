namespace ChannelCenter.API.DTOs.Admin;

public class AuditLogFilterDto
{
    public int? WorkflowId { get; set; }
    public string? AgentName { get; set; }
    public string? ToolCalled { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
