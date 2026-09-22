using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.IntakeAgent;

public class IntakeProcessRequestDto
{
    [Required(ErrorMessage = "PatientId is required.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Raw patient intake text is required.")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "Raw text must be between 3 and 2000 characters.")]
    public string RawText { get; set; } = string.Empty;
}

