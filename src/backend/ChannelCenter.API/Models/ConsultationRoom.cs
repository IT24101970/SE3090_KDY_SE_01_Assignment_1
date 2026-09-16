namespace ChannelCenter.API.Models;

public class ConsultationRoom
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}