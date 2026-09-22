namespace ChannelCenter.API.Models;

public class Doctor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SpecialtyId { get; set; }
    public string Qualifications { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User? User { get; set; }
    public Specialty? Specialty { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<DoctorLeave> Leaves { get; set; } = new List<DoctorLeave>();
}