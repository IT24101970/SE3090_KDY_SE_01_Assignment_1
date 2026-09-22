namespace ChannelCenter.API.DTOs.IntakeAgent;

public class IntakeAgentLogDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ToolCalled { get; set; } = string.Empty;
    public string InputPayload { get; set; } = "{}";
    public string OutputPayload { get; set; } = "{}";
    public string? SessionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
