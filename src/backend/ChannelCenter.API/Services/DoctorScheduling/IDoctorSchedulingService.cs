using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Models;

namespace ChannelCenter.API.Services.DoctorScheduling;

public interface IDoctorSchedulingService
{
    // Doctor Operations
    Task<IEnumerable<DoctorDto>> GetDoctorsAsync();
    Task<DoctorDto?> GetDoctorByIdAsync(int id);
    Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto dto);
    Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto dto);

    // Consultation Room Operations
    Task<IEnumerable<ConsultationRoomDto>> GetRoomsAsync();
    Task<ConsultationRoomDto?> GetRoomByIdAsync(int id);
    Task<ConsultationRoomDto> CreateRoomAsync(CreateConsultationRoomDto dto);
    Task<ConsultationRoomDto?> UpdateRoomAsync(int id, UpdateConsultationRoomDto dto);
    Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeScheduleId = null);

    // Doctor Schedule Operations
    Task<IEnumerable<DoctorScheduleDto>> GetSchedulesAsync(int? doctorId = null, DateTime? date = null);
    Task<DoctorScheduleDto?> GetScheduleByIdAsync(int id);
    Task<DoctorScheduleDto> CreateScheduleAsync(CreateDoctorScheduleDto dto);
    Task<DoctorScheduleDto?> UpdateScheduleAsync(int id, UpdateDoctorScheduleDto dto);
    Task<bool> DeleteScheduleAsync(int id);

    // Doctor Leave Operations
    Task<IEnumerable<DoctorLeaveDto>> GetLeavesAsync(int? doctorId = null, LeaveStatus? status = null);
    Task<DoctorLeaveDto?> GetLeaveByIdAsync(int id);
    Task<DoctorLeaveDto> CreateLeaveAsync(CreateDoctorLeaveDto dto);
    Task<DoctorLeaveDto?> UpdateLeaveStatusAsync(int id, LeaveStatus status);

    // Consultation & Attendance Operations
    Task<ConsultationDto?> GetConsultationByIdAsync(int id);
    Task<ConsultationDto?> GetConsultationByAppointmentIdAsync(int appointmentId);
    Task<ConsultationDto> CreateConsultationAsync(CreateConsultationDto dto);
    Task<ConsultationDto?> UpdateAttendanceStatusAsync(int id, AttendanceStatus status);

    // Agentic AI Subsystem Integration (Student 2: Schedule & Capacity Optimization Agent)
    Task<IEnumerable<object>> GetPendingAppointmentsAsync();
    Task<TriageAssessment?> GetTriageAssessmentForAppointmentAsync(int appointmentId);
    Task<object> OptimizeScheduleWithAiAsync(object inputDto);
    Task<DoctorScheduleDto> ApproveAiScheduleWorkflowAsync(string workflowId);
}



