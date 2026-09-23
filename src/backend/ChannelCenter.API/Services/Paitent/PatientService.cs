using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.Common;
using ChannelCenter.API.DTOs.Patient;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.Patient;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;

    public PatientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PatientResponseDto>> GetPatientsAsync(PatientFilterDto filter)
    {
        var query = _context.Patients
            .Include(p => p.Appointments)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.NIC.ToLower().Contains(search) ||
                p.Email.ToLower().Contains(search) ||
                p.PhoneNumber.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Gender))
        {
            query = query.Where(p => p.Gender.ToLower() == filter.Gender.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.BloodGroup))
        {
            query = query.Where(p => p.BloodGroup != null && p.BloodGroup.ToLower() == filter.BloodGroup.Trim().ToLower());
        }

        var totalCount = await query.CountAsync();

        var page = filter.Page > 0 ? filter.Page : 1;
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToResponseDto(p))
            .ToListAsync();

        return new PagedResult<PatientResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PatientResponseDto?> GetPatientByIdAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return patient == null ? null : MapToResponseDto(patient);
    }

    public async Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return patient == null ? null : MapToResponseDto(patient);
    }

    public async Task<(bool Success, string? ErrorMessage, PatientResponseDto? Data)> CreatePatientAsync(CreatePatientDto dto, int? userId = null)
    {
        var existingNic = await _context.Patients.AnyAsync(p => p.NIC.ToLower() == dto.NIC.Trim().ToLower());
        if (existingNic)
        {
            return (false, $"A patient with NIC '{dto.NIC}' is already registered.", null);
        }

        var existingEmail = await _context.Patients.AnyAsync(p => p.Email.ToLower() == dto.Email.Trim().ToLower());
        if (existingEmail)
        {
            return (false, $"A patient with Email '{dto.Email}' is already registered.", null);
        }

        var now = DateTime.UtcNow;
        var patient = new Models.Patient
        {
            UserId = userId ?? dto.UserId,
            Name = dto.Name.Trim(),
            EmergencyContact = dto.EmergencyContact.Trim(),
            DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc),
            Gender = dto.Gender.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Email = dto.Email.Trim().ToLower(),
            NIC = dto.NIC.Trim().ToUpper(),
            BloodGroup = dto.BloodGroup?.Trim(),
            Allergies = dto.Allergies?.Trim(),
            MedicalHistory = dto.MedicalHistory?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return (true, null, MapToResponseDto(patient));
    }

    public async Task<(bool Success, string? ErrorMessage, PatientResponseDto? Data)> UpdatePatientAsync(int id, UpdatePatientDto dto)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return (false, $"Patient with ID {id} not found.", null);
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
            patient.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.EmergencyContact))
            patient.EmergencyContact = dto.EmergencyContact.Trim();

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            patient.PhoneNumber = dto.PhoneNumber.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailLower = dto.Email.Trim().ToLower();
            var emailTaken = await _context.Patients.AnyAsync(p => p.Email.ToLower() == emailLower && p.Id != id);
            if (emailTaken)
            {
                return (false, $"Email '{dto.Email}' is already used by another patient.", null);
            }
            patient.Email = emailLower;
        }

        if (dto.BloodGroup != null)
            patient.BloodGroup = dto.BloodGroup.Trim();

        if (dto.Allergies != null)
            patient.Allergies = dto.Allergies.Trim();

        if (dto.MedicalHistory != null)
            patient.MedicalHistory = dto.MedicalHistory.Trim();

        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null, MapToResponseDto(patient));
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePatientAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return (false, $"Patient with ID {id} not found.");
        }

        var hasActiveAppointments = patient.Appointments.Any(a =>
            a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed);

        if (hasActiveAppointments)
        {
            return (false, "Cannot delete patient with active appointments. Please cancel or complete them first.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static PatientResponseDto MapToResponseDto(Models.Patient p)
    {
        return new PatientResponseDto
        {
            Id = p.Id,
            UserId = p.UserId,
            Name = p.Name,
            EmergencyContact = p.EmergencyContact,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            PhoneNumber = p.PhoneNumber,
            Email = p.Email,
            NIC = p.NIC,
            BloodGroup = p.BloodGroup,
            Allergies = p.Allergies,
            MedicalHistory = p.MedicalHistory,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            TotalAppointments = p.Appointments?.Count ?? 0
        };
    }
}
