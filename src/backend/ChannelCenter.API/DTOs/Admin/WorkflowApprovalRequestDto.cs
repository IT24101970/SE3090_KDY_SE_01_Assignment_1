using System.ComponentModel.DataAnnotations;
using ChannelCenter.API.Models; // For the ApprovalDecision enum

namespace ChannelCenter.API.DTOs.Admin;

public class WorkflowApprovalRequestDto
{
    [Required]
    public ApprovalDecision Decision { get; set; }
}