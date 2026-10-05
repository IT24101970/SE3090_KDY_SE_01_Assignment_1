using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.IntakeAgent;

/// <summary>
/// Student 1 — Intake &amp; Intent Structuring Agent
/// Deterministic schema validator for the agent's structured output contract.
/// This runs BEFORE the response is returned to the caller and rejects any output
/// that does not conform to the agreed schema — no LLM-as-judge, fully rule-based.
/// </summary>
public static class IntakeAgentOutputValidator
{
    // Allowed urgency level values (maps to UrgencyLevel enum strings)
    private static readonly HashSet<string> AllowedUrgencyLevels =
        new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Emergency" };

    // Allowed severity values for individual symptoms
    private static readonly HashSet<string> AllowedSeverities =
        new(StringComparer.OrdinalIgnoreCase) { "Mild", "Moderate", "Severe", "Critical" };

    /// <summary>
    /// Validates the structured response produced by the agent.
    /// Returns (true, null) on success, or (false, reason) on failure.
    /// </summary>
    public static (bool IsValid, string? FailureReason) Validate(IntakeStructuredResponseDto response)
    {
        // Rule 1 — PatientId must be a positive integer
        if (response.PatientId <= 0)
        {
            return (false, $"SCHEMA_VIOLATION: patientId must be a positive integer, got '{response.PatientId}'.");
        }

        // Rule 2 — PatientMetadata must be present and non-empty
        if (response.PatientMetadata is null)
        {
            return (false, "SCHEMA_VIOLATION: patientMetadata is required but was null.");
        }

        if (string.IsNullOrWhiteSpace(response.PatientMetadata.Name))
        {
            return (false, "SCHEMA_VIOLATION: patientMetadata.name is required and cannot be empty.");
        }

        // Rule 3 — Symptoms list must be present (can be empty, but not null)
        if (response.Symptoms is null)
        {
            return (false, "SCHEMA_VIOLATION: symptoms array is required but was null.");
        }

        foreach (var symptom in response.Symptoms)
        {
            if (string.IsNullOrWhiteSpace(symptom.Keyword))
            {
                return (false, "SCHEMA_VIOLATION: each symptom must have a non-empty keyword.");
            }

            if (!AllowedSeverities.Contains(symptom.Severity ?? ""))
            {
                return (false, $"SCHEMA_VIOLATION: symptom severity '{symptom.Severity}' is not allowed. " +
                               $"Allowed values: {string.Join(", ", AllowedSeverities)}.");
            }
        }

        // Rule 4 — SeverityFlags must be present
        if (response.SeverityFlags is null)
        {
            return (false, "SCHEMA_VIOLATION: severityFlags is required but was null.");
        }

        // Rule 5 — UrgencyLevel must be a known enum value
        var urgencyName = response.SeverityFlags.UrgencyLevel.ToString();
        if (!AllowedUrgencyLevels.Contains(urgencyName))
        {
            return (false, $"SCHEMA_VIOLATION: urgencyLevel '{urgencyName}' is not a supported value. " +
                           $"Allowed: {string.Join(", ", AllowedUrgencyLevels)}.");
        }

        // Rule 6 — PreferredTimeWindows must be non-null
        if (response.PreferredTimeWindows is null)
        {
            return (false, "SCHEMA_VIOLATION: preferredTimeWindows is required but was null.");
        }

        // Rule 7 — DoctorPreferences must be non-null
        if (response.DoctorPreferences is null)
        {
            return (false, "SCHEMA_VIOLATION: doctorPreferences is required but was null.");
        }

        // Rule 8 — ExecutionPlan must have at least one step
        if (response.ExecutionPlan is null || response.ExecutionPlan.Count == 0)
        {
            return (false, "SCHEMA_VIOLATION: executionPlan must contain at least one step.");
        }

        // Rule 9 — SummaryText must not be null (can be empty for sparse inputs)
        if (response.SummaryText is null)
        {
            return (false, "SCHEMA_VIOLATION: summaryText is required but was null.");
        }

        // Rule 10 — ProcessedAt must be a reasonable timestamp (not default/epoch)
        if (response.ProcessedAt == default || response.ProcessedAt.Year < 2020)
        {
            return (false, $"SCHEMA_VIOLATION: processedAt timestamp '{response.ProcessedAt:O}' is invalid.");
        }

        // Rule 11 — GeneratedLogIds must be non-null
        if (response.GeneratedLogIds is null)
        {
            return (false, "SCHEMA_VIOLATION: generatedLogIds is required but was null.");
        }

        return (true, null);
    }
}
