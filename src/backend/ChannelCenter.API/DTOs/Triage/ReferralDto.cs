using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Triage;

public class ReferralDto
{
    public int Id { get; set; }
    public int TriageId { get; set; }
    public string TargetSpecialty { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; }
    public TriageAssessmentDto? TriageSummary { get; set; }
    public DateTime CreatedAt { get; set; }
}