using ChannelCenter.API.DTOs.Triage;

namespace ChannelCenter.API.Services.Triage;

public interface ITriageService
{
    // Questionnaire Operations
    Task<QuestionnaireResponseDto> SubmitQuestionnaireAsync(CreateQuestionnaireDto dto);
    Task<QuestionnaireResponseDto?> GetQuestionnaireByIdAsync(int id);

    // Triage Operations
    Task<TriageAssessmentDto> ProcessTriageAsync(ProcessTriageDto dto);
    Task<TriageAssessmentDto?> GetTriageAssessmentByIdAsync(int id);
    Task<IEnumerable<TriageAssessmentDto>> GetPatientTriageHistoryAsync(int patientId);

    // Referral Operations
    Task<ReferralDto> CreateReferralAsync(CreateReferralDto dto);
    Task<ReferralDto?> UpdateReferralStatusAsync(int referralId, UpdateReferralStatusDto dto);
    Task<IEnumerable<ReferralDto>> GetAllReferralsAsync();

    // Specialty Operations
    Task<IEnumerable<SpecialtyDto>> GetSpecialtiesAsync();
}