using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Triage;
using ChannelCenter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelCenter.API.Services.Triage;

public class TriageService : ITriageService
{
    private readonly ApplicationDbContext _context;

    public TriageService(ApplicationDbContext context)
    {
        _context = context;
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
        // 1. Calculate urgency score & red flag detection
        int maxSeverity = dto.SymptomList.Any() ? dto.SymptomList.Max(s => s.SeverityRating ?? 1) : 1;
        string combinedSymptoms = ((dto.RawSymptoms ?? "") + " " + string.Join(" ", dto.SymptomList.Select(s => s.SymptomKeyword))).ToLowerInvariant();

        bool hasEmergencyRedFlag = combinedSymptoms.Contains("chest pain") || combinedSymptoms.Contains("shortness of breath") || 
                                   combinedSymptoms.Contains("stroke") || combinedSymptoms.Contains("unconscious") || combinedSymptoms.Contains("severe bleeding");

        int calculatedScore = hasEmergencyRedFlag ? Math.Max(92, maxSeverity * 10) : maxSeverity * 10;

        UrgencyLevel level = calculatedScore switch
        {
            >= 80 => UrgencyLevel.Emergency,
            >= 60 => UrgencyLevel.High,
            >= 30 => UrgencyLevel.Medium,
            _ => UrgencyLevel.Low
        };

        // 2. Specialty Matching (direct string recommended specialty)
        string matchedSpecialty = "General Medicine";
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

        string reasoningTrace = $"[Symptom Triage Agent] Parsed {dto.SymptomList.Count} symptom entries for raw symptoms: '{dto.RawSymptoms}'. Highest severity score: {maxSeverity}/10. Emergency red flags: {(hasEmergencyRedFlag ? "Detected" : "None")}. Matched specialty: {matchedSpecialty}. Assessed urgency level: {level} (Score: {calculatedScore}/100).";

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

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            TargetSpecialty = referral.TargetSpecialty,
            Status = referral.Status,
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

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            TargetSpecialty = referral.TargetSpecialty,
            Status = referral.Status,
            CreatedAt = referral.CreatedAt
        };
    }

    public async Task<IEnumerable<ReferralDto>> GetAllReferralsAsync()
    {
        return await _context.Referrals
            .Select(r => new ReferralDto
            {
                Id = r.Id,
                TriageId = r.TriageId,
                TargetSpecialty = r.TargetSpecialty,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
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