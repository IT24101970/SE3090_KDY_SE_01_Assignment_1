namespace ChannelCenter.API.Models;

public class TriageAssessment
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string RawSymptoms { get; set; } = string.Empty;
    public int UrgencyScore { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string? ReasoningTrace { get; set; }
    public string RecommendedSpecialty { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Appointment? Appointment { get; set; }
}