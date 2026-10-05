using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Appointment;

public class UpdateAppointmentDto
{
    public int? DoctorId { get; set; }
    public int? ScheduleId { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public AppointmentStatus? Status { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? CancelReason { get; set; }
}
