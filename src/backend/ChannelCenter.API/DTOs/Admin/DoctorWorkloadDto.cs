namespace ChannelCenter.API.DTOs.Admin;

public class DoctorWorkloadDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int ScheduledSlotsCount { get; set; }
}
