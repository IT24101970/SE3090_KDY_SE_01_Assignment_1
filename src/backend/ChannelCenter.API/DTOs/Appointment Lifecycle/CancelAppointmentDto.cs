using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Appointment;

public class CancelAppointmentDto
{
    [Required(ErrorMessage = "Cancellation reason is required.")]
    [StringLength(300, MinimumLength = 3, ErrorMessage = "Cancellation reason must be between 3 and 300 characters.")]
    public string CancelReason { get; set; } = string.Empty;
}
