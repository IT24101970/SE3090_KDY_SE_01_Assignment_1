namespace ChannelCenter.API.DTOs.Admin;

public class WorkflowAuditResponseDto
{
    public int Id { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public string ToolCalled { get; set; } = string.Empty;
    
    // This will be sent as a raw JSON string to the frontend, 
    // where React can parse it using JSON.parse() for display.
    public string ToolOutput { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; }
}