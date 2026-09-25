using System.Text.Json;
using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.IntakeAgent;
using Xunit;

namespace ChannelCenter.Tests.IntakeAgent;

public class IntakeAgentServiceTests
{
    private static async Task<Patient> CreateSamplePatientAsync(ChannelCenter.API.Data.ApplicationDbContext context)
    {
        var patient = new Patient
        {
            Name = "Sunil Weerasinghe",
            NIC = "198512345678",
            PhoneNumber = "+94779876543",
            Email = "sunil@gmail.com",
            EmergencyContact = "+94711234567",
            Gender = "Male",
            DateOfBirth = new DateTime(1985, 4, 10, 0, 0, 0, DateTimeKind.Utc),
            BloodGroup = "B+",
            Allergies = "Aspirin",
            MedicalHistory = "Mild hypertension"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    [Fact]
    public async Task ToolValidatePatientEligibility_CompleteProfile_ReturnsEligibleTrue()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = await CreateSamplePatientAsync(context);
        var service = new IntakeAgentService(context);

        var (success, errorMessage, json) = await service.ToolValidatePatientEligibilityAsync(patient.Id);

        Assert.True(success);
        Assert.Null(errorMessage);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("isEligible").GetBoolean());
    }

    [Fact]
    public async Task ToolValidatePatientEligibility_MissingPhone_ReturnsEligibleFalse()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = new Patient
        {
            Name = "Incomplete Patient",
            NIC = "199000000000",
            PhoneNumber = "", // missing!
            Email = "inc@example.com",
            EmergencyContact = "0771111111",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var service = new IntakeAgentService(context);
        var (success, errorMessage, json) = await service.ToolValidatePatientEligibilityAsync(patient.Id);

        Assert.True(success);
        using var doc = JsonDocument.Parse(json);
        Assert.False(doc.RootElement.GetProperty("isEligible").GetBoolean());
    }

    [Fact]
    public async Task ToolGetPatientHistory_ReturnsMedicalProfileAndBookings()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = await CreateSamplePatientAsync(context);
        var service = new IntakeAgentService(context);

        context.Appointments.Add(new Appointment
        {
            PatientId = patient.Id,
            DoctorId = 1,
            ScheduleId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(-10),
            Status = AppointmentStatus.Completed,
            ReasonForVisit = "Cardiology consultation"
        });
        await context.SaveChangesAsync();

        var (success, errorMessage, json) = await service.ToolGetPatientHistoryAsync(patient.Id);

        Assert.True(success);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("B+", doc.RootElement.GetProperty("bloodGroup").GetString());
        Assert.Equal("Aspirin", doc.RootElement.GetProperty("knownAllergies").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("recentAppointmentsCount").GetInt32());
    }

    [Fact]
    public async Task ToolFormatIntakeSummary_EmergencyChestPain_TriggersEmergencyUrgencyAndRedFlags()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new IntakeAgentService(context);

        var rawInput = "I have acute chest pain and shortness of breath since yesterday, prefer Dr. Perera tomorrow morning";

        var (success, errorMessage, json) = await service.ToolFormatIntakeSummaryAsync(rawInput);

        Assert.True(success);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("isEmergency").GetBoolean());
        Assert.Equal("Emergency", doc.RootElement.GetProperty("urgencyLevel").GetString());
        Assert.Equal("Dr. Perera", doc.RootElement.GetProperty("preferredDoctor").GetString());

        var symptoms = doc.RootElement.GetProperty("symptoms");
        Assert.True(symptoms.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task ToolFormatIntakeSummary_MildSkinRash_ExtractsLowUrgencyAndDermatology()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new IntakeAgentService(context);

        var rawInput = "I have a mild rash on my arm for 3 days, would like to see a dermatologist in the evening";

        var (success, errorMessage, json) = await service.ToolFormatIntakeSummaryAsync(rawInput);

        Assert.True(success);
        using var doc = JsonDocument.Parse(json);
        Assert.False(doc.RootElement.GetProperty("isEmergency").GetBoolean());
        Assert.Equal("Low", doc.RootElement.GetProperty("urgencyLevel").GetString());
        Assert.Equal("Dermatology", doc.RootElement.GetProperty("preferredSpecialty").GetString());
    }

    [Fact]
    public async Task ProcessIntakeAsync_FullWorkflow_CreatesAuditLogsAndReturnsStructuredContract()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = await CreateSamplePatientAsync(context);
        var service = new IntakeAgentService(context);

        var request = new IntakeProcessRequestDto
        {
            PatientId = patient.Id,
            RawText = "Severe headache and dizziness since yesterday morning. Prefer a neurologist."
        };

        var (success, errorMessage, response) = await service.ProcessIntakeAsync(request);

        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(response);

        // Verify Output Contract
        Assert.Equal(patient.Id, response.PatientId);
        Assert.Equal(patient.Name, response.PatientMetadata.Name);
        Assert.Equal(patient.NIC, response.PatientMetadata.NIC);
        Assert.NotEmpty(response.Symptoms);
        Assert.NotEmpty(response.PreferredTimeWindows);
        Assert.Equal("Neurology", response.DoctorPreferences.PreferredSpecialty);
        Assert.NotEmpty(response.ExecutionPlan);

        // Verify Durable Logging in IntakeAgentLogs
        var logs = context.IntakeAgentLogs.Where(l => l.PatientId == patient.Id).ToList();
        Assert.Equal(3, logs.Count); // ValidateEligibility, GetPatientHistory, FormatIntakeSummary
        Assert.Contains(logs, l => l.ToolCalled == "ValidatePatientEligibility");
        Assert.Contains(logs, l => l.ToolCalled == "GetPatientHistory");
        Assert.Contains(logs, l => l.ToolCalled == "FormatIntakeSummary");
    }
}
