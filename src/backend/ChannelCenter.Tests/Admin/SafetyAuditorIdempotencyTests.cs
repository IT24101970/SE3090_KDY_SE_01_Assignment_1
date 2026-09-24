using Microsoft.Extensions.Options;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.SafetyAuditor;
using Xunit;

namespace ChannelCenter.Tests.Admin;

public class SafetyAuditorIdempotencyTests
{
    [Fact]
    public async Task ApplyCallback_DuplicateCorrelation_WritesOnlyOneResult()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var workflow = new AgentWorkflow
        {
            Objective = "Appointment safety review",
            Status = WorkflowStatus.Running
        };
        context.AgentWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        var service = new SafetyAuditorService(
            context,
            new HttpClient(),
            Options.Create(new SafetyAuditorOptions()));
        var callback = new SafetyAuditResponseDto
        {
            WorkflowId = workflow.Id,
            CorrelationId = "same-correlation",
            Status = WorkflowStatus.Completed,
            RiskLevel = "Low",
            RequiresApproval = false,
            FinalOutcome = "Safe"
        };

        await service.ApplyCallbackAsync(workflow.Id, callback);
        await service.ApplyCallbackAsync(workflow.Id, callback);

        Assert.Equal(
            1,
            context.AuditLogs.Count(log =>
                log.WorkflowId == workflow.Id &&
                log.ToolCalled == "SafetyAuditor_Result"));
    }
}
