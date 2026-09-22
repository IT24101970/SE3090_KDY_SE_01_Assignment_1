using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Appointment;

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string PatientNIC { get; set; } = string.Empty;

    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;

    public int ScheduleId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomFloor { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }
    public string ReasonForVisit { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
