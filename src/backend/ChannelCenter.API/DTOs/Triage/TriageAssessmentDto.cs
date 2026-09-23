using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Triage;

public class TriageAssessmentDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string RawSymptoms { get; set; } = string.Empty;
    public int UrgencyScore { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string? ReasoningTrace { get; set; }
    public string RecommendedSpecialty { get; set; } = string.Empty;
    public List<SymptomItemDto> SymptomLogs { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}