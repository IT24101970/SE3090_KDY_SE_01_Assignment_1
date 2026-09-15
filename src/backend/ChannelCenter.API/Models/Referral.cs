namespace ChannelCenter.API.Models;

public class Referral
{
    public int Id { get; set; }
    public int TriageId { get; set; }
    public int TargetSpecialtyId { get; set; }
    public ReferralStatus Status { get; set; } = ReferralStatus.Generated;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public TriageAssessment? Triage { get; set; }
    public Specialty? TargetSpecialty { get; set; }
}