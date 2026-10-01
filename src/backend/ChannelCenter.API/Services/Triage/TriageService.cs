using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Triage;
using ChannelCenter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelCenter.API.Services.Triage;

public class TriageService : ITriageService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory? _httpClientFactory;

    public TriageService(ApplicationDbContext context, IHttpClientFactory? httpClientFactory = null)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<QuestionnaireResponseDto> SubmitQuestionnaireAsync(CreateQuestionnaireDto dto)
    {
        var questionnaire = new PreConsultationQuestionnaire
        {
            PatientId = dto.PatientId,
            ResponsesData = dto.ResponsesData,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PreConsultationQuestionnaires.Add(questionnaire);
        await _context.SaveChangesAsync();

        return new QuestionnaireResponseDto
        {
            Id = questionnaire.Id,
            PatientId = questionnaire.PatientId,
            ResponsesData = questionnaire.ResponsesData,
            CreatedAt = questionnaire.CreatedAt
        };
    }

    public async Task<QuestionnaireResponseDto?> GetQuestionnaireByIdAsync(int id)
    {
        var q = await _context.PreConsultationQuestionnaires.FindAsync(id);
        if (q == null) return null;

        return new QuestionnaireResponseDto
        {
            Id = q.Id,
            PatientId = q.PatientId,
            ResponsesData = q.ResponsesData,
            CreatedAt = q.CreatedAt
        };
    }

    public async Task<TriageAssessmentDto> ProcessTriageAsync(ProcessTriageDto dto)
    {
        int calculatedScore = 0;
        UrgencyLevel level = UrgencyLevel.Low;
        string matchedSpecialty = "General Medicine";
        string reasoningTrace = string.Empty;
        bool aiSuccess = false;

        // Try Python AI Agent Service first if available
        if (_httpClientFactory != null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AiService");
                var payload = new
                {
                    appointment_id = dto.AppointmentId,
                    raw_symptoms = dto.RawSymptoms ?? "",
                    symptom_list = dto.SymptomList.Select(s => new
                    {
                        symptom_keyword = s.SymptomKeyword,
                        severity_rating = s.SeverityRating ?? 1,
                        duration_in_days = s.DurationInDays
                    }).ToList()
                };

                var requestMsg = new HttpRequestMessage(HttpMethod.Post, "/internal/v1/triage/assess")
                {
                    Content = JsonContent.Create(payload)
                };
                requestMsg.Headers.Add("X-Internal-Service-Key", "ChannelCenterInternalKey2026_MustBeSecure!");

                var response = await client.SendAsync(requestMsg);
                if (response.IsSuccessStatusCode)
                {
                    var aiResult = await response.Content.ReadFromJsonAsync<TriageAiResultDto>();
                    if (aiResult != null)
                    {
                        calculatedScore = aiResult.UrgencyScore;
                        level = Enum.TryParse<UrgencyLevel>(aiResult.UrgencyLevel, true, out var parsedLevel)
                            ? parsedLevel
                            : UrgencyLevel.Medium;
                        matchedSpecialty = aiResult.RecommendedSpecialty;
                        reasoningTrace = aiResult.ReasoningTrace;
                        aiSuccess = true;
                    }
                }
            }
            catch
            {
                // Fallback to local rule engine if AI service is unavailable
            }
        }

        if (!aiSuccess)
        {
            // 1. Calculate urgency score & red flag detection
            int maxSeverity = dto.SymptomList.Any() ? dto.SymptomList.Max(s => s.SeverityRating ?? 1) : 1;
            string combinedSymptoms = ((dto.RawSymptoms ?? "") + " " + string.Join(" ", dto.SymptomList.Select(s => s.SymptomKeyword))).ToLowerInvariant();

            bool hasEmergencyRedFlag = combinedSymptoms.Contains("chest pain") || combinedSymptoms.Contains("shortness of breath") || 
                                       combinedSymptoms.Contains("stroke") || combinedSymptoms.Contains("unconscious") || combinedSymptoms.Contains("severe bleeding");

            calculatedScore = hasEmergencyRedFlag ? Math.Max(92, maxSeverity * 10) : maxSeverity * 10;

            level = calculatedScore switch
            {
                >= 80 => UrgencyLevel.Emergency,
                >= 60 => UrgencyLevel.High,
                >= 30 => UrgencyLevel.Medium,
                _ => UrgencyLevel.Low
            };

            // 2. Specialty Matching (direct string recommended specialty)
            if (combinedSymptoms.Contains("chest pain") || combinedSymptoms.Contains("heart") || combinedSymptoms.Contains("palpitation") || combinedSymptoms.Contains("cardiac"))
            {
                matchedSpecialty = "Cardiology";
            }
            else if (combinedSymptoms.Contains("rash") || combinedSymptoms.Contains("skin") || combinedSymptoms.Contains("itch") || combinedSymptoms.Contains("acne") || combinedSymptoms.Contains("lesion"))
            {
                matchedSpecialty = "Dermatology";
            }
            else if (combinedSymptoms.Contains("headache") || combinedSymptoms.Contains("numbness") || combinedSymptoms.Contains("dizziness") || combinedSymptoms.Contains("seizure") || combinedSymptoms.Contains("stroke") || combinedSymptoms.Contains("migraine"))
            {
                matchedSpecialty = "Neurology";
            }
            else if (combinedSymptoms.Contains("joint") || combinedSymptoms.Contains("knee") || combinedSymptoms.Contains("bone") || combinedSymptoms.Contains("fracture") || combinedSymptoms.Contains("back pain") || combinedSymptoms.Contains("sprain"))
            {
                matchedSpecialty = "Orthopedics";
            }
            else if (combinedSymptoms.Contains("stomach") || combinedSymptoms.Contains("nausea") || combinedSymptoms.Contains("vomiting") || combinedSymptoms.Contains("acid") || combinedSymptoms.Contains("reflux") || combinedSymptoms.Contains("diarrhea"))
            {
                matchedSpecialty = "Gastroenterology";
            }

            reasoningTrace = $"[Symptom Triage Agent] Parsed {dto.SymptomList.Count} symptom entries for raw symptoms: '{dto.RawSymptoms}'. Highest severity score: {maxSeverity}/10. Emergency red flags: {(hasEmergencyRedFlag ? "Detected" : "None")}. Matched specialty: {matchedSpecialty}. Assessed urgency level: {level} (Score: {calculatedScore}/100).";
        }

        // 3. Create TriageAssessment Record with AppointmentId
        var assessment = new TriageAssessment
        {
            AppointmentId = dto.AppointmentId,
            RawSymptoms = dto.RawSymptoms,
            UrgencyScore = calculatedScore,
            UrgencyLevel = level,
            ReasoningTrace = reasoningTrace,
            RecommendedSpecialty = matchedSpecialty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TriageAssessments.Add(assessment);
        await _context.SaveChangesAsync();

        // 4. Save Symptom Logs
        foreach (var item in dto.SymptomList)
        {
            _context.SymptomLogs.Add(new SymptomLog
            {
                TriageId = assessment.Id,
                SymptomKeyword = item.SymptomKeyword,
                SeverityRating = item.SeverityRating,
                DurationInDays = item.DurationInDays,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // 5. Automatically Generate Referral with direct string TargetSpecialty
        var referral = new Referral
        {
            TriageId = assessment.Id,
            TargetSpecialty = matchedSpecialty,
            Status = ReferralStatus.Generated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        return new TriageAssessmentDto
        {
            Id = assessment.Id,
            AppointmentId = assessment.AppointmentId,
            RawSymptoms = assessment.RawSymptoms,
            UrgencyScore = assessment.UrgencyScore,
            UrgencyLevel = assessment.UrgencyLevel,
            ReasoningTrace = assessment.ReasoningTrace,
            RecommendedSpecialty = assessment.RecommendedSpecialty,
            SymptomLogs = dto.SymptomList,
            CreatedAt = assessment.CreatedAt
        };
    }

    public async Task<TriageAssessmentDto?> GetTriageAssessmentByIdAsync(int id)
    {
        var a = await _context.TriageAssessments.FirstOrDefaultAsync(t => t.Id == id);
        if (a == null) return null;

        var symptomLogs = await _context.SymptomLogs
            .Where(s => s.TriageId == a.Id)
            .Select(s => new SymptomItemDto
            {
                SymptomKeyword = s.SymptomKeyword,
                SeverityRating = s.SeverityRating,
                DurationInDays = s.DurationInDays
            }).ToListAsync();

        return new TriageAssessmentDto
        {
            Id = a.Id,
            AppointmentId = a.AppointmentId,
            RawSymptoms = a.RawSymptoms,
            UrgencyScore = a.UrgencyScore,
            UrgencyLevel = a.UrgencyLevel,
            ReasoningTrace = a.ReasoningTrace,
            RecommendedSpecialty = a.RecommendedSpecialty,
            SymptomLogs = symptomLogs,
            CreatedAt = a.CreatedAt
        };
    }

    public async Task<IEnumerable<TriageAssessmentDto>> GetTriageHistoryByAppointmentAsync(int appointmentId)
    {
        var assessments = await _context.TriageAssessments
            .Where(t => t.AppointmentId == appointmentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var result = new List<TriageAssessmentDto>();
        foreach (var a in assessments)
        {
            var symptomLogs = await _context.SymptomLogs
                .Where(s => s.TriageId == a.Id)
                .Select(s => new SymptomItemDto
                {
                    SymptomKeyword = s.SymptomKeyword,
                    SeverityRating = s.SeverityRating,
                    DurationInDays = s.DurationInDays
                }).ToListAsync();

            result.Add(new TriageAssessmentDto
            {
                Id = a.Id,
                AppointmentId = a.AppointmentId,
                RawSymptoms = a.RawSymptoms,
                UrgencyScore = a.UrgencyScore,
                UrgencyLevel = a.UrgencyLevel,
                ReasoningTrace = a.ReasoningTrace,
                RecommendedSpecialty = a.RecommendedSpecialty,
                SymptomLogs = symptomLogs,
                CreatedAt = a.CreatedAt
            });
        }

        return result;
    }

    private async Task<TriageAssessmentDto?> GetTriageSummaryAsync(int triageId)
    {
        var a = await _context.TriageAssessments
            .Include(t => t.Appointment)
                .ThenInclude(app => app!.Patient)
            .Include(t => t.Appointment)
                .ThenInclude(app => app!.Doctor)
                    .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(t => t.Id == triageId);
        if (a == null) return null;

        var symptomLogs = await _context.SymptomLogs
            .Where(s => s.TriageId == a.Id)
            .Select(s => new SymptomItemDto
            {
                SymptomKeyword = s.SymptomKeyword,
                SeverityRating = s.SeverityRating,
                DurationInDays = s.DurationInDays
            }).ToListAsync();

        return new TriageAssessmentDto
        {
            Id = a.Id,
            AppointmentId = a.AppointmentId,
            PatientName = a.Appointment?.Patient?.Name,
            DoctorName = a.Appointment?.Doctor?.User?.FullName,
            AppointmentDate = a.Appointment?.AppointmentDate,
            RawSymptoms = a.RawSymptoms,
            UrgencyScore = a.UrgencyScore,
            UrgencyLevel = a.UrgencyLevel,
            ReasoningTrace = a.ReasoningTrace,
            RecommendedSpecialty = a.RecommendedSpecialty,
            SymptomLogs = symptomLogs,
            CreatedAt = a.CreatedAt
        };
    }

    public async Task<ReferralDto> CreateReferralAsync(CreateReferralDto dto)
    {
        var referral = new Referral
        {
            TriageId = dto.TriageId,
            TargetSpecialty = dto.TargetSpecialty,
            Status = ReferralStatus.Generated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var summary = await GetTriageSummaryAsync(referral.TriageId);

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            TargetSpecialty = referral.TargetSpecialty,
            Status = referral.Status,
            TriageSummary = summary,
            CreatedAt = referral.CreatedAt
        };
    }

    public async Task<ReferralDto?> UpdateReferralStatusAsync(int referralId, UpdateReferralStatusDto dto)
    {
        var referral = await _context.Referrals.FirstOrDefaultAsync(r => r.Id == referralId);
        if (referral == null) return null;

        referral.Status = dto.Status;
        referral.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var summary = await GetTriageSummaryAsync(referral.TriageId);

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            TargetSpecialty = referral.TargetSpecialty,
            Status = referral.Status,
            TriageSummary = summary,
            CreatedAt = referral.CreatedAt
        };
    }

    public async Task<IEnumerable<ReferralDto>> GetAllReferralsAsync()
    {
        var referrals = await _context.Referrals
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = new List<ReferralDto>();
        foreach (var r in referrals)
        {
            var summary = await GetTriageSummaryAsync(r.TriageId);
            result.Add(new ReferralDto
            {
                Id = r.Id,
                TriageId = r.TriageId,
                TargetSpecialty = r.TargetSpecialty,
                Status = r.Status,
                TriageSummary = summary,
                CreatedAt = r.CreatedAt
            });
        }

        return result;
    }

    public Task<IEnumerable<string>> GetSpecialtiesAsync()
    {
        var specialties = new List<string>
        {
            "Cardiology",
            "Dermatology",
            "Neurology",
            "Orthopedics",
            "Gastroenterology",
            "General Medicine"
        };

        return Task.FromResult<IEnumerable<string>>(specialties);
    }
}

internal class TriageAiResultDto
{
    [JsonPropertyName("urgency_score")]
    public int UrgencyScore { get; set; }

    [JsonPropertyName("urgency_level")]
    public string UrgencyLevel { get; set; } = "Low";

    [JsonPropertyName("recommended_specialty")]
    public string RecommendedSpecialty { get; set; } = "General Medicine";

    [JsonPropertyName("reasoning_trace")]
    public string ReasoningTrace { get; set; } = string.Empty;

    [JsonPropertyName("matched_condition")]
    public string MatchedCondition { get; set; } = string.Empty;
}