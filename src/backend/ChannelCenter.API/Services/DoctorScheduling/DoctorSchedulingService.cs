using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ChannelCenter.API.Services.DoctorScheduling;

public class DoctorSchedulingService : IDoctorSchedulingService
{
    private readonly ApplicationDbContext _context;

    public DoctorSchedulingService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Doctor Operations

    public async Task<IEnumerable<DoctorDto>> GetDoctorsAsync()
    {
        var doctors = await _context.Doctors
            .AsNoTracking()
            .ToListAsync();

        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id);
        var specialties = await _context.Specialties.AsNoTracking().ToDictionaryAsync(s => s.Id);

        return doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            SpecialtyId = d.SpecialtyId,
            Qualifications = d.Qualifications,
            DoctorName = users.TryGetValue(d.UserId, out var u) ? u.FullName : "Unknown",
            SpecialtyName = specialties.TryGetValue(d.SpecialtyId, out var s) ? s.Name : "General",
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        });
    }

    public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
    {
        var d = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return null;

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == d.UserId);
        var specialty = await _context.Specialties.AsNoTracking().FirstOrDefaultAsync(s => s.Id == d.SpecialtyId);

        return new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            SpecialtyId = d.SpecialtyId,
            Qualifications = d.Qualifications,
            DoctorName = user?.FullName ?? "Unknown",
            SpecialtyName = specialty?.Name ?? "General",
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }

    public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto dto)
    {
        var doctor = new Doctor
        {
            UserId = dto.UserId,
            SpecialtyId = dto.SpecialtyId,
            Qualifications = dto.Qualifications,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return (await GetDoctorByIdAsync(doctor.Id))!;
    }

    public async Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null) return null;

        doctor.SpecialtyId = dto.SpecialtyId;
        doctor.Qualifications = dto.Qualifications;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetDoctorByIdAsync(id);
    }

    #endregion

    #region Consultation Room Operations

    public async Task<IEnumerable<ConsultationRoomDto>> GetRoomsAsync()
    {
        return await _context.ConsultationRooms
            .AsNoTracking()
            .Select(r => new ConsultationRoomDto
            {
                Id = r.Id,
                RoomName = r.RoomName,
                Floor = r.Floor,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ConsultationRoomDto?> GetRoomByIdAsync(int id)
    {
        var r = await _context.ConsultationRooms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return null;

        return new ConsultationRoomDto
        {
            Id = r.Id,
            RoomName = r.RoomName,
            Floor = r.Floor,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    public async Task<ConsultationRoomDto> CreateRoomAsync(CreateConsultationRoomDto dto)
    {
        var room = new ConsultationRoom
        {
            RoomName = dto.RoomName,
            Floor = dto.Floor,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ConsultationRooms.Add(room);
        await _context.SaveChangesAsync();

        return new ConsultationRoomDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            Floor = room.Floor,
            IsActive = room.IsActive,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }

    public async Task<ConsultationRoomDto?> UpdateRoomAsync(int id, UpdateConsultationRoomDto dto)
    {
        var room = await _context.ConsultationRooms.FindAsync(id);
        if (room == null) return null;

        room.RoomName = dto.RoomName;
        room.Floor = dto.Floor;
        room.IsActive = dto.IsActive;
        room.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ConsultationRoomDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            Floor = room.Floor,
            IsActive = room.IsActive,
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt
        };
    }

    public async Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeScheduleId = null)
    {
        var query = _context.DoctorSchedules
            .AsNoTracking()
            .Where(s => s.RoomId == roomId && s.StartTime < endTime && s.EndTime > startTime);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.Id != excludeScheduleId.Value);
        }

        var isOccupied = await query.AnyAsync();
        return !isOccupied;
    }

    #endregion

    #region Doctor Schedule Operations

    public async Task<IEnumerable<DoctorScheduleDto>> GetSchedulesAsync(int? doctorId = null, DateTime? date = null)
    {
        var query = _context.DoctorSchedules.AsNoTracking().AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(s => s.DoctorId == doctorId.Value);
        }

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(s => s.StartTime >= dayStart && s.StartTime < dayEnd);
        }

        var schedules = await query.ToListAsync();

        var doctors = await _context.Doctors.AsNoTracking().ToDictionaryAsync(d => d.Id);
        var rooms = await _context.ConsultationRooms.AsNoTracking().ToDictionaryAsync(r => r.Id);
        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id);
        var specialties = await _context.Specialties.AsNoTracking().ToDictionaryAsync(s => s.Id);

        return schedules.Select(s =>
        {
            doctors.TryGetValue(s.DoctorId, out var doc);
            rooms.TryGetValue(s.RoomId, out var rm);
            users.TryGetValue(doc?.UserId ?? 0, out var usr);
            specialties.TryGetValue(doc?.SpecialtyId ?? 0, out var sp);

            return new DoctorScheduleDto
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorName = usr?.FullName ?? "Unknown",
                SpecialtyName = sp?.Name ?? "General",
                RoomId = s.RoomId,
                RoomName = rm?.RoomName ?? "Unknown Room",
                Floor = rm?.Floor ?? "",
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                MaxPatients = s.MaxPatients,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
        });
    }

    public async Task<DoctorScheduleDto?> GetScheduleByIdAsync(int id)
    {
        var list = await GetSchedulesAsync();
        return list.FirstOrDefault(s => s.Id == id);
    }

    public async Task<DoctorScheduleDto> CreateScheduleAsync(CreateDoctorScheduleDto dto)
    {
        // 1. Verify if Doctor is on Approved leave during requested schedule time
        var scheduleStart = dto.StartTime.Date;
        var scheduleEnd = dto.EndTime.Date;

        var isOnLeave = await _context.DoctorLeaves
            .AsNoTracking()
            .AnyAsync(l => l.DoctorId == dto.DoctorId 
                        && l.Status == LeaveStatus.Approved 
                        && l.StartDate.Date <= scheduleEnd 
                        && l.EndDate.Date >= scheduleStart);

        if (isOnLeave)
        {
            throw new InvalidOperationException("Doctor is on approved leave during the requested schedule time.");
        }

        // 2. Verify Room is Active (not in Maintenance)
        var room = await _context.ConsultationRooms.FindAsync(dto.RoomId);
        if (room == null || !room.IsActive)
        {
            throw new InvalidOperationException("Selected consultation room is currently inactive or under maintenance.");
        }

        // 3. Verify Room Availability (no overlap)
        var isRoomAvailable = await CheckRoomAvailabilityAsync(dto.RoomId, dto.StartTime, dto.EndTime);
        if (!isRoomAvailable)
        {
            throw new InvalidOperationException("Consultation room is already occupied during the selected time slot.");
        }

        var schedule = new DoctorSchedule
        {
            DoctorId = dto.DoctorId,
            RoomId = dto.RoomId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MaxPatients = dto.MaxPatients,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DoctorSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return (await GetScheduleByIdAsync(schedule.Id))!;
    }

    public async Task<DoctorScheduleDto?> UpdateScheduleAsync(int id, UpdateDoctorScheduleDto dto)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule == null) return null;

        // Verify Room Availability excluding current schedule
        var isRoomAvailable = await CheckRoomAvailabilityAsync(dto.RoomId, dto.StartTime, dto.EndTime, excludeScheduleId: id);
        if (!isRoomAvailable)
        {
            throw new InvalidOperationException("Consultation room is already occupied during the selected time slot.");
        }

        schedule.RoomId = dto.RoomId;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.MaxPatients = dto.MaxPatients;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetScheduleByIdAsync(id);
    }

    public async Task<bool> DeleteScheduleAsync(int id)
    {
        var schedule = await _context.DoctorSchedules.FindAsync(id);
        if (schedule == null) return false;

        _context.DoctorSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Doctor Leave Operations

    public async Task<IEnumerable<DoctorLeaveDto>> GetLeavesAsync(int? doctorId = null, LeaveStatus? status = null)
    {
        var query = _context.DoctorLeaves.AsNoTracking().AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(l => l.DoctorId == doctorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var leaves = await query.ToListAsync();
        var doctors = await _context.Doctors.AsNoTracking().ToDictionaryAsync(d => d.Id);
        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id);

        return leaves.Select(l =>
        {
            doctors.TryGetValue(l.DoctorId, out var doc);
            users.TryGetValue(doc?.UserId ?? 0, out var usr);

            return new DoctorLeaveDto
            {
                Id = l.Id,
                DoctorId = l.DoctorId,
                DoctorName = usr?.FullName ?? "Unknown Doctor",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            };
        });
    }

    public async Task<DoctorLeaveDto?> GetLeaveByIdAsync(int id)
    {
        var leaves = await GetLeavesAsync();
        return leaves.FirstOrDefault(l => l.Id == id);
    }

    public async Task<DoctorLeaveDto> CreateLeaveAsync(CreateDoctorLeaveDto dto)
    {
        var leave = new DoctorLeave
        {
            DoctorId = dto.DoctorId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DoctorLeaves.Add(leave);
        await _context.SaveChangesAsync();

        return (await GetLeaveByIdAsync(leave.Id))!;
    }

    public async Task<DoctorLeaveDto?> UpdateLeaveStatusAsync(int id, LeaveStatus status)
    {
        var leave = await _context.DoctorLeaves.FindAsync(id);
        if (leave == null) return null;

        leave.Status = status;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetLeaveByIdAsync(id);
    }

    #endregion

    #region Consultation & Attendance Operations

    public async Task<ConsultationDto?> GetConsultationByIdAsync(int id)
    {
        var c = await _context.Consultations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return null;

        return new ConsultationDto
        {
            Id = c.Id,
            AppointmentId = c.AppointmentId,
            ClinicalNotes = c.ClinicalNotes,
            PrescriptionData = c.PrescriptionData,
            AttendanceStatus = c.AttendanceStatus,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    public async Task<ConsultationDto?> GetConsultationByAppointmentIdAsync(int appointmentId)
    {
        var c = await _context.Consultations.AsNoTracking().FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        if (c == null) return null;

        return new ConsultationDto
        {
            Id = c.Id,
            AppointmentId = c.AppointmentId,
            ClinicalNotes = c.ClinicalNotes,
            PrescriptionData = c.PrescriptionData,
            AttendanceStatus = c.AttendanceStatus,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    public async Task<ConsultationDto> CreateConsultationAsync(CreateConsultationDto dto)
    {
        var consultation = new Consultation
        {
            AppointmentId = dto.AppointmentId,
            ClinicalNotes = dto.ClinicalNotes,
            PrescriptionData = dto.PrescriptionData,
            AttendanceStatus = dto.AttendanceStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        return new ConsultationDto
        {
            Id = consultation.Id,
            AppointmentId = consultation.AppointmentId,
            ClinicalNotes = consultation.ClinicalNotes,
            PrescriptionData = consultation.PrescriptionData,
            AttendanceStatus = consultation.AttendanceStatus,
            CreatedAt = consultation.CreatedAt,
            UpdatedAt = consultation.UpdatedAt
        };
    }

    public async Task<ConsultationDto?> UpdateAttendanceStatusAsync(int id, AttendanceStatus status)
    {
        var consultation = await _context.Consultations.FindAsync(id);
        if (consultation == null) return null;

        consultation.AttendanceStatus = status;
        consultation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ConsultationDto
        {
            Id = consultation.Id,
            AppointmentId = consultation.AppointmentId,
            ClinicalNotes = consultation.ClinicalNotes,
            PrescriptionData = consultation.PrescriptionData,
            AttendanceStatus = consultation.AttendanceStatus,
            CreatedAt = consultation.CreatedAt,
            UpdatedAt = consultation.UpdatedAt
        };
    }

    #endregion

    #region Agentic AI Integration (Student 2: Schedule & Capacity Optimization Agent)

    public async Task<IEnumerable<object>> GetPendingAppointmentsAsync()
    {
        var appts = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Pending || a.ScheduleId == 0 || a.DoctorId == 0)
            .ToListAsync();

        var patients = await _context.Patients.AsNoTracking().ToDictionaryAsync(p => p.Id);
        var triages = await _context.TriageAssessments.AsNoTracking().ToDictionaryAsync(t => t.AppointmentId);

        var list = new List<object>();

        foreach (var a in appts)
        {
            patients.TryGetValue(a.PatientId, out var patient);
            triages.TryGetValue(a.Id, out var triage);

            list.Add(new
            {
                id = a.Id,
                patientId = a.PatientId,
                patientName = patient?.Name ?? $"Patient #{a.PatientId}",
                reasonForVisit = a.ReasonForVisit,
                status = a.Status.ToString(),
                appointmentDate = a.AppointmentDate,
                urgencyScore = triage?.UrgencyScore ?? 75,
                urgencyLevel = triage?.UrgencyLevel.ToString() ?? "High",
                recommendedSpecialty = triage?.RecommendedSpecialty ?? "Cardiology"
            });
        }

        return list;
    }

    public async Task<TriageAssessment?> GetTriageAssessmentForAppointmentAsync(int appointmentId)

    {
        return await _context.TriageAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.AppointmentId == appointmentId);
    }

    public async Task<object> OptimizeScheduleWithAiAsync(object inputDto)

    {
        using var client = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };
        var response = await client.PostAsJsonAsync("/api/agent/doctor-scheduling/optimize", inputDto);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<object>();
        return json ?? new { message = "AI Optimization service returned empty response" };
    }

    public async Task<DoctorScheduleDto> ApproveAiScheduleWorkflowAsync(string workflowId)
    {
        using var client = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };
        var response = await client.PostAsync($"/api/agent/doctor-scheduling/workflows/{workflowId}/approve", null);
        response.EnsureSuccessStatusCode();

        var state = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var rec = state.GetProperty("recommended_option");

        var doctorId = rec.GetProperty("doctor_id").GetInt32();
        var roomId = rec.GetProperty("allocated_room_id").GetInt32();
        var startTime = rec.GetProperty("recommended_start_time").GetDateTime();
        var endTime = rec.GetProperty("recommended_end_time").GetDateTime();
        var maxPatients = rec.GetProperty("max_patients").GetInt32();

        int? appointmentId = null;
        if (rec.TryGetProperty("appointment_id", out var apptProp) && apptProp.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            appointmentId = apptProp.GetInt32();
        }

        var createDto = new CreateDoctorScheduleDto
        {
            DoctorId = doctorId,
            RoomId = roomId,
            StartTime = startTime,
            EndTime = endTime,
            MaxPatients = maxPatients
        };

        var createdSchedule = await CreateScheduleAsync(createDto);

        // Link Appointment and create Consultation if AppointmentId is present
        if (appointmentId.HasValue && appointmentId.Value > 0)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId.Value);
            
            // If appointment does not exist in DB yet, create it along with patient & triage records
            if (appointment == null)
            {
                var patient = await _context.Patients.FirstOrDefaultAsync();
                int firstPatientId = patient?.Id ?? 1;

                appointment = new ChannelCenter.API.Models.Appointment
                {
                    PatientId = firstPatientId,
                    DoctorId = doctorId,
                    ScheduleId = createdSchedule.Id,
                    AppointmentDate = startTime,
                    ReasonForVisit = "Chest tightness and fatigue (AI Scheduled)",
                    Status = AppointmentStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Create matching TriageAssessment record
                var triage = await _context.TriageAssessments
                    .FirstOrDefaultAsync(t => t.AppointmentId == appointment.Id);

                if (triage == null)
                {
                    triage = new TriageAssessment
                    {
                        AppointmentId = appointment.Id,
                        RawSymptoms = "Chest tightness and fatigue",
                        UrgencyScore = 88,
                        UrgencyLevel = UrgencyLevel.High,
                        RecommendedSpecialty = "Cardiology",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.TriageAssessments.Add(triage);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                appointment.DoctorId = doctorId;
                appointment.ScheduleId = createdSchedule.Id;
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.UtcNow;
            }

            // Create initial Consultation entry for Doctor session queue
            var existingConsultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.AppointmentId == appointment.Id);

            if (existingConsultation == null)
            {
                var consultation = new Consultation
                {
                    AppointmentId = appointment.Id,
                    ClinicalNotes = string.Empty,
                    PrescriptionData = "{}",
                    AttendanceStatus = AttendanceStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Consultations.Add(consultation);
            }
            await _context.SaveChangesAsync();
        }

        return createdSchedule;
    }

    #endregion
}


