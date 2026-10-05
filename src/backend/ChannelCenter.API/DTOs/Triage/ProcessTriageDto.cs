using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Triage;

public class ProcessTriageDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public string RawSymptoms { get; set; } = string.Empty;

    public List<SymptomItemDto> SymptomList { get; set; } = new();
}