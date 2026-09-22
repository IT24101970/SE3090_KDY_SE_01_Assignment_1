namespace ChannelCenter.API.DTOs.Appointment;

public class ChannelSlotDto
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomFloor { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxPatients { get; set; }
    public int BookedSlots { get; set; }
    public int AvailableSlots => Math.Max(0, MaxPatients - BookedSlots);
    public bool IsAvailable => AvailableSlots > 0;
}
