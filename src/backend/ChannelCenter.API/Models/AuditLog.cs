using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelCenter.API.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string ToolCalled { get; set; } = string.Empty;
    public string? StepName { get; set; }
    public string? CorrelationId { get; set; }
    public string? ContractVersion { get; set; }
    public string? Outcome { get; set; }
    public int? DurationMs { get; set; }
    
    //JSONB stored in the database
    public string ToolOutput { get; set; } = "{}"; 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public AgentWorkflow Workflow { get; set; } = null!;
}