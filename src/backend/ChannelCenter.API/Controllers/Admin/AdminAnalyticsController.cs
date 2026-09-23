using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Data;
using ChannelCenter.API.Services.Admin;

namespace ChannelCenter.API.Controllers.Admin;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IAdminAnalyticsService _analyticsService;

    public AdminAnalyticsController(IAdminAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // GET: api/admin/analytics/overview
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var overview = await _analyticsService.GetOverviewAsync();
        return Ok(overview);
    }

    // GET: api/admin/analytics/daily-appointments?startDate=2026-09-01&endDate=2026-09-20
    [HttpGet("daily-appointments")]
    public async Task<IActionResult> GetDailyAppointments([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var volumes = await _analyticsService.GetDailyAppointmentVolumesAsync(startDate, endDate);
        return Ok(volumes);
    }

    // GET: api/admin/analytics/triage-ratios
    [HttpGet("triage-ratios")]
    public async Task<IActionResult> GetTriageRatios()
    {
        var ratios = await _analyticsService.GetTriageRatiosAsync();
        return Ok(ratios);
    }

    // GET: api/admin/analytics/doctor-workloads
    [HttpGet("doctor-workloads")]
    public async Task<IActionResult> GetDoctorWorkloads()
    {
        var workloads = await _analyticsService.GetDoctorWorkloadsAsync();
        return Ok(workloads);
    }

    // GET: api/admin/analytics/ai-metrics
    [HttpGet("ai-metrics")]
    public async Task<IActionResult> GetAiMetrics()
    {
        var metrics = await _analyticsService.GetAiSafetyMetricsAsync();
        return Ok(metrics);
    }
}
