using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Triage;

public class CreateReferralDto
{
    [Required]
    public int TriageId { get; set; }

    [Required]
    public int TargetSpecialtyId { get; set; }
}