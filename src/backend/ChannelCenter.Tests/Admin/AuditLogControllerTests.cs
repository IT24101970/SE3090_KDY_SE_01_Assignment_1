using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Admin;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.Tests;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class AuditLogControllerTests
{
    // ── GetAuditLogs – Filtering ─────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_NoFilter_ReturnsAllLogs_Paginated()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "WF", Status = WorkflowStatus.Running };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        context.AuditLogs.AddRange(
            new AuditLog { WorkflowId = wf.Id, AgentName = "IntakeAgent",  ToolCalled = "ParseSymptoms", ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf.Id, AgentName = "TriageAgent",  ToolCalled = "AssignUrgency", ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf.Id, AgentName = "SafetyAuditor",ToolCalled = "PauseAction",   ToolOutput = "{}" }
        );
        await context.SaveChangesAsync();

        var controller = new AuditLogController(context);
        var result = await controller.GetAuditLogs(new AuditLogFilterDto { Page = 1, PageSize = 10 });

        var ok  = Assert.IsType<OkObjectResult>(result);
        var paged = Assert.IsType<PagedResult<WorkflowAuditResponseDto>>(ok.Value);
        Assert.Equal(3, paged.TotalCount);
        Assert.Equal(3, paged.Items.Count());
    }

    [Fact]
    public async Task GetAuditLogs_FilterByAgentName_ReturnsOnlyMatchingLogs()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "WF", Status = WorkflowStatus.Running };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        context.AuditLogs.AddRange(
            new AuditLog { WorkflowId = wf.Id, AgentName = "IntakeAgent",   ToolCalled = "Parse",  ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf.Id, AgentName = "TriageAgent",   ToolCalled = "Assign", ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf.Id, AgentName = "SafetyAuditor", ToolCalled = "Pause",  ToolOutput = "{}" }
        );
        await context.SaveChangesAsync();

        var controller = new AuditLogController(context);
        var filter = new AuditLogFilterDto { AgentName = "Triage", Page = 1, PageSize = 10 };
        var result = await controller.GetAuditLogs(filter);

        var ok    = Assert.IsType<OkObjectResult>(result);
        var paged = Assert.IsType<PagedResult<WorkflowAuditResponseDto>>(ok.Value);
        Assert.Equal(1, paged.TotalCount);
        Assert.Equal("TriageAgent", paged.Items.First().AgentName);
    }

    [Fact]
    public async Task GetAuditLogs_FilterByWorkflowId_ReturnsOnlyLogsForThatWorkflow()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf1 = new AgentWorkflow { Objective = "WF1", Status = WorkflowStatus.Running };
        var wf2 = new AgentWorkflow { Objective = "WF2", Status = WorkflowStatus.Running };
        context.AgentWorkflows.AddRange(wf1, wf2);
        await context.SaveChangesAsync();

        context.AuditLogs.AddRange(
            new AuditLog { WorkflowId = wf1.Id, AgentName = "IntakeAgent", ToolCalled = "Parse",  ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf1.Id, AgentName = "TriageAgent", ToolCalled = "Assign", ToolOutput = "{}" },
            new AuditLog { WorkflowId = wf2.Id, AgentName = "IntakeAgent", ToolCalled = "Parse",  ToolOutput = "{}" }
        );
        await context.SaveChangesAsync();

        var controller = new AuditLogController(context);
        var filter = new AuditLogFilterDto { WorkflowId = wf1.Id, Page = 1, PageSize = 10 };
        var result = await controller.GetAuditLogs(filter);

        var ok    = Assert.IsType<OkObjectResult>(result);
        var paged = Assert.IsType<PagedResult<WorkflowAuditResponseDto>>(ok.Value);
        Assert.Equal(2, paged.TotalCount);
    }

    [Fact]
    public async Task GetAuditLogs_Pagination_ReturnsCorrectPage()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "WF", Status = WorkflowStatus.Running };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        // Add 5 logs
        for (int i = 1; i <= 5; i++)
        {
            context.AuditLogs.Add(new AuditLog
            {
                WorkflowId = wf.Id,
                AgentName  = $"Agent{i}",
                ToolCalled = "Tool",
                ToolOutput = "{}",
                CreatedAt  = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var controller = new AuditLogController(context);

        // Page 1 with page size 3 → should return 3 items
        var result1 = await controller.GetAuditLogs(new AuditLogFilterDto { Page = 1, PageSize = 3 });
        var ok1     = Assert.IsType<OkObjectResult>(result1);
        var paged1  = Assert.IsType<PagedResult<WorkflowAuditResponseDto>>(ok1.Value);
        Assert.Equal(5,  paged1.TotalCount);
        Assert.Equal(3,  paged1.Items.Count());
        Assert.Equal(2,  paged1.TotalPages);

        // Page 2 with page size 3 → should return 2 items
        var result2 = await controller.GetAuditLogs(new AuditLogFilterDto { Page = 2, PageSize = 3 });
        var ok2     = Assert.IsType<OkObjectResult>(result2);
        var paged2  = Assert.IsType<PagedResult<WorkflowAuditResponseDto>>(ok2.Value);
        Assert.Equal(2, paged2.Items.Count());
    }

    // ── CreateAuditLog ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAuditLog_ValidRequest_ReturnsCreatedLog()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "WF", Status = WorkflowStatus.Running };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        var controller = new AuditLogController(context);
        var request = new CreateAuditLogDto
        {
            WorkflowId = wf.Id,
            AgentName  = "SafetyAuditor",
            ToolCalled = "VerifySpecialtyMatch",
            ToolOutput = "{\"match\":true,\"doctorId\":3,\"specialtyId\":1}"
        };

        var result = await controller.CreateAuditLog(request);

        var ok  = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<WorkflowAuditResponseDto>(ok.Value);
        Assert.Equal("SafetyAuditor", dto.AgentName);
        Assert.Equal("VerifySpecialtyMatch", dto.ToolCalled);
        Assert.True(context.AuditLogs.Any(a => a.WorkflowId == wf.Id && a.AgentName == "SafetyAuditor"));
    }
}
