using System.ComponentModel.DataAnnotations;

namespace ChannelCenter.API.DTOs.Appointment;

public class CreateAppointmentDto
{
    [Required(ErrorMessage = "PatientId is required.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "DoctorId is required.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "ScheduleId is required.")]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = "AppointmentDate is required.")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Reason for visit is required.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason for visit must be between 3 and 500 characters.")]
    public string ReasonForVisit { get; set; } = string.Empty;
}
