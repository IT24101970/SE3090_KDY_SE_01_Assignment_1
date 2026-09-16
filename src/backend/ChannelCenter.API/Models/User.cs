namespace ChannelCenter.API.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int ScheduleId { get; set; }                             // Links to DoctorSchedule (Student 2)
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? CancelReason { get; set; }                       // Populated only if cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public DoctorSchedule? Schedule { get; set; }
}
