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
        // 1. Calculate urgency based on symptom severity ratings
        int maxSeverity = dto.SymptomList.Any() ? dto.SymptomList.Max(s => s.SeverityRating ?? 1) : 1;
        int calculatedScore = maxSeverity * 10;

        UrgencyLevel level = calculatedScore switch
        {
            >= 80 => UrgencyLevel.Emergency,
            >= 60 => UrgencyLevel.High,
            >= 30 => UrgencyLevel.Medium,
            _ => UrgencyLevel.Low
        };

        // 2. Specialty Matching Logic (Default to General Medicine if none matched)
        var defaultSpecialty = await _context.Specialties.FirstOrDefaultAsync() 
            ?? new Specialty { Name = "General Medicine", Description = "General health care" };

        if (defaultSpecialty.Id == 0)
        {
            _context.Specialties.Add(defaultSpecialty);
            await _context.SaveChangesAsync();
        }

        // 3. Create Assessment Record
        var assessment = new TriageAssessment
        {
            PatientId = dto.PatientId,
            QuestionnaireId = dto.QuestionnaireId,
            RawSymptoms = dto.RawSymptoms,
            UrgencyScore = calculatedScore,
            UrgencyLevel = level,
            ReasoningTrace = $"Evaluated {dto.SymptomList.Count} symptoms. Highest severity rating: {maxSeverity}/10. Recommended urgency classification: {level}.",
            RecommendedSpecialtyId = defaultSpecialty.Id,
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

        // 5. Automatically Generate Referral
        var referral = new Referral
        {
            TriageId = assessment.Id,
            TargetSpecialtyId = defaultSpecialty.Id,
            Status = ReferralStatus.Generated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        return new TriageAssessmentDto
        {
            Id = assessment.Id,
            PatientId = assessment.PatientId,
            QuestionnaireId = assessment.QuestionnaireId,
            RawSymptoms = assessment.RawSymptoms,
            UrgencyScore = assessment.UrgencyScore,
            UrgencyLevel = assessment.UrgencyLevel,
            ReasoningTrace = assessment.ReasoningTrace,
            RecommendedSpecialty = new SpecialtyDto
            {
                Id = defaultSpecialty.Id,
                Name = defaultSpecialty.Name,
                Description = defaultSpecialty.Description
            },
            SymptomLogs = dto.SymptomList,
            CreatedAt = assessment.CreatedAt
        };
    }

    public async Task<TriageAssessmentDto?> GetTriageAssessmentByIdAsync(int id)
    {
        var a = await _context.TriageAssessments
            .Include(t => t.RecommendedSpecialty)
            .FirstOrDefaultAsync(t => t.Id == id);

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
            PatientId = a.PatientId,
            QuestionnaireId = a.QuestionnaireId,
            RawSymptoms = a.RawSymptoms,
            UrgencyScore = a.UrgencyScore,
            UrgencyLevel = a.UrgencyLevel,
            ReasoningTrace = a.ReasoningTrace,
            RecommendedSpecialty = a.RecommendedSpecialty == null ? null : new SpecialtyDto
            {
                Id = a.RecommendedSpecialty.Id,
                Name = a.RecommendedSpecialty.Name,
                Description = a.RecommendedSpecialty.Description
            },
            SymptomLogs = symptomLogs,
            CreatedAt = a.CreatedAt
        };
    }

    public async Task<IEnumerable<TriageAssessmentDto>> GetPatientTriageHistoryAsync(int patientId)
    {
        var assessments = await _context.TriageAssessments
            .Include(t => t.RecommendedSpecialty)
            .Where(t => t.PatientId == patientId)
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
                PatientId = a.PatientId,
                QuestionnaireId = a.QuestionnaireId,
                RawSymptoms = a.RawSymptoms,
                UrgencyScore = a.UrgencyScore,
                UrgencyLevel = a.UrgencyLevel,
                ReasoningTrace = a.ReasoningTrace,
                RecommendedSpecialty = a.RecommendedSpecialty == null ? null : new SpecialtyDto
                {
                    Id = a.RecommendedSpecialty.Id,
                    Name = a.RecommendedSpecialty.Name,
                    Description = a.RecommendedSpecialty.Description
                },
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
            TargetSpecialtyId = dto.TargetSpecialtyId,
            Status = ReferralStatus.Generated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        var specialty = await _context.Specialties.FindAsync(dto.TargetSpecialtyId);

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            Status = referral.Status,
            CreatedAt = referral.CreatedAt,
            TargetSpecialty = specialty == null ? null : new SpecialtyDto
            {
                Id = specialty.Id,
                Name = specialty.Name,
                Description = specialty.Description
            }
        };
    }

    public async Task<ReferralDto?> UpdateReferralStatusAsync(int referralId, UpdateReferralStatusDto dto)
    {
        var referral = await _context.Referrals
            .Include(r => r.TargetSpecialty)
            .FirstOrDefaultAsync(r => r.Id == referralId);

        if (referral == null) return null;

        referral.Status = dto.Status;
        referral.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ReferralDto
        {
            Id = referral.Id,
            TriageId = referral.TriageId,
            Status = referral.Status,
            CreatedAt = referral.CreatedAt,
            TargetSpecialty = referral.TargetSpecialty == null ? null : new SpecialtyDto
            {
                Id = referral.TargetSpecialty.Id,
                Name = referral.TargetSpecialty.Name,
                Description = referral.TargetSpecialty.Description
            }
        };
    }

    public async Task<IEnumerable<ReferralDto>> GetAllReferralsAsync()
    {
        return await _context.Referrals
            .Include(r => r.TargetSpecialty)
            .Select(r => new ReferralDto
            {
                Id = r.Id,
                TriageId = r.TriageId,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                TargetSpecialty = r.TargetSpecialty == null ? null : new SpecialtyDto
                {
                    Id = r.TargetSpecialty.Id,
                    Name = r.TargetSpecialty.Name,
                    Description = r.TargetSpecialty.Description
                }
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SpecialtyDto>> GetSpecialtiesAsync()
    {
        return await _context.Specialties
            .Select(s => new SpecialtyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description
            })
            .ToListAsync();
    }
}