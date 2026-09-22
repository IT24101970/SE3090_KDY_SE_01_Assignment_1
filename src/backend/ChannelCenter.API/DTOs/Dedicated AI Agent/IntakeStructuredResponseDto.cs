using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.IntakeAgent;

public class IntakeStructuredResponseDto
{
    public int PatientId { get; set; }
    public ValidatedPatientMetadataDto PatientMetadata { get; set; } = new();
    public List<StructuredSymptomDto> Symptoms { get; set; } = new();
    public IntakeSeverityFlagsDto SeverityFlags { get; set; } = new();
    public List<string> PreferredTimeWindows { get; set; } = new();
    public IntakeDoctorPreferencesDto DoctorPreferences { get; set; } = new();
    public List<string> ExecutionPlan { get; set; } = new();
    public string SummaryText { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public List<int> GeneratedLogIds { get; set; } = new();
}

public class ValidatedPatientMetadataDto
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string NIC { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public bool IsEligible { get; set; } = true;
    public string EligibilityStatus { get; set; } = "Eligible";
}

public class StructuredSymptomDto
{
    public string Keyword { get; set; } = string.Empty;
    public string Severity { get; set; } = "Moderate"; // Mild, Moderate, Severe, Critical
    public string? Duration { get; set; }
    public string? Notes { get; set; }
}

public class IntakeSeverityFlagsDto
{
    public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Medium;
    public bool IsEmergency { get; set; }
    public bool RequiresImmediateAttention { get; set; }
    public List<string> RedFlagsDetected { get; set; } = new();
    public string UrgencyNotes { get; set; } = string.Empty;
}

public class IntakeDoctorPreferencesDto
{
    public string? PreferredDoctorName { get; set; }
    public string? PreferredSpecialty { get; set; }
    public string? Notes { get; set; }
}
