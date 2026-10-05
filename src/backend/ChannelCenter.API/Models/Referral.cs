namespace ChannelCenter.API.Models;

public class Referral
{
    public int Id { get; set; }
    public int TriageId { get; set; }
    public string TargetSpecialty { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; } = ReferralStatus.Generated;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TriageAssessment? Triage { get; set; }
}