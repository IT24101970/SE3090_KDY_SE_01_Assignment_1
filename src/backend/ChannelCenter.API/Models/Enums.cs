namespace ChannelCenter.API.Models;

// 4. Admin Overview
public enum WorkflowStatus
{
    Running,
    PausedForApproval,
    Completed,
    Terminated
}

public enum ApprovalDecision
{
    Approved,
    Rejected,
    Revised
}


// Student 1: User roles
public enum UserRole
{
    Patient,
    Doctor,
    Admin
}

// Student 1: Patient Management & Appointment Lifecycle
// Note: ApoinmentStatus kept for backward compat (typo from initial scaffold)
public enum ApoinmentStatus
{
    Pending,
    Confirmed,
    Cancelled
}

// Correctly spelled alias used by Appointment.cs
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
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