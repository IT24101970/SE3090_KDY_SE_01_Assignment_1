namespace ChannelCenter.API.Models;

// 4. Admin Overview
public enum WorkflowStatus
{
    Running,
    PausedForApproval,
    Completed
}

public enum ApprovalDecision
{
    Approved,
    Rejected,
    Revised
}


// 1. Patiant Management
public enum ApoinmentStatus
{
    Pending,
    Confirmed,
    Cancelled
}