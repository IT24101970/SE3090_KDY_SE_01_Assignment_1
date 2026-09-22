using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Triage;

public class CreateQuestionnaireDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public string ResponsesData { get; set; } = string.Empty;
}