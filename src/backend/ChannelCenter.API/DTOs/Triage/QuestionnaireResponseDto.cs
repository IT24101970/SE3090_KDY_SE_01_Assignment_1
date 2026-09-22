namespace ChannelCenter.API.DTOs.Triage;

public class QuestionnaireResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string ResponsesData { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}