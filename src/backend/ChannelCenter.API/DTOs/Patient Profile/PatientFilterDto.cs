namespace ChannelCenter.API.DTOs.Patient;

public class PatientFilterDto
{
    public string? SearchTerm { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
