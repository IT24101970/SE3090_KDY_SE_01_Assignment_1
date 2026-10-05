using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.SafetyAuditor;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.SafetyAuditor;

public sealed class SafetyAuditorReadService : ISafetyAuditorReadService
{
    private readonly ApplicationDbContext _context;

    public SafetyAuditorReadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentAuditContextDto?> GetAppointmentContextAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor!.Specialty)
            .Include(a => a.Schedule)
            .Include(a => a.TriageAssessments)
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

        if (appointment == null)
        {
            return null;
        }

        var schedule = appointment.Schedule;
        var bookedPatients = schedule == null
            ? 0
            : await _context.Appointments.CountAsync(a =>
                a.ScheduleId == schedule.Id &&
                a.Id != appointmentId &&
                (a.Status == AppointmentStatus.Pending ||
                 a.Status == AppointmentStatus.Confirmed), cancellationToken);

        var now = DateTime.UtcNow;
        var isOnApprovedLeave = schedule != null &&
            await _context.DoctorLeaves.AsNoTracking().AnyAsync(l =>
                l.DoctorId == appointment.DoctorId &&
                l.Status == LeaveStatus.Approved &&
                l.StartDate <= schedule.EndTime &&
                l.EndDate >= schedule.StartTime, cancellationToken);
        var isAvailable = schedule != null &&
            schedule.DoctorId == appointment.DoctorId &&
            schedule.EndTime > now &&
            !isOnApprovedLeave &&
            (schedule.MaxPatients <= 0 || bookedPatients < schedule.MaxPatients);

        var triage = appointment.TriageAssessments
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        return new AppointmentAuditContextDto
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            ScheduleId = appointment.ScheduleId,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentStatus = appointment.Status.ToString(),
            ReasonForVisit = appointment.ReasonForVisit,
            Triage = triage == null
                ? null
                : new TriageAuditContextDto
                {
                    Id = triage.Id,
                    UrgencyLevel = triage.UrgencyLevel.ToString(),
                    RecommendedSpecialty = triage.RecommendedSpecialty,
                    RawSymptoms = triage.RawSymptoms
                },
            Doctor = appointment.Doctor == null
                ? null
                : new DoctorAuditContextDto
                {
                    Id = appointment.Doctor.Id,
                    SpecialtyId = appointment.Doctor.SpecialtyId,
                    SpecialtyName = appointment.Doctor.Specialty?.Name ?? string.Empty
                },
            Schedule = schedule == null
                ? null
                : new ScheduleAuditContextDto
                {
                    Id = schedule.Id,
                    DoctorId = schedule.DoctorId,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    MaxPatients = schedule.MaxPatients,
                    BookedPatients = bookedPatients,
                    IsAvailable = isAvailable
                }
        };
    }
}
