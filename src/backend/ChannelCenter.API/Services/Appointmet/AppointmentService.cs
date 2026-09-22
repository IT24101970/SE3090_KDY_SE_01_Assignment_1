using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.DTOs.Common;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Appointment;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;

    public AppointmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, AppointmentResponseDto? Data)> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        var patient = await _context.Patients.FindAsync(dto.PatientId);
        if (patient == null)
        {
            return (false, $"Patient with ID {dto.PatientId} does not exist.", null);
        }

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);
        if (doctor == null)
        {
            return (false, $"Doctor with ID {dto.DoctorId} does not exist.", null);
        }

        var schedule = await _context.DoctorSchedules
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == dto.ScheduleId);
        if (schedule == null)
        {
            return (false, $"Schedule with ID {dto.ScheduleId} does not exist.", null);
        }

        if (schedule.DoctorId != dto.DoctorId)
        {
            return (false, $"Schedule {dto.ScheduleId} does not belong to Doctor {dto.DoctorId}.", null);
        }

        // Prevent duplicate active booking for the same patient on this schedule
        var duplicateBooking = await _context.Appointments.AnyAsync(a =>
            a.PatientId == dto.PatientId &&
            a.ScheduleId == dto.ScheduleId &&
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        if (duplicateBooking)
        {
            return (false, "Patient already has an active appointment for this channel schedule.", null);
        }

        // Check channel slot capacity against MaxPatients
        var activeAppointmentsCount = await _context.Appointments.CountAsync(a =>
            a.ScheduleId == dto.ScheduleId &&
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        if (schedule.MaxPatients > 0 && activeAppointmentsCount >= schedule.MaxPatients)
        {
            return (false, $"Channel session is fully booked (Maximum {schedule.MaxPatients} patients reached).", null);
        }

        var now = DateTime.UtcNow;
        var appointment = new Models.Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            ScheduleId = dto.ScheduleId,
            AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc),
            Status = AppointmentStatus.Pending,
            ReasonForVisit = dto.ReasonForVisit.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var response = await GetAppointmentByIdAsync(appointment.Id);
        return (true, null, response);
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
            .AsNoTracking();
    }

    private static AppointmentResponseDto MapToDto(Models.Appointment a)
    {
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
            UpdatedAt = a.UpdatedAt
        };
    }
}
