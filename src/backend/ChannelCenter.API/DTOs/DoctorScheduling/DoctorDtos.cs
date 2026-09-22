namespace ChannelCenter.API.DTOs.DoctorScheduling;

public class DoctorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SpecialtyId { get; set; }
    public string Qualifications { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDoctorDto
{
    public int UserId { get; set; }
    public int SpecialtyId { get; set; }
    public string Qualifications { get; set; } = string.Empty;
}

public class UpdateDoctorDto
{
    public int SpecialtyId { get; set; }
    public string Qualifications { get; set; } = string.Empty;
}
