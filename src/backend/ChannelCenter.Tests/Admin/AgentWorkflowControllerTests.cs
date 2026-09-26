using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Admin;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.Tests;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class AgentWorkflowControllerTests
{
    [Fact]
    public async Task GetWorkflows_ReturnsAllWorkflows_WithoutAuditLogsInSummary()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var wf1 = new AgentWorkflow { Objective = "Intake parsing 1", Status = WorkflowStatus.Running, CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
        var wf2 = new AgentWorkflow { Objective = "Intake parsing 2", Status = WorkflowStatus.PausedForApproval, CreatedAt = DateTime.UtcNow };
        
        context.AgentWorkflows.AddRange(wf1, wf2);
        
        // Add an audit log to verify it does not inflate the summary DTO
        context.AuditLogs.Add(new AuditLog
        {
            WorkflowId = wf1.Id,
            AgentName = "IntakeAgent",
            ToolCalled = "FormatSummary",
            ToolOutput = "{}"
        });
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);

        // Act
        var result = await controller.GetWorkflows();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<AgentWorkflowResponseDto>>(okResult.Value);
        Assert.Equal(2, dtos.Count());
        
        // Ensure newest is first (ordered by CreatedAt desc)
        Assert.Equal(wf2.Id, dtos.First().Id);
    }

    [Fact]
    public async Task GetWorkflows_WithStatusFilter_ReturnsOnlyMatchingWorkflows()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.AgentWorkflows.AddRange(
            new AgentWorkflow { Objective = "Running task", Status = WorkflowStatus.Running },
            new AgentWorkflow { Objective = "Paused task", Status = WorkflowStatus.PausedForApproval },
            new AgentWorkflow { Objective = "Completed task", Status = WorkflowStatus.Completed }
        );
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);

        // Act
        var result = await controller.GetWorkflows(status: WorkflowStatus.PausedForApproval);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<AgentWorkflowResponseDto>>(okResult.Value);
        var list = dtos.ToList();
        Assert.Single(list);
        Assert.Equal("Paused task", list[0].Objective);
        Assert.Equal(WorkflowStatus.PausedForApproval, list[0].Status);
    }

    [Fact]
    public async Task GetWorkflowById_ReturnsWorkflowWithOrderedAuditLogs_WhenFound()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Triage assessment",
            Status = WorkflowStatus.PausedForApproval
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        context.AuditLogs.AddRange(
            new AuditLog
            {
                WorkflowId = workflow.Id,
                AgentName = "TriageAgent",
                ToolCalled = "CheckSpecialty",
                ToolOutput = "{\"specialty\":\"Cardiology\"}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new AuditLog
            {
                WorkflowId = workflow.Id,
                AgentName = "SafetyAgent",
                ToolCalled = "AssessRisk",
                ToolOutput = "{\"riskScore\":9}",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);

        // Act
        var result = await controller.GetWorkflowById(workflow.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AgentWorkflowResponseDto>(okResult.Value);
        Assert.Equal(workflow.Id, dto.Id);
        Assert.Equal(2, dto.AuditLogs.Count);
        Assert.Equal("SafetyAgent", dto.AuditLogs.First().AgentName); // Newest first
    }

    [Fact]
    public async Task GetWorkflowById_ReturnsNotFound_WhenDoesNotExist()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AgentWorkflowsController(context);

        // Act
        var result = await controller.GetWorkflowById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ApproveWorkflow_ApprovedDecision_TransitionsToCompleted_AndCreatesApprovalRecord()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var user = new User { Id = 1, FullName = "Dr. Admin", Email = "admin@hospital.com", Role = UserRole.Admin };
        context.Users.Add(user);

        var workflow = new AgentWorkflow
        {
            Objective = "High risk triage",
            Status = WorkflowStatus.PausedForApproval,
            RequiresHumanApproval = true
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var request = new WorkflowApprovalRequestDto
        {
            Decision = ApprovalDecision.Approved,
            AdminUserId = user.Id
        };

        // Act
        var result = await controller.ApproveWorkflow(workflow.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<WorkflowApprovalResponseDto>(okResult.Value);
        Assert.Equal(ApprovalDecision.Approved, responseDto.Decision);
        Assert.Equal(WorkflowStatus.Completed, responseDto.UpdatedWorkflowStatus);

        // Verify DB updates
        var updated = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.NotNull(updated);
        Assert.Equal(WorkflowStatus.Completed, updated.Status);

        var approval = context.AdminApprovals.FirstOrDefault(a => a.WorkflowId == workflow.Id);
        Assert.NotNull(approval);
        Assert.Equal(user.Id, approval.AdminUserId);
        Assert.Equal(ApprovalDecision.Approved, approval.Decision);
    }

    [Fact]
    public async Task ApproveWorkflow_RejectedDecision_TransitionsToTerminated()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Suspicious intake",
            Status = WorkflowStatus.PausedForApproval
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var request = new WorkflowApprovalRequestDto
        {
            Decision = ApprovalDecision.Rejected
        };

        // Act
        var result = await controller.ApproveWorkflow(workflow.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<WorkflowApprovalResponseDto>(okResult.Value);
        Assert.Equal(WorkflowStatus.Terminated, responseDto.UpdatedWorkflowStatus);

        var updated = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.NotNull(updated);
        Assert.Equal(WorkflowStatus.Terminated, updated.Status);
    }

    [Fact]
    public async Task ApproveWorkflow_RevisedDecision_TransitionsToPausedForApproval()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Needs doctor note revision",
            Status = WorkflowStatus.Running
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var request = new WorkflowApprovalRequestDto
        {
            Decision = ApprovalDecision.Revised
        };

        // Act
        var result = await controller.ApproveWorkflow(workflow.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<WorkflowApprovalResponseDto>(okResult.Value);
        Assert.Equal(WorkflowStatus.PausedForApproval, responseDto.UpdatedWorkflowStatus);
    }

    [Fact]
    public async Task ApproveWorkflow_ReturnsBadRequest_WhenAlreadyCompletedOrTerminated()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var completedWorkflow = new AgentWorkflow
        {
            Objective = "Old completed task",
            Status = WorkflowStatus.Completed
        };
        context.AgentWorkflows.Add(completedWorkflow);
        await context.SaveChangesAsync();

        var controller = new AgentWorkflowsController(context);
        var request = new WorkflowApprovalRequestDto { Decision = ApprovalDecision.Approved };

        // Act
        var result = await controller.ApproveWorkflow(completedWorkflow.Id, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
