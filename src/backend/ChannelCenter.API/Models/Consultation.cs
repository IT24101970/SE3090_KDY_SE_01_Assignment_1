namespace ChannelCenter.API.Models;

public class Consultation
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string ClinicalNotes { get; set; } = string.Empty;
    
    // Stored as JSONB in the database
    public string PrescriptionData { get; set; } = "{}"; 
    
    public AttendanceStatus AttendanceStatus { get; set; } = AttendanceStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}