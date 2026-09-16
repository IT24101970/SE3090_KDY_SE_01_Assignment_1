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

    // JSON input sent to the tool
    public string InputPayload { get; set; } = "{}";

    // JSON output received from the tool
    public string OutputPayload { get; set; } = "{}";

    // Optional notes about the intake session
    public string? SessionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Patient? Patient { get; set; }
}
