namespace ChannelCenter.API.DTOs.DoctorScheduling;

public class ConsultationRoomDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateConsultationRoomDto
{
    public string RoomName { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateConsultationRoomDto
{
    public string RoomName { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
