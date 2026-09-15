namespace ChannelCenter.API.Models;

public class SymptomLog
{
    public int Id { get; set; }
    public int TriageId { get; set; }
    public string SymptomKeyword { get; set; } = string.Empty;
    public int? SeverityRating { get; set; }
    public int DurationInDays { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TriageAssessment? Triage { get; set; }
}