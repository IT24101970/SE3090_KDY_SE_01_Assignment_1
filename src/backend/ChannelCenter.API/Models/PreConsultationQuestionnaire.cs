namespace ChannelCenter.API.Models;

public class PreConsultationQuestionnaire
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    
    // JSONB stored in the database, representing the patient's responses to the questionnaire
    public string ResponsesData { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}