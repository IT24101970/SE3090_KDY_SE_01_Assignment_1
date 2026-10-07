using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Admin;
using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.IntakeAgent;
using ChannelCenter.API.Services.SafetyAuditor;

namespace ChannelCenter.API.Services.Appointment;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IIntakeAgentService? _intakeAgentService;
    private readonly ISafetyAuditorService? _safetyAuditorService;

    public AppointmentService(
        ApplicationDbContext context,
        IIntakeAgentService? intakeAgentService = null,
        ISafetyAuditorService? safetyAuditorService = null)
    {
        _context = context;
        _intakeAgentService = intakeAgentService;
        _safetyAuditorService = safetyAuditorService;
    }

    public AppointmentService(
        ApplicationDbContext context,
        ISafetyAuditorService? safetyAuditorService)
        : this(context, null, safetyAuditorService)
    {
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        Models.Patient? patient = null;
        if (dto.PatientId.HasValue && dto.PatientId.Value > 0)
        {
            patient = await _context.Patients.FindAsync(dto.PatientId.Value);
        }

        if (patient == null)
        {
            // Find or create a dedicated Walk-in Patient profile so unregistered appointments don't hijack registered Patient #1
            patient = await _context.Patients.FirstOrDefaultAsync(p => p.NIC == "WALKIN-PATIENT" || p.Name == "Walk-in Patient");

            if (patient == null)
            {
                patient = new Models.Patient
                {
                    Name = "Walk-in Patient",
                    NIC = "WALKIN-PATIENT",
                    PhoneNumber = "N/A",
                    Email = "walkin@hospital.local",
                    Gender = "Unspecified",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
        }
        dto.PatientId = patient.Id;

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == dto.DoctorId)
            ?? await _context.Doctors.Include(d => d.User).Include(d => d.Specialty).FirstOrDefaultAsync();

        if (doctor == null)
        {
            return (false, "No doctors registered in the database. Please register a doctor first.", null);
        }
        dto.DoctorId = doctor.Id;

        var schedule = await _context.DoctorSchedules
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId)
            ?? await _context.DoctorSchedules.Include(s => s.Room).FirstOrDefaultAsync(s => s.DoctorId == doctor.Id)
            ?? await _context.DoctorSchedules.Include(s => s.Room).FirstOrDefaultAsync();

        if (schedule == null)
        {
            return (false, "No doctor schedules found in database. Please create at least one doctor schedule first.", null);
        }
        dto.ScheduleId = schedule.Id;

        var nowUtc = DateTime.UtcNow;
        var apptDate = dto.AppointmentDate != default
            ? DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc)
            : (schedule.StartTime >= nowUtc ? schedule.StartTime : nowUtc.AddHours(1));

        // Check channel slot capacity against MaxPatients
        var activeAppointmentsCount = await _context.Appointments.CountAsync(a =>
            a.ScheduleId == dto.ScheduleId &&
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        if (schedule.MaxPatients > 0 && activeAppointmentsCount >= schedule.MaxPatients)
        {
            return (false, $"Channel session is fully booked (Maximum {schedule.MaxPatients} patients reached).", null);
        }

        var appointment = new Models.Appointment
        {
            PatientId = patient.Id,
            DoctorId = dto.DoctorId,
            ScheduleId = dto.ScheduleId,
            AppointmentDate = apptDate,
            Status = AppointmentStatus.Pending,
            ReasonForVisit = (dto.ReasonForVisit ?? string.Empty).Trim(),
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // ── Skip AI Workflows for Direct Manual Admin Assignments ─────────────────
        if (!dto.SkipAiWorkflows && _intakeAgentService != null && !string.IsNullOrWhiteSpace(appointment.ReasonForVisit))
        {
            try
            {
                var intakeResult = await _intakeAgentService.ProcessIntakeAsync(new IntakeProcessRequestDto
                {
                    PatientId = appointment.PatientId,
                    RawText = appointment.ReasonForVisit
                });

                if (intakeResult.Success && intakeResult.Data != null)
                {
                    var extractedKeywords = intakeResult.Data.Symptoms
                        .Select(s => s.Keyword)
                        .Where(k => !string.IsNullOrWhiteSpace(k) && !k.StartsWith("General discomfort", StringComparison.OrdinalIgnoreCase))
                        .Distinct()
                        .ToList();

                    var rawSymptoms = extractedKeywords.Count > 0
                        ? string.Join("; ", extractedKeywords)
                        : (intakeResult.Data.Symptoms.FirstOrDefault()?.Keyword ?? "General evaluation / Unspecified symptoms");

                    var existingTriage = await _context.TriageAssessments
                        .FirstOrDefaultAsync(t => t.AppointmentId == appointment.Id);

                    if (existingTriage != null)
                    {
                        existingTriage.RawSymptoms = rawSymptoms;
                        existingTriage.UrgencyLevel = intakeResult.Data.SeverityFlags.UrgencyLevel;
                        existingTriage.UrgencyScore = (int)intakeResult.Data.SeverityFlags.UrgencyLevel * 25;
                        existingTriage.RecommendedSpecialty = intakeResult.Data.DoctorPreferences?.PreferredSpecialty ?? "General Practice";
                        existingTriage.ReasoningTrace = $"Intake Agent normalized symptoms: {rawSymptoms}";
                        existingTriage.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var triage = new TriageAssessment
                        {
                            AppointmentId = appointment.Id,
                            RawSymptoms = rawSymptoms,
                            UrgencyScore = (int)intakeResult.Data.SeverityFlags.UrgencyLevel * 25,
                            UrgencyLevel = intakeResult.Data.SeverityFlags.UrgencyLevel,
                            RecommendedSpecialty = intakeResult.Data.DoctorPreferences?.PreferredSpecialty ?? "General Practice",
                            ReasoningTrace = $"Intake Agent normalized symptoms: {rawSymptoms}",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.TriageAssessments.Add(triage);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Safe failure
            }
        }

        if (!dto.SkipAiWorkflows)
        {
            await StartSafetyAuditAfterSaveAsync(appointment);
        }

        var response = await GetAppointmentByIdAsync(appointment.Id);
        return (true, null, response);
    }

    private async Task StartSafetyAuditAfterSaveAsync(Models.Appointment appointment)
    {
        if (_safetyAuditorService == null)
        {
            return;
        }

        AgentWorkflow? workflow = null;
        try
        {
            workflow = await _context.AgentWorkflows
                .FirstOrDefaultAsync(w => w.AppointmentId == appointment.Id);

            if (workflow == null)
            {
                workflow = new AgentWorkflow
                {
                    AppointmentId = appointment.Id,
                    Objective = $"Safety review for appointment {appointment.Id}",
                    Status = WorkflowStatus.Running,
                    RequiresHumanApproval = false,
                    CorrelationId = $"appointment-{appointment.Id}-safety-audit",
                    ContractVersion = "safety-audit.v1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.AgentWorkflows.Add(workflow);
                await _context.SaveChangesAsync();
            }

            var request = new SafetyAuditStartRequestDto
            {
                Objective = workflow.Objective,
                CorrelationId = workflow.CorrelationId,
                ContractVersion = workflow.ContractVersion,
                SourceAgent = "Component2.AppointmentService",
                AppointmentId = appointment.Id,
                Proposal = new Dictionary<string, object?>
                {
                    ["action"] = "review_appointment",
                    ["appointmentId"] = appointment.Id,
                    ["evidence"] = new[] { $"appointment:{appointment.Id}" }
                }
            };

            var auditResult = await _safetyAuditorService.StartAsync(workflow.Id, request);
            workflow.Status = auditResult.Status;
            workflow.RequiresHumanApproval = auditResult.RequiresApproval;
            workflow.RiskLevel = auditResult.RiskLevel;
            workflow.ValidationSummary = auditResult.ValidationSummary;
            workflow.FinalOutcome = auditResult.FinalOutcome;
            workflow.ErrorCode = auditResult.Error?.Code;
            workflow.ErrorMessage = auditResult.Error?.Message;
            workflow.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception error)
        {
            if (workflow != null)
            {
                // The appointment is already durable; the workflow records an unavailable auditor.
                workflow.Status = WorkflowStatus.SafeFailed;
                workflow.ErrorCode = "auditor_unavailable";
                workflow.ErrorMessage = error.Message.Length > 500 ? error.Message[..500] : error.Message;
                workflow.SafeFailedAt = DateTime.UtcNow;
                workflow.UpdatedAt = DateTime.UtcNow;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Ignore DB save errors on safe-fail logging
                }
            }
        }
    }

    public async Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id)
    {
        var appt = await BuildAppointmentQuery()
            .FirstOrDefaultAsync(a => a.Id == id);

        return appt == null ? null : MapToDto(appt);
    }

    public async Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
    {
        var query = BuildAppointmentQuery();

        if (filter.PatientId.HasValue)
            query = query.Where(a => a.PatientId == filter.PatientId.Value);

        if (filter.DoctorId.HasValue)
            query = query.Where(a => a.DoctorId == filter.DoctorId.Value);

        if (filter.ScheduleId.HasValue)
            query = query.Where(a => a.ScheduleId == filter.ScheduleId.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(filter.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.AppointmentDate >= startUtc);
        }

        if (filter.EndDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(filter.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(a => a.AppointmentDate <= endUtc);
        }

        if (filter.UpcomingOnly.HasValue && filter.UpcomingOnly.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(a => a.AppointmentDate >= now &&
                (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));
        }

        var totalCount = await query.CountAsync();
        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AppointmentResponseDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<AppointmentResponseDto>> GetPatientAppointmentHistoryAsync(int patientId, AppointmentFilterDto filter)
    {
        filter.PatientId = patientId;
        return await GetAppointmentsAsync(filter);
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> UpdateAppointmentStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor!.User)
            .Include(a => a.Doctor!.Specialty)
            .Include(a => a.Schedule!.Room)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null)
        {
            return (false, $"Appointment with ID {id} not found.", null);
        }

        if (appt.Status == AppointmentStatus.Completed)
        {
            return (false, "Cannot alter the status of a completed appointment.", null);
        }

        if (appt.Status == AppointmentStatus.Cancelled)
        {
            return (false, "Cannot alter the status of a cancelled appointment.", null);
        }

        // Validate allowed progression: Pending -> Confirmed -> Completed / Cancelled
        switch (dto.Status)
        {
            case AppointmentStatus.Confirmed:
                if (appt.Status != AppointmentStatus.Pending)
                {
                    return (false, $"Cannot transition appointment from {appt.Status} to Confirmed.", null);
                }
                appt.Status = AppointmentStatus.Confirmed;
                break;

            case AppointmentStatus.Completed:
                if (appt.Status != AppointmentStatus.Confirmed)
                {
                    return (false, "Only confirmed appointments can be marked as completed.", null);
                }
                appt.Status = AppointmentStatus.Completed;
                break;

            case AppointmentStatus.Cancelled:
                if (string.IsNullOrWhiteSpace(dto.CancelReason))
                {
                    return (false, "A cancellation reason must be provided when cancelling an appointment.", null);
                }
                appt.Status = AppointmentStatus.Cancelled;
                appt.CancelReason = dto.CancelReason.Trim();
                break;

            case AppointmentStatus.Pending:
                return (false, "Cannot reset appointment status back to Pending.", null);

            default:
                return (false, $"Unsupported status transition: {dto.Status}", null);
        }

        appt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, MapToDto(appt));
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> CancelAppointmentAsync(int id, CancelAppointmentDto dto)
    {
        return await UpdateAppointmentStatusAsync(id, new UpdateAppointmentStatusDto
        {
            Status = AppointmentStatus.Cancelled,
            CancelReason = dto.CancelReason
        });
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor!.User)
            .Include(a => a.Doctor!.Specialty)
            .Include(a => a.Schedule!.Room)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null)
        {
            return (false, $"Appointment with ID {id} not found.", null);
        }

        if (dto.DoctorId.HasValue && dto.DoctorId.Value > 0)
        {
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId.Value);
            if (doctor != null) appt.DoctorId = doctor.Id;
        }

        if (dto.ScheduleId.HasValue && dto.ScheduleId.Value > 0)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(dto.ScheduleId.Value);
            if (schedule != null)
            {
                appt.ScheduleId = schedule.Id;
                if (!dto.DoctorId.HasValue || dto.DoctorId.Value == 0)
                {
                    appt.DoctorId = schedule.DoctorId;
                }
            }
        }

        if (dto.AppointmentDate.HasValue && dto.AppointmentDate.Value != default)
        {
            appt.AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate.Value, DateTimeKind.Utc);
        }

        if (dto.ReasonForVisit != null)
        {
            appt.ReasonForVisit = dto.ReasonForVisit.Trim();
        }

        if (dto.Status.HasValue)
        {
            appt.Status = dto.Status.Value;
            if (dto.Status.Value == AppointmentStatus.Cancelled && !string.IsNullOrWhiteSpace(dto.CancelReason))
            {
                appt.CancelReason = dto.CancelReason.Trim();
            }
        }

        appt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var updated = await GetAppointmentByIdAsync(id);
        return (true, null, updated);
    }

    public async Task<List<ChannelSlotDto>> GetAvailableChannelSlotsAsync(ChannelSlotFilterDto filter)
    {
        IQueryable<DoctorSchedule> query = _context.DoctorSchedules
            .Include(s => s.Doctor!.User)
            .Include(s => s.Doctor!.Specialty)
            .Include(s => s.Room)
            .AsNoTracking();

        if (filter.DoctorId.HasValue)
        {
            query = query.Where(s => s.DoctorId == filter.DoctorId.Value);
        }

        if (filter.SpecialtyId.HasValue)
        {
            query = query.Where(s => s.Doctor != null && s.Doctor.SpecialtyId == filter.SpecialtyId.Value);
        }

        if (filter.Date.HasValue)
        {
            var filterDateUtc = DateTime.SpecifyKind(filter.Date.Value.Date, DateTimeKind.Utc);
            query = query.Where(s => s.StartTime.Date == filterDateUtc);
        }

        var schedules = await query.ToListAsync();
        var scheduleIds = schedules.Select(s => s.Id).ToList();

        // Calculate booked slots for each schedule
        var bookingCounts = await _context.Appointments
            .Where(a => scheduleIds.Contains(a.ScheduleId) &&
                        (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .GroupBy(a => a.ScheduleId)
            .Select(g => new { ScheduleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ScheduleId, x => x.Count);

        var slots = schedules.Select(s =>
        {
            var booked = bookingCounts.TryGetValue(s.Id, out var count) ? count : 0;
            return new ChannelSlotDto
            {
                ScheduleId = s.Id,
                DoctorId = s.DoctorId,
                DoctorName = s.Doctor?.User?.FullName ?? "Unknown Doctor",
                Qualifications = s.Doctor?.Qualifications ?? string.Empty,
                SpecialtyId = s.Doctor?.SpecialtyId ?? 0,
                SpecialtyName = s.Doctor?.Specialty?.Name ?? "General",
                RoomId = s.RoomId,
                RoomName = s.Room?.RoomName ?? "Unassigned",
                RoomFloor = s.Room?.Floor ?? "N/A",
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                MaxPatients = s.MaxPatients,
                BookedSlots = booked
            };
        }).ToList();

        if (filter.AvailableOnly.HasValue && filter.AvailableOnly.Value)
        {
            slots = slots.Where(s => s.IsAvailable).ToList();
        }

        return slots;
    }

    private IQueryable<Models.Appointment> BuildAppointmentQuery()
    {
        return _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor!.User)
            .Include(a => a.Doctor!.Specialty)
            .Include(a => a.Schedule!.Room)
            .Include(a => a.TriageAssessments)
            .AsNoTracking();
    }

    private static AppointmentResponseDto MapToDto(Models.Appointment a)
    {
        var latestTriage = a.TriageAssessments?
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault();

        return new AppointmentResponseDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient?.Name ?? "Unknown",
            PatientPhone = a.Patient?.PhoneNumber ?? string.Empty,
            PatientNIC = a.Patient?.NIC ?? string.Empty,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.FullName ?? "Unknown",
            DoctorSpecialty = a.Doctor?.Specialty?.Name ?? "General",
            ScheduleId = a.ScheduleId,
            RoomName = a.Schedule?.Room?.RoomName ?? "Unassigned",
            RoomFloor = a.Schedule?.Room?.Floor ?? "N/A",
            AppointmentDate = a.AppointmentDate,
            Status = a.Status,
            ReasonForVisit = a.ReasonForVisit,
            CancelReason = a.CancelReason,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            NormalizedRawSymptoms = latestTriage?.RawSymptoms,
            TriageAssessmentId = latestTriage?.Id
        };
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> AssignDoctorAndScheduleAsync(int id, string recommendedSpecialty)
    {
        var appt = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null)
        {
            return (false, $"Appointment with ID {id} not found.", null);
        }

        var now = DateTime.UtcNow;
        DoctorSchedule? matchedSchedule = null;

        var cleanSpecialty = recommendedSpecialty?.Trim().ToLower() ?? "";
        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Name.ToLower().Contains(cleanSpecialty) || cleanSpecialty.Contains(s.Name.ToLower()));

        if (specialty != null)
        {
            var specialtyDocIds = await _context.Doctors
                .Where(d => d.SpecialtyId == specialty.Id)
                .Select(d => d.Id)
                .ToListAsync();

            if (specialtyDocIds.Any())
            {
                // Match ONLY future active schedules for doctors of matching specialty
                matchedSchedule = await _context.DoctorSchedules
                    .Include(s => s.Doctor)
                    .ThenInclude(d => d!.User)
                    .Include(s => s.Doctor)
                    .ThenInclude(d => d!.Specialty)
                    .Include(s => s.Room)
                    .Where(s => specialtyDocIds.Contains(s.DoctorId) && s.EndTime >= now)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefaultAsync();
            }
        }

        var workflow = await _context.AgentWorkflows
            .FirstOrDefaultAsync(w => w.AppointmentId == appt.Id);

        if (matchedSchedule != null)
        {
            appt.DoctorId = matchedSchedule.DoctorId;
            appt.ScheduleId = matchedSchedule.Id;
            appt.AppointmentDate = matchedSchedule.StartTime >= now ? matchedSchedule.StartTime : now.AddDays(1);
            appt.Status = AppointmentStatus.Confirmed;
            appt.UpdatedAt = now;

            if (workflow != null)
            {
                workflow.Status = WorkflowStatus.Completed;
                workflow.RequiresHumanApproval = false;
                workflow.ValidationSummary = $"Automatically assigned to Dr. {matchedSchedule.Doctor?.User?.FullName ?? "Specialist"} ({matchedSchedule.Room?.RoomName ?? "Room TBD"}).";
                workflow.UpdatedAt = now;
            }
        }
        else
        {
            // NO valid future schedule available for recommended specialty:
            // Do NOT assign doctor/schedule, keep status PAUSED / PENDING for admin review.
            appt.DoctorId = 0;
            appt.ScheduleId = 0;
            appt.Status = AppointmentStatus.Pending;
            appt.UpdatedAt = now;

            if (workflow == null)
            {
                workflow = new AgentWorkflow
                {
                    AppointmentId = appt.Id,
                    Objective = $"Schedule assignment for appointment #{appt.Id} ({recommendedSpecialty})",
                    CorrelationId = $"appointment-{appt.Id}-scheduling",
                    ContractVersion = "v1",
                    CreatedAt = now
                };
                _context.AgentWorkflows.Add(workflow);
            }

            workflow.Status = WorkflowStatus.PausedForApproval;
            workflow.RequiresHumanApproval = true;
            workflow.ValidationSummary = $"No valid schedules available for recommended specialty '{recommendedSpecialty}'. Appointment is paused pending human admin schedule assignment.";
            workflow.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();

        var result = await GetAppointmentByIdAsync(appt.Id);
        return (true, null, result);
    }
}
