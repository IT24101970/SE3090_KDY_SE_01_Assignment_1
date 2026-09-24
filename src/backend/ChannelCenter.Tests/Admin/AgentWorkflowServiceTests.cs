using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Admin;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.Tests;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class AgentWorkflowServiceTests
{
    // ── CreateWorkflow ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateWorkflow_ReturnsNewWorkflow_WithRunningStatus()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AgentWorkflowsController(context);

        var request = new CreateWorkflowRequestDto
        {
            Objective             = "Intake for patient #77 with fever",
            RequiresHumanApproval = false
        };

        var result = await controller.CreateWorkflow(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var dto     = Assert.IsType<AgentWorkflowResponseDto>(created.Value);
        Assert.Equal("Intake for patient #77 with fever", dto.Objective);
        Assert.Equal(WorkflowStatus.Running, dto.Status);
        Assert.True(dto.Id > 0);

        // Confirm persisted
        var saved = await context.AgentWorkflows.FindAsync(dto.Id);
        Assert.NotNull(saved);
        Assert.Equal(WorkflowStatus.Running, saved.Status);
    }

    [Fact]
    public async Task CreateWorkflow_WithHumanApprovalFlag_PersistsFlag()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AgentWorkflowsController(context);

        var request = new CreateWorkflowRequestDto
        {
            Objective             = "High-risk cardiology intake",
            RequiresHumanApproval = true
        };

        var result = await controller.CreateWorkflow(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var dto     = Assert.IsType<AgentWorkflowResponseDto>(created.Value);
        Assert.True(dto.RequiresHumanApproval);
    }

    // ── PauseWorkflow (Safety Auditor trigger) ────────────────────────────────

    [Fact]
    public async Task PauseWorkflow_SetsStatusToPausedForApproval_AndLogsAuditEntry()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "Emergency triage", Status = WorkflowStatus.Running };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var request = new PauseWorkflowRequestDto
        {
            Reason              = "Emergency urgency level detected",
            AgentName           = "SafetyAuditor",
            ValidationViolation = "UrgencyLevel=Emergency triggers mandatory pause"
        };

        var result = await controller.PauseWorkflow(wf.Id, request);

        Assert.IsType<OkObjectResult>(result);

        var updated = await context.AgentWorkflows.FindAsync(wf.Id);
        Assert.NotNull(updated);
        Assert.Equal(WorkflowStatus.PausedForApproval, updated.Status);
        Assert.True(updated.RequiresHumanApproval);

        var auditLog = context.AuditLogs.FirstOrDefault(a => a.WorkflowId == wf.Id);
        Assert.NotNull(auditLog);
        Assert.Equal("SafetyAuditor", auditLog.AgentName);
        Assert.Equal("SafetyAuditor_PauseAction", auditLog.ToolCalled);
        Assert.Contains("Emergency urgency level detected", auditLog.ToolOutput);
    }

    [Fact]
    public async Task PauseWorkflow_ReturnsNotFound_WhenWorkflowDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AgentWorkflowsController(context);

        var result = await controller.PauseWorkflow(999, new PauseWorkflowRequestDto
        {
            Reason    = "Test",
            AgentName = "SafetyAuditor"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PauseWorkflow_ReturnsBadRequest_WhenWorkflowAlreadyCompleted()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var wf = new AgentWorkflow { Objective = "Done", Status = WorkflowStatus.Completed };
        context.AgentWorkflows.Add(wf);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var result = await controller.PauseWorkflow(wf.Id, new PauseWorkflowRequestDto
        {
            Reason    = "Too late",
            AgentName = "SafetyAuditor"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ── Full Agentic Lifecycle ────────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_Create_Pause_Approve_TransitionsCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var adminUser = new User { Id = 1, FullName = "Admin", Email = "a@b.com", Role = UserRole.Admin };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);

        // Step 1: Start the workflow (simulates Intake Agent beginning)
        var createResult = await controller.CreateWorkflow(new CreateWorkflowRequestDto
        {
            Objective = "Patient intake – possible cardiac event"
        });
        var dto = Assert.IsType<AgentWorkflowResponseDto>(((CreatedAtActionResult)createResult).Value!);
        Assert.Equal(WorkflowStatus.Running, dto.Status);

        // Step 2: Safety Auditor pauses it
        var pauseResult = await controller.PauseWorkflow(dto.Id, new PauseWorkflowRequestDto
        {
            Reason    = "Emergency urgency – manual review required",
            AgentName = "SafetyAuditor"
        });
        Assert.IsType<OkObjectResult>(pauseResult);

        var paused = await context.AgentWorkflows.FindAsync(dto.Id);
        Assert.Equal(WorkflowStatus.PausedForApproval, paused!.Status);

        // Step 3: Admin approves
        var approveResult = await controller.ApproveWorkflow(dto.Id, new WorkflowApprovalRequestDto
        {
            Decision    = ApprovalDecision.Approved,
            AdminUserId = 1
        });
        Assert.IsType<OkObjectResult>(approveResult);

        var approved = await context.AgentWorkflows.FindAsync(dto.Id);
        Assert.Equal(WorkflowStatus.Running, approved!.Status);

        // Verify full audit trail exists
        var logs = context.AuditLogs.Where(a => a.WorkflowId == dto.Id).ToList();
        Assert.Single(logs); // Only the pause log (approve doesn't add an audit log)
        Assert.Equal("SafetyAuditor", logs[0].AgentName);

        var approval = context.AdminApprovals.FirstOrDefault(a => a.WorkflowId == dto.Id);
        Assert.NotNull(approval);
        Assert.Equal(ApprovalDecision.Approved, approval.Decision);
    }

    [Fact]
    public async Task FullLifecycle_Create_Pause_Reject_TerminatesWorkflow()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var adminUser = new User { Id = 1, FullName = "Admin", Email = "a@b.com", Role = UserRole.Admin };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);

        var createResult = await controller.CreateWorkflow(new CreateWorkflowRequestDto
        {
            Objective = "Suspicious symptom cluster intake"
        });
        var dto = Assert.IsType<AgentWorkflowResponseDto>(((CreatedAtActionResult)createResult).Value!);

        await controller.PauseWorkflow(dto.Id, new PauseWorkflowRequestDto
        {
            Reason              = "Specialty mismatch detected",
            AgentName           = "SafetyAuditor",
            ValidationViolation = "VerifySpecialtyMatch failed"
        });

        var rejectResult = await controller.ApproveWorkflow(dto.Id, new WorkflowApprovalRequestDto
        {
            Decision    = ApprovalDecision.Rejected,
            AdminUserId = 1
        });
        Assert.IsType<OkObjectResult>(rejectResult);

        var terminated = await context.AgentWorkflows.FindAsync(dto.Id);
        Assert.Equal(WorkflowStatus.Terminated, terminated!.Status);
    }

    [Fact]
    public async Task RestartWorkflow_RestartsAnyPausedWorkflow_AndRecordsAdminAction()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Paused for a schedule mismatch",
            Status = WorkflowStatus.PausedForApproval,
            RequiresHumanApproval = true,
            CorrelationId = "original-correlation"
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var result = await controller.RestartWorkflow(
            workflow.Id,
            new RestartWorkflowRequestDto { Reason = "Schedule corrected by admin" });

        var response = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AgentWorkflowResponseDto>(response.Value);
        Assert.Equal(WorkflowStatus.Running, dto.Status);
        Assert.NotEqual("original-correlation", dto.CorrelationId);

        var updated = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.False(updated!.RequiresHumanApproval);
        Assert.Contains(
            context.AuditLogs,
            log => log.WorkflowId == workflow.Id &&
                   log.ToolCalled == "Admin_RestartWorkflow");
    }
}
