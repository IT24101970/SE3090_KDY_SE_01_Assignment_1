using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class OverrideReassignRequestDto
{
    [Required]
    public int TargetDoctorId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}