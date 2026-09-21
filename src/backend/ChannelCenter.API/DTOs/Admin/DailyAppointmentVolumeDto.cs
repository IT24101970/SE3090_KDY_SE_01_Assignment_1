namespace ChannelCenter.API.DTOs.Admin;

public class DailyAppointmentVolumeDto
{
    public string Date { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int ConfirmedCount { get; set; }
    public int CancelledCount { get; set; }
    public int PendingCount { get; set; }
}
