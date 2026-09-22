using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.DoctorScheduling;

public class ConsultationDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string ClinicalNotes { get; set; } = string.Empty;
    public string PrescriptionData { get; set; } = "{}";
    public AttendanceStatus AttendanceStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateConsultationDto
{
    public int AppointmentId { get; set; }
    public string ClinicalNotes { get; set; } = string.Empty;
    public string PrescriptionData { get; set; } = "{}";
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Present;
}

public class UpdateAttendanceStatusDto
{
    public AttendanceStatus AttendanceStatus { get; set; }
}
