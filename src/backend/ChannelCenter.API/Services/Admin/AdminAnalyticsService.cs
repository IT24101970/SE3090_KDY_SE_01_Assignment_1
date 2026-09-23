using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Admin;

public class AdminAnalyticsService : IAdminAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public AdminAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var activeWorkflows = await _context.AgentWorkflows.CountAsync(w => w.Status == WorkflowStatus.Running);
        var pausedWorkflows = await _context.AgentWorkflows.CountAsync(w => w.Status == WorkflowStatus.PausedForApproval);
        var completedWorkflows = await _context.AgentWorkflows.CountAsync(w => w.Status == WorkflowStatus.Completed);
        
        var todayAppointments = await _context.Appointments
            .CountAsync(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow);

        var emergencyCases = await _context.TriageAssessments
            .CountAsync(t => t.UrgencyLevel == UrgencyLevel.Emergency);

        var totalDoctors = await _context.Doctors.CountAsync();

        var totalDecisions = await _context.AdminApprovals.CountAsync();
        var approvedDecisions = await _context.AdminApprovals.CountAsync(a => a.Decision == ApprovalDecision.Approved);

        double approvalRate = totalDecisions > 0 
            ? Math.Round((double)approvedDecisions / totalDecisions * 100.0, 1) 
            : 100.0;

        return new AnalyticsOverviewDto
        {
            ActiveWorkflowsCount = activeWorkflows,
            PausedWorkflowsCount = pausedWorkflows,
            CompletedWorkflowsCount = completedWorkflows,
            TodayAppointmentsCount = todayAppointments,
            EmergencyCasesCount = emergencyCases,
            TotalDoctorsCount = totalDoctors,
            SystemApprovalRate = approvalRate
        };
    }

    public async Task<IEnumerable<DailyAppointmentVolumeDto>> GetDailyAppointmentVolumesAsync(
        DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = (startDate ?? DateTime.UtcNow.AddDays(-6)).Date;
        var end = (endDate ?? DateTime.UtcNow).Date.AddDays(1);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end)
            .ToListAsync();

        var grouped = appointments
            .GroupBy(a => a.AppointmentDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyAppointmentVolumeDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalAppointments = g.Count(),
                ConfirmedCount = g.Count(a => a.Status == AppointmentStatus.Confirmed),
                CancelledCount = g.Count(a => a.Status == AppointmentStatus.Cancelled),
                PendingCount = g.Count(a => a.Status == AppointmentStatus.Pending)
            })
            .ToList();

        return grouped;
    }

    public async Task<IEnumerable<TriageUrgencyRatioDto>> GetTriageRatiosAsync()
    {
        var assessments = await _context.TriageAssessments
            .AsNoTracking()
            .ToListAsync();

        int total = assessments.Count;
        if (total == 0)
        {
            return Enum.GetValues<UrgencyLevel>().Select(level => new TriageUrgencyRatioDto
            {
                UrgencyLevel = level.ToString(),
                Count = 0,
                Percentage = 0.0
            }).ToList();
        }

        return assessments
            .GroupBy(t => t.UrgencyLevel)
            .Select(g => new TriageUrgencyRatioDto
            {
                UrgencyLevel = g.Key.ToString(),
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() / total * 100.0, 1)
            })
            .OrderByDescending(r => r.Count)
            .ToList();
    }

    public async Task<IEnumerable<DoctorWorkloadDto>> GetDoctorWorkloadsAsync()
    {
        var doctors = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.Schedules)
            .ToListAsync();

        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u.FullName);
        var specialties = await _context.Specialties.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name);
        var appointments = await _context.Appointments.AsNoTracking().ToListAsync();

        var workloads = doctors.Select(doc =>
        {
            var docAppointments = appointments.Where(a => a.DoctorId == doc.Id).ToList();
            var docName = users.TryGetValue(doc.UserId, out var name) ? name : $"Dr. #{doc.Id}";
            var specialtyName = specialties.TryGetValue(doc.SpecialtyId, out var spec) ? spec : "General Medicine";

            return new DoctorWorkloadDto
            {
                DoctorId = doc.Id,
                DoctorName = docName,
                SpecialtyName = specialtyName,
                TotalAppointments = docAppointments.Count,
                ConfirmedAppointments = docAppointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                ScheduledSlotsCount = doc.Schedules.Count
            };
        }).OrderByDescending(w => w.TotalAppointments).ToList();

        return workloads;
    }

    public async Task<AiSafetyMetricsDto> GetAiSafetyMetricsAsync()
    {
        var totalWorkflows = await _context.AgentWorkflows.CountAsync();
        var safeFailedCount = await _context.AgentWorkflows
            .CountAsync(w => w.Status == WorkflowStatus.SafeFailed);
        var validationFailedCount = await _context.AgentWorkflows
            .CountAsync(w => w.ValidationSummary != null &&
                             w.Status == WorkflowStatus.PausedForApproval);
        var highImpactPaused = await _context.AgentWorkflows
            .CountAsync(w => w.RequiresHumanApproval || w.Status == WorkflowStatus.PausedForApproval);

        var approvals = await _context.AdminApprovals.AsNoTracking().ToListAsync();
        var approvedCount = approvals.Count(a => a.Decision == ApprovalDecision.Approved);
        var rejectedCount = approvals.Count(a => a.Decision == ApprovalDecision.Rejected);
        var revisedCount = approvals.Count(a => a.Decision == ApprovalDecision.Revised);

        var manualOverrides = await _context.AuditLogs
            .CountAsync(a => a.ToolCalled.StartsWith("ManualOverride"));
        var durations = await _context.AuditLogs
            .Where(a => a.DurationMs.HasValue)
            .Select(a => a.DurationMs!.Value)
            .ToListAsync();

        double interventionRate = totalWorkflows > 0
            ? Math.Round((double)(highImpactPaused + manualOverrides) / totalWorkflows * 100.0, 1)
            : 0.0;

        return new AiSafetyMetricsDto
        {
            TotalWorkflows = totalWorkflows,
            HighImpactPausedCount = highImpactPaused,
            ApprovedCount = approvedCount,
            RejectedCount = rejectedCount,
            RevisedCount = revisedCount,
            ManualOverridesCount = manualOverrides,
            SafeFailedCount = safeFailedCount,
            ValidationFailedCount = validationFailedCount,
            AverageLatencyMs = durations.Count == 0 ? 0 : Math.Round(durations.Average(), 1),
            HumanInterventionRate = interventionRate
        };
    }
}
