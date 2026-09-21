namespace ChannelCenter.API.DTOs.Admin;

public class AiSafetyMetricsDto
{
    public int TotalWorkflows { get; set; }
    public int HighImpactPausedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int RevisedCount { get; set; }
    public int ManualOverridesCount { get; set; }
    public double HumanInterventionRate { get; set; }
}
