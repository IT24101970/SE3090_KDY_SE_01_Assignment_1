using ChannelCenter.API.DTOs.Patient;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Patient;
using Xunit;

namespace ChannelCenter.Tests.Patients;

public class PatientServiceTests
{
    [Fact]
    public async Task CreatePatientAsync_ValidData_CreatesPatientSuccessfully()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new PatientService(context);

        var dto = new CreatePatientDto
        {
            Name = "Kamal Perera",
            EmergencyContact = "+94771234567",
            DateOfBirth = new DateTime(1990, 5, 12, 0, 0, 0, DateTimeKind.Utc),
            Gender = "Male",
            PhoneNumber = "+94719876543",
            Email = "kamal@example.com",
            NIC = "199013301234",
            BloodGroup = "O+",
            Allergies = "Penicillin",
            MedicalHistory = "Asthma"
        };

        var (success, errorMessage, data) = await service.CreatePatientAsync(dto);

        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(data);
        Assert.Equal("Kamal Perera", data.Name);
        Assert.Equal("O+", data.BloodGroup);
        Assert.Equal("Penicillin", data.Allergies);
        Assert.Equal("Asthma", data.MedicalHistory);
        Assert.True(data.Id > 0);

        var saved = await context.Patients.FindAsync(data.Id);
        Assert.NotNull(saved);
        Assert.Equal("199013301234", saved.NIC);
    }

    [Fact]
    public async Task CreatePatientAsync_DuplicateNIC_ReturnsError()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new PatientService(context);

        context.Patients.Add(new Patient
        {
            Name = "Existing Patient",
            NIC = "990011223V",
            Email = "existing@example.com",
            PhoneNumber = "0771112233",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        });
        await context.SaveChangesAsync();

        var dto = new CreatePatientDto
        {
            Name = "New Patient",
            NIC = "990011223V",
            Email = "new@example.com",
            PhoneNumber = "0774445566",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            EmergencyContact = "0778889900",
            Gender = "Female"
        };

        var (success, errorMessage, data) = await service.CreatePatientAsync(dto);

        Assert.False(success);
        Assert.Contains("already registered", errorMessage);
        Assert.Null(data);
    }

    [Fact]
    public async Task GetPatientsAsync_WithSearchTerm_ReturnsMatchingPatients()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new PatientService(context);

        context.Patients.AddRange(
            new Patient { Name = "Sunil Silva", NIC = "851122334V", Email = "sunil@gmail.com", PhoneNumber = "0711111111", DateOfBirth = DateTime.UtcNow.AddYears(-40), Gender = "Male" },
            new Patient { Name = "Nimal Fernando", NIC = "901122334V", Email = "nimal@gmail.com", PhoneNumber = "0722222222", DateOfBirth = DateTime.UtcNow.AddYears(-35), Gender = "Male" },
            new Patient { Name = "Kamani Perera", NIC = "951122334V", Email = "kamani@gmail.com", PhoneNumber = "0733333333", DateOfBirth = DateTime.UtcNow.AddYears(-28), Gender = "Female" }
        );
        await context.SaveChangesAsync();

        var result = await service.GetPatientsAsync(new PatientFilterDto { SearchTerm = "silva" });

        Assert.Single(result.Items);
        Assert.Equal("Sunil Silva", result.Items.First().Name);
    }

    [Fact]
    public async Task UpdatePatientAsync_UpdatesProfileAndMedicalNotes()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new PatientService(context);

        var patient = new Patient
        {
            Name = "John Doe",
            NIC = "801122334V",
            Email = "john@example.com",
            PhoneNumber = "0770001122",
            Gender = "Male",
            DateOfBirth = DateTime.UtcNow.AddYears(-45)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var updateDto = new UpdatePatientDto
        {
            Name = "Johnathon Doe",
            PhoneNumber = "0779998877",
            BloodGroup = "A-",
            Allergies = "Peanuts",
            MedicalHistory = "Hypertension"
        };

        var (success, errorMessage, data) = await service.UpdatePatientAsync(patient.Id, updateDto);

        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(data);
        Assert.Equal("Johnathon Doe", data.Name);
        Assert.Equal("A-", data.BloodGroup);
        Assert.Equal("Peanuts", data.Allergies);
        Assert.Equal("Hypertension", data.MedicalHistory);
    }

    [Fact]
    public async Task DeletePatientAsync_WithActiveAppointments_FailsWithNotice()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new PatientService(context);

        var patient = new Patient
        {
            Name = "Active Booking Patient",
            NIC = "889977665V",
            Email = "active@hospital.com",
            PhoneNumber = "0773332211",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        context.Appointments.Add(new Appointment
        {
            PatientId = patient.Id,
            DoctorId = 1,
            ScheduleId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(2),
            Status = AppointmentStatus.Pending,
            ReasonForVisit = "Fever"
        });
        await context.SaveChangesAsync();

        var (success, errorMessage) = await service.DeletePatientAsync(patient.Id);

        Assert.False(success);
        Assert.Contains("active appointments", errorMessage);
    }
}
