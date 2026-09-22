using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Common;
using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.IntakeAgent;

public class IntakeAgentService : IIntakeAgentService
{
    private readonly ApplicationDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public IntakeAgentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, IntakeStructuredResponseDto? Data)> ProcessIntakeAsync(IntakeProcessRequestDto request)
    {
        if (request.PatientId <= 0)
        {
            return (false, "A valid PatientId is required.", null);
        }

        if (string.IsNullOrWhiteSpace(request.RawText))
        {
            return (false, "Raw text is required for intake processing.", null);
        }

        var logIds = new List<int>();

        // Step 1: Tool Call - ValidatePatientEligibility(patientId)
        var eligibilityResult = await ToolValidatePatientEligibilityAsync(request.PatientId);
        if (!eligibilityResult.Success)
        {
            return (false, eligibilityResult.ErrorMessage, null);
        }
        var log1 = await SaveAgentLogAsync(request.PatientId, "ValidatePatientEligibility",
            JsonSerializer.Serialize(new { patientId = request.PatientId }),
            eligibilityResult.ResultJson,
            "Step 1: Patient eligibility validated");
        logIds.Add(log1.Id);

        // Parse eligibility
        var eligibilityDoc = JsonDocument.Parse(eligibilityResult.ResultJson);
        var isEligible = eligibilityDoc.RootElement.GetProperty("isEligible").GetBoolean();
        if (!isEligible)
        {
            var reason = eligibilityDoc.RootElement.GetProperty("reason").GetString() ?? "Patient is not currently eligible for booking.";
            return (false, reason, null);
        }

        // Step 2: Tool Call - GetPatientHistory(patientId)
        var historyResult = await ToolGetPatientHistoryAsync(request.PatientId);
        var log2 = await SaveAgentLogAsync(request.PatientId, "GetPatientHistory",
            JsonSerializer.Serialize(new { patientId = request.PatientId }),
            historyResult.ResultJson,
            "Step 2: Historical medical profile and prior bookings retrieved");
        logIds.Add(log2.Id);

        // Step 3: Tool Call - FormatIntakeSummary(rawText)
        var summaryResult = await ToolFormatIntakeSummaryAsync(request.RawText);
        var log3 = await SaveAgentLogAsync(request.PatientId, "FormatIntakeSummary",
            JsonSerializer.Serialize(new { rawText = request.RawText }),
            summaryResult.ResultJson,
            "Step 3: Unstructured text structured into symptoms, severity, and preferences");
        logIds.Add(log3.Id);

        // Step 4: Synthesize Structured Execution Contract
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PatientId);

        if (patient == null)
        {
            return (false, "Patient not found.", null);
        }

        var parsedSummary = JsonSerializer.Deserialize<IntakeParsedSummaryPayload>(summaryResult.ResultJson, JsonOptions) ?? new IntakeParsedSummaryPayload();

        var structuredResponse = new IntakeStructuredResponseDto
        {
            PatientId = patient.Id,
            PatientMetadata = new ValidatedPatientMetadataDto
            {
                PatientId = patient.Id,
                Name = patient.Name,
                Age = DateTime.UtcNow.Year - patient.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < patient.DateOfBirth.DayOfYear ? 1 : 0),
                Gender = patient.Gender,
                NIC = patient.NIC,
                PhoneNumber = patient.PhoneNumber,
                EmergencyContact = patient.EmergencyContact,
                BloodGroup = patient.BloodGroup,
                Allergies = patient.Allergies,
                MedicalHistory = patient.MedicalHistory,
                IsEligible = true,
                EligibilityStatus = "Verified & Eligible"
            },
            Symptoms = parsedSummary.Symptoms,
            SeverityFlags = new IntakeSeverityFlagsDto
            {
                UrgencyLevel = parsedSummary.UrgencyLevel,
                IsEmergency = parsedSummary.IsEmergency,
                RequiresImmediateAttention = parsedSummary.IsEmergency || parsedSummary.UrgencyLevel == UrgencyLevel.High,
                RedFlagsDetected = parsedSummary.RedFlags,
                UrgencyNotes = parsedSummary.UrgencyNotes
            },
            PreferredTimeWindows = parsedSummary.PreferredTimeWindows,
            DoctorPreferences = new IntakeDoctorPreferencesDto
            {
                PreferredDoctorName = parsedSummary.PreferredDoctor,
                PreferredSpecialty = parsedSummary.PreferredSpecialty,
                Notes = parsedSummary.DoctorPreferenceNotes
            },
            SummaryText = parsedSummary.FormattedSummary,
            ExecutionPlan = new List<string>
            {
                "Step 1: Patient identity & eligibility verified (No blocking safety locks)",
                "Step 2: Medical history & pre-existing conditions referenced",
                "Step 3: Unstructured symptom text structured with duration & severity ratings",
                parsedSummary.IsEmergency
                    ? "Step 4: [SAFETY TRIGGER] Red flags identified. Escalating to Emergency Triage & Admin Oversight"
                    : $"Step 4: Forwarding structured package to Component C (Triage: {parsedSummary.PreferredSpecialty ?? "General Practice"}) and Component B (Scheduling)"
            },
            ProcessedAt = DateTime.UtcNow,
            GeneratedLogIds = logIds
        };

        return (true, null, structuredResponse);
    }

    public async Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolGetPatientHistoryAsync(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            return (false, $"Patient with ID {patientId} not found.", "{}");
        }

        var pastBookings = patient.Appointments
            .OrderByDescending(a => a.AppointmentDate)
            .Take(5)
            .Select(a => new
            {
                appointmentId = a.Id,
                date = a.AppointmentDate.ToString("yyyy-MM-dd"),
                status = a.Status.ToString(),
                reason = a.ReasonForVisit
            })
            .ToList();

        var payload = new
        {
            patientId = patient.Id,
            name = patient.Name,
            bloodGroup = patient.BloodGroup ?? "Unknown",
            knownAllergies = patient.Allergies ?? "None recorded",
            preExistingConditions = patient.MedicalHistory ?? "None recorded",
            recentAppointmentsCount = patient.Appointments.Count,
            recentAppointments = pastBookings,
            retrievedAt = DateTime.UtcNow
        };

        return (true, null, JsonSerializer.Serialize(payload, JsonOptions));
    }

    public Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolFormatIntakeSummaryAsync(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return Task.FromResult((false, (string?)"Input text cannot be empty.", "{}"));
        }

        var text = rawText.ToLower();

        // 1. Detect Symptoms
        var symptoms = new List<StructuredSymptomDto>();
        var symptomKeywords = new Dictionary<string, (string DefaultSeverity, string Category)>
        {
            { "chest pain", ("Severe", "Cardiology") },
            { "shortness of breath", ("Severe", "Pulmonology/Cardiology") },
            { "difficulty breathing", ("Severe", "Pulmonology/Cardiology") },
            { "heart palpitations", ("Moderate", "Cardiology") },
            { "numbness", ("Moderate", "Neurology") },
            { "headache", ("Moderate", "Neurology") },
            { "migraine", ("Moderate", "Neurology") },
            { "fever", ("Moderate", "General") },
            { "high fever", ("Severe", "General") },
            { "cough", ("Mild", "General/Pulmonology") },
            { "sore throat", ("Mild", "ENT") },
            { "stomach pain", ("Moderate", "Gastroenterology") },
            { "abdominal pain", ("Moderate", "Gastroenterology") },
            { "vomiting", ("Moderate", "Gastroenterology") },
            { "dizziness", ("Moderate", "Neurology/General") },
            { "rash", ("Mild", "Dermatology") },
            { "skin itch", ("Mild", "Dermatology") },
            { "back pain", ("Moderate", "Orthopedics") },
            { "knee pain", ("Moderate", "Orthopedics") },
            { "joint pain", ("Moderate", "Rheumatology/Orthopedics") },
            { "blurred vision", ("Severe", "Ophthalmology") }
        };

        foreach (var kvp in symptomKeywords)
        {
            if (text.Contains(kvp.Key))
            {
                symptoms.Add(new StructuredSymptomDto
                {
                    Keyword = kvp.Key,
                    Severity = kvp.Value.DefaultSeverity,
                    Duration = ExtractDuration(text),
                    Notes = $"Mapped to potential category: {kvp.Value.Category}"
                });
            }
        }

        if (symptoms.Count == 0)
        {
            symptoms.Add(new StructuredSymptomDto
            {
                Keyword = "General discomfort / Unspecified symptoms",
                Severity = "Mild",
                Duration = ExtractDuration(text),
                Notes = "No predefined high-specificity keywords matched; marked for general evaluation."
            });
        }

        // 2. Red Flags & Urgency Determination
        var redFlags = new List<string>();
        var isEmergency = false;
        var urgency = UrgencyLevel.Medium;

        if (text.Contains("chest pain") || (text.Contains("shortness of breath") && text.Contains("severe")) ||
            text.Contains("unconscious") || text.Contains("fainted") || text.Contains("stroke") ||
            (text.Contains("left arm") && text.Contains("pain")))
        {
            isEmergency = true;
            urgency = UrgencyLevel.Emergency;
            redFlags.Add("Acute cardiac or respiratory distress indicator detected");
        }
        else if (text.Contains("high fever") || text.Contains("severe pain") || text.Contains("bleeding") || text.Contains("blurred vision"))
        {
            urgency = UrgencyLevel.High;
            redFlags.Add("High-severity condition indicator detected");
        }
        else if (text.Contains("mild") || text.Contains("checkup") || text.Contains("routine") || text.Contains("allergy"))
        {
            urgency = UrgencyLevel.Low;
        }

        // 3. Preferred Time Windows
        var preferredTimeWindows = new List<string>();
        if (text.Contains("morning")) preferredTimeWindows.Add("Morning (08:00 - 12:00)");
        if (text.Contains("afternoon")) preferredTimeWindows.Add("Afternoon (12:00 - 16:00)");
        if (text.Contains("evening") || text.Contains("night") || text.Contains("after work")) preferredTimeWindows.Add("Evening (16:00 - 20:00)");
        if (text.Contains("tomorrow")) preferredTimeWindows.Add("Tomorrow");
        if (text.Contains("weekend") || text.Contains("saturday") || text.Contains("sunday")) preferredTimeWindows.Add("Weekend");
        if (preferredTimeWindows.Count == 0) preferredTimeWindows.Add("Any available slot (Earliest preferred)");

        // 4. Doctor / Specialty Preferences
        string? preferredDoctor = null;
        string? preferredSpecialty = null;

        var doctorMatch = Regex.Match(rawText, @"(dr\.|doctor)\s+([a-zA-Z]+)", RegexOptions.IgnoreCase);
        if (doctorMatch.Success)
        {
            preferredDoctor = doctorMatch.Value.Trim();
        }

        if (text.Contains("cardio") || text.Contains("heart")) preferredSpecialty = "Cardiology";
        else if (text.Contains("derma") || text.Contains("skin")) preferredSpecialty = "Dermatology";
        else if (text.Contains("neuro") || text.Contains("brain") || text.Contains("nerves")) preferredSpecialty = "Neurology";
        else if (text.Contains("ortho") || text.Contains("bone") || text.Contains("joint")) preferredSpecialty = "Orthopedics";
        else if (text.Contains("pediatric") || text.Contains("child") || text.Contains("baby")) preferredSpecialty = "Pediatrics";
        else if (text.Contains("ent") || text.Contains("ear") || text.Contains("nose") || text.Contains("throat")) preferredSpecialty = "ENT";

        var payload = new IntakeParsedSummaryPayload
        {
            Symptoms = symptoms,
            UrgencyLevel = urgency,
            IsEmergency = isEmergency,
            RedFlags = redFlags,
            UrgencyNotes = isEmergency
                ? "Immediate escalation required due to emergency red flags."
                : $"Standard appointment recommended at {urgency} urgency.",
            PreferredTimeWindows = preferredTimeWindows,
            PreferredDoctor = preferredDoctor,
            PreferredSpecialty = preferredSpecialty,
            DoctorPreferenceNotes = preferredDoctor != null ? $"Requested specific physician: {preferredDoctor}" : null,
            FormattedSummary = $"Patient reports: {string.Join(", ", symptoms.Select(s => s.Keyword))}. Urgency: {urgency}. Preferred Window: {string.Join(", ", preferredTimeWindows)}."
        };

        return Task.FromResult((true, (string?)null, JsonSerializer.Serialize(payload, JsonOptions)));
    }

    public async Task<(bool Success, string? ErrorMessage, string ResultJson)> ToolValidatePatientEligibilityAsync(int patientId)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            var notFoundPayload = new
            {
                patientId,
                isEligible = false,
                reason = $"Patient ID {patientId} not found in hospital directory.",
                validatedAt = DateTime.UtcNow
            };
            return (true, null, JsonSerializer.Serialize(notFoundPayload));
        }

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(patient.PhoneNumber)) missingFields.Add("PhoneNumber");
        if (string.IsNullOrWhiteSpace(patient.EmergencyContact)) missingFields.Add("EmergencyContact");
        if (string.IsNullOrWhiteSpace(patient.NIC)) missingFields.Add("NIC");

        var isEligible = missingFields.Count == 0;
        var resultPayload = new
        {
            patientId = patient.Id,
            isEligible,
            reason = isEligible ? "Patient profile is complete and verified." : $"Patient profile missing mandatory contact fields: {string.Join(", ", missingFields)}.",
            patientName = patient.Name,
            nic = patient.NIC,
            missingFields,
            validatedAt = DateTime.UtcNow
        };

        return (true, null, JsonSerializer.Serialize(resultPayload, JsonOptions));
    }

    public async Task<List<IntakeAgentLogDto>> GetLogsByPatientIdAsync(int patientId)
    {
        var logs = await _context.IntakeAgentLogs
            .Include(l => l.Patient)
            .Where(l => l.PatientId == patientId)
            .OrderByDescending(l => l.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return logs.Select(MapToLogDto).ToList();
    }

    public async Task<PagedResult<IntakeAgentLogDto>> GetAllLogsAsync(int page = 1, int pageSize = 20)
    {
        var p = page > 0 ? page : 1;
        var ps = pageSize > 0 ? pageSize : 20;

        var query = _context.IntakeAgentLogs
            .Include(l => l.Patient)
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync();

        return new PagedResult<IntakeAgentLogDto>
        {
            Items = items.Select(MapToLogDto),
            TotalCount = totalCount,
            Page = p,
            PageSize = ps
        };
    }

    private async Task<IntakeAgentLog> SaveAgentLogAsync(int patientId, string toolCalled, string inputJson, string outputJson, string notes)
    {
        var log = new IntakeAgentLog
        {
            PatientId = patientId,
            ToolCalled = toolCalled,
            InputPayload = inputJson,
            OutputPayload = outputJson,
            SessionNotes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IntakeAgentLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    private static string? ExtractDuration(string text)
    {
        var match = Regex.Match(text, @"(\d+\s+(day|days|week|weeks|month|months|hour|hours))|since\s+(yesterday|today|last\s+week)", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static IntakeAgentLogDto MapToLogDto(IntakeAgentLog l)
    {
        return new IntakeAgentLogDto
        {
            Id = l.Id,
            PatientId = l.PatientId,
            PatientName = l.Patient?.Name ?? "Unknown",
            ToolCalled = l.ToolCalled,
            InputPayload = l.InputPayload,
            OutputPayload = l.OutputPayload,
            SessionNotes = l.SessionNotes,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        };
    }
}

internal class IntakeParsedSummaryPayload
{
    public List<StructuredSymptomDto> Symptoms { get; set; } = new();
    public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Medium;
    public bool IsEmergency { get; set; }
    public List<string> RedFlags { get; set; } = new();
    public string UrgencyNotes { get; set; } = string.Empty;
    public List<string> PreferredTimeWindows { get; set; } = new();
    public string? PreferredDoctor { get; set; }
    public string? PreferredSpecialty { get; set; }
    public string? DoctorPreferenceNotes { get; set; }
    public string FormattedSummary { get; set; } = string.Empty;
}
