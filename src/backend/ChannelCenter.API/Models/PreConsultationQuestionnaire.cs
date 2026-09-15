namespace ChannelCenter.API.Models;

public class PreConsultationQuestionnaire
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ResponsesData { get; set; } = string.Empty; // JSON payload
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}