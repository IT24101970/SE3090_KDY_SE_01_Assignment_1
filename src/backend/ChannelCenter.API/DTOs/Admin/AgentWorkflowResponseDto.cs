using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Admin;

public class AgentWorkflowResponseDto
{
    public int Id { get; set; }
    public string Objective { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } 
    public bool RequiresHumanApproval { get; set; }
    public DateTime CreatedAt { get; set; }

    // Nested list of the specific audit logs for this single workflow
    public List<WorkflowAuditResponseDto> AuditLogs { get; set; } = new();
}