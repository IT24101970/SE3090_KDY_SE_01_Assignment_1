using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Triage;

public class TriageAssessmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? QuestionnaireId { get; set; }
    public string RawSymptoms { get; set; } = string.Empty;
    public int UrgencyScore { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; } // Low, Medium, High, Emergency[cite: 1, 4]
    public string? ReasoningTrace { get; set; } // AI reasoning trace[cite: 1]
    public SpecialtyDto? RecommendedSpecialty { get; set; }
    public List<SymptomItemDto> SymptomLogs { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}