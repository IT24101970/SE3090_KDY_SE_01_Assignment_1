using System.ComponentModel.DataAnnotations;
using ChannelCenter.API.Models; // For the ApprovalDecision enum

namespace ChannelCenter.API.DTOs.Admin;

public class WorkflowApprovalRequestDto
{
    // The AdminUserId will be extracted from their JWT token.
    // The WorkflowId is typically passed in the URL (e.g., POST /api/workflows/5/approve)
    [Required]  
    [Range(1, int.MaxValue, ErrorMessage = "WorkflowId must be a positive integer.")]
    public int WorkflowId { get; set; }
    
    [Required]
    public ApprovalDecision Decision { get; set; }
}