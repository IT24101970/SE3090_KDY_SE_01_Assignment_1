namespace ChannelCenter.API.Models;

public class AgentWorkflow
{
    public int Id { get; set; }
    public string Objective { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
    public bool RequiresHumanApproval { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<AdminApproval> AdminApprovals { get; set; } = new List<AdminApproval>();
}