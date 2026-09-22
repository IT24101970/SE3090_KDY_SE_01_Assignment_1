using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Triage;

public class ReferralDto
{
    public int Id { get; set; }
    public int TriageId { get; set; }
    public SpecialtyDto? TargetSpecialty { get; set; }
    public ReferralStatus Status { get; set; }
    public TriageAssessmentDto? TriageSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}