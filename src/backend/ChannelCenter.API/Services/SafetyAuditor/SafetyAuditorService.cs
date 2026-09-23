using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.SafetyAuditor;

public class SafetyAuditorService : ISafetyAuditorService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly SafetyAuditorOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public SafetyAuditorService(
        ApplicationDbContext context,
        HttpClient httpClient,
        IOptions<SafetyAuditorOptions> options)
    {
        _context = context;
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SafetyAuditResponseDto> StartAsync(
        int workflowId,
        SafetyAuditStartRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var workflow = await _context.AgentWorkflows
            .FirstOrDefaultAsync(w => w.Id == workflowId, cancellationToken);

        if (workflow == null)
        {
            return FailedResponse(workflowId, request, "workflow_not_found", "Workflow was not found.");
        }

        var hasTerminalRecord = await _context.AuditLogs.AnyAsync(
            a => a.WorkflowId == workflowId &&
                 a.CorrelationId == request.CorrelationId &&
                 (a.ToolCalled == "SafetyAuditor_Result" || a.ToolCalled == "SafetyAuditor_Error"),
            cancellationToken);

        if (workflow.CorrelationId == request.CorrelationId &&
            hasTerminalRecord &&
            workflow.Status is WorkflowStatus.Completed or WorkflowStatus.PausedForApproval or WorkflowStatus.SafeFailed)
        {
            return ToResponse(workflow);
        }

        workflow.CorrelationId = request.CorrelationId;
        workflow.ContractVersion = request.ContractVersion;
        workflow.Status = WorkflowStatus.Running;
        workflow.RequiresHumanApproval = false;
        workflow.ErrorCode = null;
        workflow.ErrorMessage = null;
        workflow.SafetyAuditStartedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                $"/internal/v1/safety-audits/{workflowId}",
                new
                {
                    workflowId,
                    request.Objective,
                    request.Proposal,
                    request.SourceAgent,
                    request.CorrelationId,
                    request.ContractVersion
                },
                _jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await MarkSafeFailedAsync(
                    workflow,
                    "auditor_unavailable",
                    $"Safety Auditor returned HTTP {(int)response.StatusCode}.",
                    cancellationToken);
            }

            if (response.Content.Headers.ContentLength > _options.MaxResponseBytes)
            {
                return await MarkSafeFailedAsync(
                    workflow,
                    "response_too_large",
                    "Safety Auditor response exceeded the configured size limit.",
                    cancellationToken);
            }

            var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (responseBody.Length > _options.MaxResponseBytes)
            {
                return await MarkSafeFailedAsync(
                    workflow,
                    "response_too_large",
                    "Safety Auditor response exceeded the configured size limit.",
                    cancellationToken);
            }

            var result = JsonSerializer.Deserialize<SafetyAuditResponseDto>(
                responseBody,
                _jsonOptions);

            if (result == null || result.WorkflowId != workflowId ||
                !string.Equals(result.CorrelationId, request.CorrelationId, StringComparison.Ordinal))
            {
                return await MarkSafeFailedAsync(
                    workflow,
                    "malformed_response",
                    "Safety Auditor returned an invalid correlation or workflow response.",
                    cancellationToken);
            }

            return await ApplyCallbackAsync(workflowId, result, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return await MarkSafeFailedAsync(
                workflow,
                "auditor_timeout",
                "Safety Auditor timed out.",
                CancellationToken.None);
        }
        catch (HttpRequestException)
        {
            return await MarkSafeFailedAsync(
                workflow,
                "auditor_unavailable",
                "Safety Auditor could not be reached.",
                cancellationToken);
        }
        catch (JsonException)
        {
            return await MarkSafeFailedAsync(
                workflow,
                "malformed_response",
                "Safety Auditor returned malformed JSON.",
                cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return await MarkSafeFailedAsync(
                workflow,
                "malformed_response",
                "Safety Auditor returned an invalid response.",
                cancellationToken);
        }
    }

    public async Task<SafetyAuditResponseDto> ApplyCallbackAsync(
        int workflowId,
        SafetyAuditResponseDto response,
        CancellationToken cancellationToken = default)
    {
        var workflow = await _context.AgentWorkflows
            .FirstOrDefaultAsync(w => w.Id == workflowId, cancellationToken);

        if (workflow == null)
        {
            return FailedResponse(workflowId, response, "workflow_not_found", "Workflow was not found.");
        }

        var hasAppliedResult = await _context.AuditLogs.AnyAsync(
            a => a.WorkflowId == workflowId &&
                 a.CorrelationId == response.CorrelationId &&
                 a.ToolCalled == "SafetyAuditor_Result",
            cancellationToken);

        if (workflow.CorrelationId == response.CorrelationId &&
            hasAppliedResult &&
            workflow.Status is WorkflowStatus.Completed or WorkflowStatus.PausedForApproval or WorkflowStatus.SafeFailed)
        {
            return ToResponse(workflow);
        }

        workflow.CorrelationId = response.CorrelationId;
        workflow.ContractVersion = response.ContractVersion;
        workflow.Status = response.Status;
        workflow.RequiresHumanApproval = response.RequiresApproval;
        workflow.RiskLevel = Limit(response.RiskLevel, 32);
        workflow.PlanSummary = Limit(JsonSerializer.Serialize(response.Plan), 2_000);
        workflow.ValidationSummary = Limit(response.ValidationSummary, 2_000);
        workflow.FinalOutcome = Limit(response.FinalOutcome, 2_000);
        workflow.ErrorCode = Limit(response.Error?.Code, 64);
        workflow.ErrorMessage = Limit(response.Error?.Message, 500);
        workflow.CompletedAt = response.Status == WorkflowStatus.Completed ? DateTime.UtcNow : null;
        workflow.SafeFailedAt = response.Status == WorkflowStatus.SafeFailed ? DateTime.UtcNow : null;
        workflow.UpdatedAt = DateTime.UtcNow;

        var existingAudit = await _context.AuditLogs.AnyAsync(
            a => a.WorkflowId == workflowId &&
                 a.CorrelationId == response.CorrelationId &&
                 a.ToolCalled == "SafetyAuditor_Result",
            cancellationToken);

        if (!existingAudit)
        {
            foreach (var step in response.Steps.Take(4))
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    WorkflowId = workflowId,
                    AgentName = step.Name == "Validate" ? "EvidenceValidator" :
                        step.Name == "RiskAssess" ? "RiskClassifier" : "SafetyAuditor",
                    ToolCalled = "SafetyAuditor_Step",
                    StepName = Limit(step.Name, 64),
                    CorrelationId = response.CorrelationId,
                    ContractVersion = response.ContractVersion,
                    Outcome = Limit(step.Status, 32),
                    DurationMs = step.DurationMs,
                    ToolOutput = JsonSerializer.Serialize(new { summary = Limit(step.Summary, 500) }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            foreach (var tool in response.ToolCalls.Take(8))
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    WorkflowId = workflowId,
                    AgentName = "SafetyAuditor",
                    ToolCalled = Limit(tool.Name, 100) ?? "SafetyAuditor_Tool",
                    StepName = "PauseOrComplete",
                    CorrelationId = response.CorrelationId,
                    ContractVersion = response.ContractVersion,
                    Outcome = Limit(tool.Status, 32),
                    ToolOutput = JsonSerializer.Serialize(new { summary = Limit(tool.Summary, 500) }),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _context.AuditLogs.Add(new AuditLog
            {
                WorkflowId = workflowId,
                AgentName = "SafetyAuditor",
                ToolCalled = "SafetyAuditor_Result",
                StepName = "PauseOrComplete",
                CorrelationId = response.CorrelationId,
                ContractVersion = response.ContractVersion,
                Outcome = response.Status.ToString(),
                ToolOutput = JsonSerializer.Serialize(new
                {
                    response.RiskLevel,
                    response.RequiresApproval,
                    response.Violations,
                    response.FinalOutcome,
                    error = response.Error
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ToResponse(workflow);
    }

    private async Task<SafetyAuditResponseDto> MarkSafeFailedAsync(
        AgentWorkflow workflow,
        string code,
        string message,
        CancellationToken cancellationToken)
    {
        workflow.Status = WorkflowStatus.SafeFailed;
        workflow.RequiresHumanApproval = false;
        workflow.ErrorCode = code;
        workflow.ErrorMessage = message;
        workflow.FinalOutcome = "No proposed action was executed.";
        workflow.SafeFailedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;
        if (!await _context.AuditLogs.AnyAsync(
                a => a.WorkflowId == workflow.Id &&
                     a.CorrelationId == workflow.CorrelationId &&
                     a.ToolCalled == "SafetyAuditor_Error",
                cancellationToken))
        {
            _context.AuditLogs.Add(new AuditLog
            {
                WorkflowId = workflow.Id,
                AgentName = "SafetyAuditor",
                ToolCalled = "SafetyAuditor_Error",
                StepName = "PauseOrComplete",
                CorrelationId = workflow.CorrelationId,
                ContractVersion = workflow.ContractVersion,
                Outcome = "SafeFailed",
                ToolOutput = JsonSerializer.Serialize(new { code, message }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync(cancellationToken);
        return ToResponse(workflow);
    }

    private static SafetyAuditResponseDto FailedResponse(
        int workflowId,
        SafetyAuditStartRequestDto request,
        string code,
        string message) =>
        new()
        {
            WorkflowId = workflowId,
            CorrelationId = request.CorrelationId,
            ContractVersion = request.ContractVersion,
            Status = WorkflowStatus.SafeFailed,
            FinalOutcome = "No proposed action was executed.",
            Error = new SafetyAuditErrorDto { Code = code, Message = message }
        };

    private static SafetyAuditResponseDto FailedResponse(
        int workflowId,
        SafetyAuditResponseDto response,
        string code,
        string message) =>
        new()
        {
            WorkflowId = workflowId,
            CorrelationId = response.CorrelationId,
            ContractVersion = response.ContractVersion,
            Status = WorkflowStatus.SafeFailed,
            FinalOutcome = "No proposed action was executed.",
            Error = new SafetyAuditErrorDto { Code = code, Message = message }
        };

    private static SafetyAuditResponseDto ToResponse(AgentWorkflow workflow) =>
        new()
        {
            WorkflowId = workflow.Id,
            CorrelationId = workflow.CorrelationId,
            ContractVersion = workflow.ContractVersion,
            Status = workflow.Status,
            RiskLevel = workflow.RiskLevel ?? "Unknown",
            RequiresApproval = workflow.RequiresHumanApproval,
            ValidationSummary = workflow.ValidationSummary,
            FinalOutcome = workflow.FinalOutcome,
            Error = workflow.ErrorCode == null
                ? null
                : new SafetyAuditErrorDto
                {
                    Code = workflow.ErrorCode,
                    Message = workflow.ErrorMessage ?? string.Empty
                },
            CompletedAt = workflow.CompletedAt
        };

    private static string? Limit(string? value, int length) =>
        value == null ? null : value.Length <= length ? value : value[..length];
}
