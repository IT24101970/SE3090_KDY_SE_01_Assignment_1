using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Admin;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WorkflowAuditResponseDto>> GetAuditLogsAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (filter.WorkflowId.HasValue)
        {
            query = query.Where(a => a.WorkflowId == filter.WorkflowId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AgentName))
        {
            query = query.Where(a => a.AgentName.ToLower().Contains(filter.AgentName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filter.ToolCalled))
        {
            query = query.Where(a => a.ToolCalled.ToLower().Contains(filter.ToolCalled.ToLower()));
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= filter.ToDate.Value);
        }

        int totalCount = await query.CountAsync();
        int page = filter.Page < 1 ? 1 : filter.Page;
        int pageSize = filter.PageSize < 1 ? 20 : Math.Min(filter.PageSize, 100);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new WorkflowAuditResponseDto
            {
                Id = a.Id,
                AgentName = a.AgentName,
                ToolCalled = a.ToolCalled,
                ToolOutput = a.ToolOutput,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<WorkflowAuditResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WorkflowAuditResponseDto> CreateAuditLogAsync(CreateAuditLogDto request)
    {
        var log = new AuditLog
        {
            WorkflowId = request.WorkflowId,
            AgentName = request.AgentName,
            ToolCalled = request.ToolCalled,
            ToolOutput = request.ToolOutput,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        return new WorkflowAuditResponseDto
        {
            Id = log.Id,
            AgentName = log.AgentName,
            ToolCalled = log.ToolCalled,
            ToolOutput = log.ToolOutput,
            CreatedAt = log.CreatedAt
        };
    }
}
