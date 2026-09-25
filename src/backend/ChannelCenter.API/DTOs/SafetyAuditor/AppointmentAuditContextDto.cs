namespace ChannelCenter.API.DTOs.SafetyAuditor;

public sealed class AppointmentAuditContextDto
{
    public int AppointmentId { get; init; }
    public int PatientId { get; init; }
    public int DoctorId { get; init; }
    public int ScheduleId { get; init; }
    public DateTime AppointmentDate { get; init; }
    public string AppointmentStatus { get; init; } = string.Empty;
    public string ReasonForVisit { get; init; } = string.Empty;
    public TriageAuditContextDto? Triage { get; init; }
    public DoctorAuditContextDto? Doctor { get; init; }
    public ScheduleAuditContextDto? Schedule { get; init; }
}

public sealed class TriageAuditContextDto
{
    public int Id { get; init; }
    public string UrgencyLevel { get; init; } = string.Empty;
    public string RecommendedSpecialty { get; init; } = string.Empty;
    public string RawSymptoms { get; init; } = string.Empty;
}

public sealed class DoctorAuditContextDto
{
    public int Id { get; init; }
    public int SpecialtyId { get; init; }
    public string SpecialtyName { get; init; } = string.Empty;
}

public sealed class ScheduleAuditContextDto
{
    public int Id { get; init; }
    public int DoctorId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int MaxPatients { get; init; }
    public int BookedPatients { get; init; }
    public bool IsAvailable { get; init; }
}
