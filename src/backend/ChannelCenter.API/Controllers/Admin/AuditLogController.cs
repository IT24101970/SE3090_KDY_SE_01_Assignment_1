using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Services.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    [ActivatorUtilitiesConstructor]
    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    // Convenience constructor for unit tests
    public AuditLogController(ApplicationDbContext context)
        : this(new AuditLogService(context))
    {
    }
    
    // GET: api/admin/audit-logs?workflowId=1&agentName=Triage&toolCalled=CheckSpecialty&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _auditLogService.GetAuditLogsAsync(filter);
        return Ok(result);
    }

    // POST: api/admin/audit-logs (Internal endpoint for agents to log execution steps)
    [HttpPost]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _auditLogService.CreateAuditLogAsync(request);
        return Ok(result);
    }
}
