using System.ComponentModel.DataAnnotations.Schema;

namespace ChannelCenter.API.Models;

// Student 1: Intake & Intent Structuring Agent — logs each agent tool call
// Mirrors the pattern used by Student 4's AuditLog.cs for the Admin agent
public class IntakeAgentLog
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    // Which agent tool was invoked: GetPatientHistory | FormatIntakeSummary | ValidatePatientEligibility
    public string ToolCalled { get; set; } = string.Empty;

    // Raw JSON input sent to the tool
    [Column(TypeName = "jsonb")]
    public string InputPayload { get; set; } = "{}";

    // Raw JSON output returned from the tool
    [Column(TypeName = "jsonb")]
    public string OutputPayload { get; set; } = "{}";

    public string? SessionNotes { get; set; }                       // Optional human-readable summary
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Patient? Patient { get; set; }
}
