using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.Models;
using ChannelCenter.API.DTOs.Admin;

namespace ChannelCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public WorkflowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkflows()
    {
        return Ok(new {message = "Workflows endpoint is working!"});
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllWorkflows()
    {
        var workflows = await _context.AgentWorkflows
            .Include(w => w.AuditLogs)
            .ToListAsync();

        var workflowDtos = workflows.Select(w => new AgentWorkflowResponseDto
        {
            WorkflowId = w.Id,
            Objective = w.Objective,
            Status = w.Status,
            RequiresHumanApproval = w.RequiresHumanApproval,
            CreatedAt = w.CreatedAt,
            AuditLogs = w.AuditLogs.Select(a => new WorkflowAuditResponseDto
            {
                Id = a.Id,
                AgentName = a.AgentName,
                ToolCalled = a.ToolCalled,
                ToolOutput = a.ToolOutput,
                CreatedAt = a.CreatedAt
            }).ToList()
        }).ToList();

        return Ok(workflowDtos);
    }
}

