using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Admin;

public class OverrideCancelRequestDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}