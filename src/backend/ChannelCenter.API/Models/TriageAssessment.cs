namespace ChannelCenter.API.Models;

public class TriageAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? QuestionnaireId { get; set; }
    public string RawSymptoms { get; set; } = string.Empty;
    public int UrgencyScore { get; set; }
    public UrgencyLevel UrgencyLevel { get; set; }
    public string? ReasoningTrace { get; set; }
    public int RecommendedSpecialtyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public PreConsultationQuestionnaire? Questionnaire { get; set; }
    public Specialty? RecommendedSpecialty { get; set; }
}