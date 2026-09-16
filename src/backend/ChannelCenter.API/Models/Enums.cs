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

// 3. Medical Triage & Specialist Matching
public enum UrgencyLevel
{
    Low,
    Medium,
    High,
    Emergency
}

public enum ReferralStatus
{
    Generated,
    Reviewed,
    Assigned
}


//student 2 
public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected
}

public enum AttendanceStatus
{
    Pending,
    Present,
    NoShow
}