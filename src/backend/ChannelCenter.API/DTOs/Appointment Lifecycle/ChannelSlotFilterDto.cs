namespace ChannelCenter.API.DTOs.Appointment;

public class ChannelSlotFilterDto
{
    public int? SpecialtyId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? Date { get; set; }
    public bool? AvailableOnly { get; set; } = true;
}
