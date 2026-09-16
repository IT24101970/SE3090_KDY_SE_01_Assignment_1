namespace ChannelCenter.API.Models;

public class DoctorSchedule
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxPatients { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Doctor? Doctor { get; set; }
    public ConsultationRoom? Room { get; set; }
}