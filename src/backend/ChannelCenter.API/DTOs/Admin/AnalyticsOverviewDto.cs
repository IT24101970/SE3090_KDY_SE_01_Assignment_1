namespace ChannelCenter.API.DTOs.Admin;

public class AnalyticsOverviewDto
{
    public int ActiveWorkflowsCount { get; set; }
    public int PausedWorkflowsCount { get; set; }
    public int CompletedWorkflowsCount { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int EmergencyCasesCount { get; set; }
    public int TotalDoctorsCount { get; set; }
    public double SystemApprovalRate { get; set; }
}
