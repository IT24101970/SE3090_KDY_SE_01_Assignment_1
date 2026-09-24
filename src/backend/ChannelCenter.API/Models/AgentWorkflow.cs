namespace ChannelCenter.API.Models;

public class AgentWorkflow
{
    public int Id { get; set; }
    public string Objective { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
    public bool RequiresHumanApproval { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    public string ContractVersion { get; set; } = "safety-audit.v1";
    public string? RiskLevel { get; set; }
    public string? PlanSummary { get; set; }
    public string? ValidationSummary { get; set; }
    public string? FinalOutcome { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime? SafetyAuditStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SafeFailedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<AdminApproval> AdminApprovals { get; set; } = new List<AdminApproval>();
    public Appointment? Appointment { get; set; }
}