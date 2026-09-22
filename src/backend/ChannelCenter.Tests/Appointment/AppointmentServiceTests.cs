using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Appointment;
using Xunit;

namespace ChannelCenter.Tests.Appointments;

public class AppointmentServiceTests
{
    private static async Task<(Patient patient, Doctor doctor, DoctorSchedule schedule)> SetupBaseDataAsync(ChannelCenter.API.Data.ApplicationDbContext context, int maxPatients = 5)
    {
        var user = new User { FullName = "Dr. Silva", Email = "silva@hospital.com", Role = UserRole.Doctor };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var specialty = new Specialty { Name = "Cardiology", Description = "Heart Specialist" };
        context.Specialties.Add(specialty);
        await context.SaveChangesAsync();

        var doctor = new Doctor
        {
            UserId = user.Id,
            SpecialtyId = specialty.Id,
            Qualifications = "MBBS, MD",
            User = user,
            Specialty = specialty
        };
        context.Doctors.Add(doctor);

        var room = new ConsultationRoom { RoomName = "Room 101", Floor = "1st Floor", IsActive = true };
        context.ConsultationRooms.Add(room);
        await context.SaveChangesAsync();

        var schedule = new DoctorSchedule
        {
            DoctorId = doctor.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxPatients = maxPatients,
            Doctor = doctor,
            Room = room
        };
        context.DoctorSchedules.Add(schedule);

        var patient = new Patient
        {
            Name = "John Perera",
            NIC = "911223344V",
            Email = "john.p@gmail.com",
            PhoneNumber = "0771234567",
            Gender = "Male",
            DateOfBirth = DateTime.UtcNow.AddYears(-35)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        return (patient, doctor, schedule);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ValidSlot_CreatesPendingAppointment()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var (patient, doctor, schedule) = await SetupBaseDataAsync(context);
        var service = new AppointmentService(context);

        var dto = new CreateAppointmentDto
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduleId = schedule.Id,
            AppointmentDate = schedule.StartTime,
            ReasonForVisit = "Annual cardiac checkup"
        };

        var (success, errorMessage, data) = await service.CreateAppointmentAsync(dto);

        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(data);
        Assert.Equal(AppointmentStatus.Pending, data.Status);
        Assert.Equal("Annual cardiac checkup", data.ReasonForVisit);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ExceedsCapacity_ReturnsError()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var (patient, doctor, schedule) = await SetupBaseDataAsync(context, maxPatients: 1);
        var service = new AppointmentService(context);

        // Fill the 1 slot
        context.Appointments.Add(new Appointment
        {
            PatientId = 99,
            DoctorId = doctor.Id,
            ScheduleId = schedule.Id,
            AppointmentDate = schedule.StartTime,
            Status = AppointmentStatus.Confirmed,
            ReasonForVisit = "First patient"
        });
        await context.SaveChangesAsync();

        var dto = new CreateAppointmentDto
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduleId = schedule.Id,
            AppointmentDate = schedule.StartTime,
            ReasonForVisit = "Should fail due to max capacity"
        };

        var (success, errorMessage, data) = await service.CreateAppointmentAsync(dto);

        Assert.False(success);
        Assert.Contains("fully booked", errorMessage);
        Assert.Null(data);
    }

    [Fact]
    public async Task StatusProgression_PendingToConfirmedToCompleted_Succeeds()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var (patient, doctor, schedule) = await SetupBaseDataAsync(context);
        var service = new AppointmentService(context);

        var appt = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduleId = schedule.Id,
            AppointmentDate = schedule.StartTime,
            Status = AppointmentStatus.Pending,
            ReasonForVisit = "Routine"
        };
        context.Appointments.Add(appt);
        await context.SaveChangesAsync();

        // Step 1: Pending -> Confirmed
        var (confSuccess, confErr, confData) = await service.UpdateAppointmentStatusAsync(appt.Id, new UpdateAppointmentStatusDto
        {
            Status = AppointmentStatus.Confirmed
        });
        Assert.True(confSuccess);
        Assert.Equal(AppointmentStatus.Confirmed, confData!.Status);

        // Step 2: Confirmed -> Completed
        var (compSuccess, compErr, compData) = await service.UpdateAppointmentStatusAsync(appt.Id, new UpdateAppointmentStatusDto
        {
            Status = AppointmentStatus.Completed
        });
        Assert.True(compSuccess);
        Assert.Equal(AppointmentStatus.Completed, compData!.Status);

        // Step 3: Completed cannot be changed
        var (failSuccess, failErr, _) = await service.UpdateAppointmentStatusAsync(appt.Id, new UpdateAppointmentStatusDto
        {
            Status = AppointmentStatus.Cancelled,
            CancelReason = "Try cancel"
        });
        Assert.False(failSuccess);
        Assert.Contains("completed appointment", failErr);
    }

    [Fact]
    public async Task CancelAppointmentAsync_WithReason_SucceedsAndSetsTimestamp()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var (patient, doctor, schedule) = await SetupBaseDataAsync(context);
        var service = new AppointmentService(context);

        var appt = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduleId = schedule.Id,
            AppointmentDate = schedule.StartTime,
            Status = AppointmentStatus.Pending,
            ReasonForVisit = "Flu"
        };
        context.Appointments.Add(appt);
        await context.SaveChangesAsync();

        var (success, errorMessage, data) = await service.CancelAppointmentAsync(appt.Id, new CancelAppointmentDto
        {
            CancelReason = "Emergency travel overseas"
        });

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(AppointmentStatus.Cancelled, data.Status);
        Assert.Equal("Emergency travel overseas", data.CancelReason);

        var updated = await context.Appointments.FindAsync(appt.Id);
        Assert.Equal(AppointmentStatus.Cancelled, updated!.Status);
        Assert.Equal("Emergency travel overseas", updated.CancelReason);
    }

    [Fact]
    public async Task GetAvailableChannelSlotsAsync_CalculatesRemainingCapacityCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var (patient, doctor, schedule) = await SetupBaseDataAsync(context, maxPatients: 10);
        var service = new AppointmentService(context);

        // Add 3 booked appointments
        context.Appointments.AddRange(
            new Appointment { PatientId = 1, DoctorId = doctor.Id, ScheduleId = schedule.Id, Status = AppointmentStatus.Pending, AppointmentDate = schedule.StartTime, ReasonForVisit = "A" },
            new Appointment { PatientId = 2, DoctorId = doctor.Id, ScheduleId = schedule.Id, Status = AppointmentStatus.Confirmed, AppointmentDate = schedule.StartTime, ReasonForVisit = "B" },
            new Appointment { PatientId = 3, DoctorId = doctor.Id, ScheduleId = schedule.Id, Status = AppointmentStatus.Confirmed, AppointmentDate = schedule.StartTime, ReasonForVisit = "C" },
            new Appointment { PatientId = 4, DoctorId = doctor.Id, ScheduleId = schedule.Id, Status = AppointmentStatus.Cancelled, AppointmentDate = schedule.StartTime, ReasonForVisit = "D" } // Cancelled should NOT count towards capacity!
        );
        await context.SaveChangesAsync();

        var slots = await service.GetAvailableChannelSlotsAsync(new ChannelSlotFilterDto());

        Assert.Single(slots);
        var slot = slots.First();
        Assert.Equal(10, slot.MaxPatients);
        Assert.Equal(3, slot.BookedSlots);
        Assert.Equal(7, slot.AvailableSlots);
        Assert.True(slot.IsAvailable);
    }
}
