using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Patient;

public class UpdatePatientDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string? Name { get; set; }

    public string? EmergencyContact { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    public string? BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
}
