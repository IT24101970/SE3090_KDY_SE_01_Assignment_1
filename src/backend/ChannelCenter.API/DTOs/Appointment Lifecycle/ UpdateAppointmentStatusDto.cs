using System.ComponentModel.DataAnnotations;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.DTOs.Appointment;

public class UpdateAppointmentStatusDto
{
    [Required(ErrorMessage = "New status is required.")]
    public AppointmentStatus Status { get; set; }

    public string? CancelReason { get; set; }
}
