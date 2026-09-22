using System.ComponentModel.DataAnnotations;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Triage;

public class UpdateReferralStatusDto
{
    [Required]
    public ReferralStatus Status { get; set; } // Generated, Reviewed, Assigned[cite: 1, 4]
}