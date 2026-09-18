using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Admin;

public class WorkflowApprovalResponseDto
{
    public int ApprovalId { get; set; }
    public int WorkflowId { get; set; }
    public ApprovalDecision Decision { get; set; }
    
    // Crucial for the UI: Let the frontend know if the workflow resumed or completed
    public WorkflowStatus UpdatedWorkflowStatus { get; set; }
    public DateTime ProcessedAt { get; set; }
}