namespace ChannelCenter.API.Models;

public class AdminApproval
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    
    // Note: Once Member 1 creates the 'User' model, you can link this properly
    public int AdminUserId { get; set; } 
    
    public ApprovalDecision Decision { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public AgentWorkflow Workflow { get; set; } = null!;
}