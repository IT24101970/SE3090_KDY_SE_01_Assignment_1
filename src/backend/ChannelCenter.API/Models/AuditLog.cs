using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelCenter.API.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string ToolCalled { get; set; } = string.Empty;
    
    // This tells PostgreSQL to format this string as a JSONB column
    [Column(TypeName = "jsonb")]
    public string ToolOutput { get; set; } = "{}"; 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public AgentWorkflow Workflow { get; set; } = null!;
}