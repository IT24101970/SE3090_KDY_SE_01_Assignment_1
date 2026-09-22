using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Appointment;

public class AppointmentFilterDto
{
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int? ScheduleId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? UpcomingOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
