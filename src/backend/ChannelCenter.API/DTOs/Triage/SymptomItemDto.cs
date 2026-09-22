using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Triage;

public class SymptomItemDto
{
    [Required]
    [MaxLength(100)]
    public string SymptomKeyword { get; set; } = string.Empty;

    [Range(1, 10)]
    public int? SeverityRating { get; set; }

    [Range(1, 365)]
    public int DurationInDays { get; set; }
}