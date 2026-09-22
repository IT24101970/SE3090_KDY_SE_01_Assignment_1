namespace ChannelCenter.API.Models;

public class Patient
{
    public int Id { get; set; }
    public int? UserId { get; set; }                                 // Links to the User account
    public string Name { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;              // e.g. "Male", "Female", "Other"
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NIC { get; set; } = string.Empty;                 // National Identity Card / gov ID
    public string? BloodGroup { get; set; }                         // e.g., "A+", "O-", etc.
    public string? Allergies { get; set; }                          // Known allergies
    public string? MedicalHistory { get; set; }                     // Chronic illnesses, prior conditions
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User? User { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<IntakeAgentLog> IntakeAgentLogs { get; set; } = new List<IntakeAgentLog>();
}