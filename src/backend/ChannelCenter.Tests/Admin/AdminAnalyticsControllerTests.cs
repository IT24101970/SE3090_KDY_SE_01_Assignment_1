using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Admin;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.Tests;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class AdminAnalyticsControllerTests
{
    // ── Overview ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOverview_ReturnsCorrectCounts()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        context.AgentWorkflows.AddRange(
            new AgentWorkflow { Objective = "Running wf",   Status = WorkflowStatus.Running },
            new AgentWorkflow { Objective = "Paused wf",    Status = WorkflowStatus.PausedForApproval, RequiresHumanApproval = true },
            new AgentWorkflow { Objective = "Completed wf", Status = WorkflowStatus.Completed }
        );

        context.TriageAssessments.Add(new TriageAssessment
        {
            AppointmentId        = 1,
            RawSymptoms          = "chest pain",
            UrgencyScore         = 95,
            UrgencyLevel         = UrgencyLevel.Emergency,
            RecommendedSpecialty = "Cardiology"
        });

        await context.SaveChangesAsync();

        var controller = new AdminAnalyticsController(context);

        // Act
        var result = await controller.GetOverview();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AnalyticsOverviewDto>(ok.Value);
        Assert.Equal(1, dto.ActiveWorkflowsCount);
        Assert.Equal(1, dto.PausedWorkflowsCount);
        Assert.Equal(1, dto.CompletedWorkflowsCount);
        Assert.Equal(1, dto.EmergencyCasesCount);
    }

    [Fact]
    public async Task GetOverview_WithNoData_ReturnsZeroCounts()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AdminAnalyticsController(context);

        var result = await controller.GetOverview();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AnalyticsOverviewDto>(ok.Value);
        Assert.Equal(0, dto.ActiveWorkflowsCount);
        Assert.Equal(0, dto.PausedWorkflowsCount);
        Assert.Equal(0, dto.EmergencyCasesCount);
        Assert.Equal(100.0, dto.SystemApprovalRate); // 100% when no decisions
    }

    // ── Daily Appointments ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDailyAppointments_GroupsByDate_AndCountsCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var today     = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        context.Appointments.AddRange(
            new Appointment { PatientId = 1, DoctorId = 1, ScheduleId = 1, AppointmentDate = today,     Status = AppointmentStatus.Confirmed, ReasonForVisit = "fever" },
            new Appointment { PatientId = 1, DoctorId = 1, ScheduleId = 1, AppointmentDate = today,     Status = AppointmentStatus.Cancelled, ReasonForVisit = "flu"   },
            new Appointment { PatientId = 2, DoctorId = 1, ScheduleId = 1, AppointmentDate = yesterday, Status = AppointmentStatus.Pending,   ReasonForVisit = "rash"  }
        );
        await context.SaveChangesAsync();

        var controller = new AdminAnalyticsController(context);
        var result = await controller.GetDailyAppointments(yesterday, today);

        var ok   = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<DailyAppointmentVolumeDto>>(ok.Value).ToList();

        Assert.Equal(2, list.Count); // two distinct dates

        var todayData = list.First(x => x.Date == today.ToString("yyyy-MM-dd"));
        Assert.Equal(2, todayData.TotalAppointments);
        Assert.Equal(1, todayData.ConfirmedCount);
        Assert.Equal(1, todayData.CancelledCount);
    }

    // ── Triage Ratios ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTriageRatios_CalculatesPercentagesCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        context.TriageAssessments.AddRange(
            new TriageAssessment { AppointmentId = 1, RawSymptoms = "s1", UrgencyScore = 90, UrgencyLevel = UrgencyLevel.Emergency, RecommendedSpecialty = "Cardiology" },
            new TriageAssessment { AppointmentId = 2, RawSymptoms = "s2", UrgencyScore = 90, UrgencyLevel = UrgencyLevel.Emergency, RecommendedSpecialty = "Cardiology" },
            new TriageAssessment { AppointmentId = 3, RawSymptoms = "s3", UrgencyScore = 40, UrgencyLevel = UrgencyLevel.Low,       RecommendedSpecialty = "General Medicine" },
            new TriageAssessment { AppointmentId = 4, RawSymptoms = "s4", UrgencyScore = 40, UrgencyLevel = UrgencyLevel.Low,       RecommendedSpecialty = "General Medicine" }
        );
        await context.SaveChangesAsync();

        var controller = new AdminAnalyticsController(context);
        var result = await controller.GetTriageRatios();

        var ok   = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<TriageUrgencyRatioDto>>(ok.Value).ToList();

        Assert.Equal(2, list.Count);
        var emergency = list.First(x => x.UrgencyLevel == "Emergency");
        Assert.Equal(50.0, emergency.Percentage);
    }

    [Fact]
    public async Task GetTriageRatios_WithNoData_ReturnsAllLevelsAtZero()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AdminAnalyticsController(context);

        var result = await controller.GetTriageRatios();

        var ok   = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<TriageUrgencyRatioDto>>(ok.Value).ToList();

        Assert.Equal(4, list.Count); // Low, Medium, High, Emergency
        Assert.All(list, dto => Assert.Equal(0, dto.Count));
    }

    // ── AI Safety Metrics ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAiMetrics_CalculatesApprovalAndRejectionCounts()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user = new User { Id = 1, FullName = "Admin", Email = "a@b.com", Role = UserRole.Admin };
        context.Users.Add(user);

        var wf1 = new AgentWorkflow { Objective = "WF1", Status = WorkflowStatus.Completed, RequiresHumanApproval = true };
        var wf2 = new AgentWorkflow { Objective = "WF2", Status = WorkflowStatus.Terminated };
        context.AgentWorkflows.AddRange(wf1, wf2);
        await context.SaveChangesAsync();

        context.AdminApprovals.AddRange(
            new AdminApproval { WorkflowId = wf1.Id, AdminUserId = 1, Decision = ApprovalDecision.Approved },
            new AdminApproval { WorkflowId = wf2.Id, AdminUserId = 1, Decision = ApprovalDecision.Rejected }
        );
        await context.SaveChangesAsync();

        var controller = new AdminAnalyticsController(context);
        var result = await controller.GetAiMetrics();

        var ok  = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AiSafetyMetricsDto>(ok.Value);
        Assert.Equal(2, dto.TotalWorkflows);
        Assert.Equal(1, dto.ApprovedCount);
        Assert.Equal(1, dto.RejectedCount);
    }
}
