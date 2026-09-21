namespace ChannelCenter.API.DTOs.Admin;

public class TriageUrgencyRatioDto
{
    public string UrgencyLevel { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
