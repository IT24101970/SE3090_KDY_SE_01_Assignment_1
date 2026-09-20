using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Admin;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;
using ChannelCenter.Tests;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class AdminOverrideControllerTests
{
    [Fact]
    public async Task CancelWorkflow_Success_SetsStatusToTerminated_AndCreatesValidAuditLog()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Triage patient chest pain",
            Status = WorkflowStatus.PausedForApproval,
            RequiresHumanApproval = true
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideCancelRequestDto { Reason = "Emergency override by doctor" };

        // Act
        var result = await controller.CancelWorkflow(workflow.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify workflow updated in DB
        var updatedWorkflow = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.NotNull(updatedWorkflow);
        Assert.Equal(WorkflowStatus.Terminated, updatedWorkflow.Status);

        // Verify audit log created with correct WorkflowId (foreign key bug fix verification)
        var auditLog = context.AuditLogs.FirstOrDefault(a => a.WorkflowId == workflow.Id);
        Assert.NotNull(auditLog);
        Assert.Equal(workflow.Id, auditLog.WorkflowId);
        Assert.Equal("SystemAdmin", auditLog.AgentName);
        Assert.Equal("ManualOverride_Cancel", auditLog.ToolCalled);

        // Verify ToolOutput is valid JSON (PostgreSQL jsonb compatibility verification)
        using var doc = JsonDocument.Parse(auditLog.ToolOutput);
        var root = doc.RootElement;
        Assert.Equal("ManualOverride_Cancel", root.GetProperty("action").GetString());
        Assert.Equal(request.Reason, root.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task CancelWorkflow_ReturnsNotFound_WhenWorkflowDoesNotExist()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AdminOverrideController(context);
        var request = new OverrideCancelRequestDto { Reason = "Test cancel" };

        // Act
        var result = await controller.CancelWorkflow(999, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelWorkflow_ReturnsBadRequest_WhenAlreadyCompleted()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Routine checkup",
            Status = WorkflowStatus.Completed
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideCancelRequestDto { Reason = "Cancel completed" };

        // Act
        var result = await controller.CancelWorkflow(workflow.Id, request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task CancelWorkflow_ReturnsBadRequest_WhenAlreadyTerminated()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Cancelled intake",
            Status = WorkflowStatus.Terminated
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideCancelRequestDto { Reason = "Cancel again" };

        // Act
        var result = await controller.CancelWorkflow(workflow.Id, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReassignWorkflow_Success_SetsStatusToCompleted_AndCreatesValidAuditLog()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Cardiology consult",
            Status = WorkflowStatus.PausedForApproval
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideReassignRequestDto
        {
            TargetDoctorId = 15,
            Reason = "Patient requested specialist transfer"
        };

        // Act
        var result = await controller.ReassignWorkflow(workflow.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var updated = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.NotNull(updated);
        Assert.Equal(WorkflowStatus.Completed, updated.Status);

        // Verify audit log has valid JSON with targetDoctorId
        var auditLog = context.AuditLogs.FirstOrDefault(a => a.WorkflowId == workflow.Id);
        Assert.NotNull(auditLog);
        using var doc = JsonDocument.Parse(auditLog.ToolOutput);
        Assert.Equal(15, doc.RootElement.GetProperty("targetDoctorId").GetInt32());
        Assert.Equal(request.Reason, doc.RootElement.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task ReassignWorkflow_Allowed_WhenTerminated()
    {
        // Arrange: Terminated workflows can be manually reassigned by a system admin
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Terminated workflow needing manual rescue",
            Status = WorkflowStatus.Terminated
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideReassignRequestDto
        {
            TargetDoctorId = 7,
            Reason = "Admin manual override to assign doctor"
        };

        // Act
        var result = await controller.ReassignWorkflow(workflow.Id, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updated = await context.AgentWorkflows.FindAsync(workflow.Id);
        Assert.NotNull(updated);
        Assert.Equal(WorkflowStatus.Completed, updated.Status);
    }

    [Fact]
    public async Task ReassignWorkflow_ReturnsBadRequest_WhenAlreadyCompleted()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Finished appointment",
            Status = WorkflowStatus.Completed
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var controller = new AdminOverrideController(context);
        var request = new OverrideReassignRequestDto
        {
            TargetDoctorId = 5,
            Reason = "Reassign after complete"
        };

        // Act
        var result = await controller.ReassignWorkflow(workflow.Id, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
