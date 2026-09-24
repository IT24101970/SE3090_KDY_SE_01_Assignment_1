using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers;
using ChannelCenter.API.DTOs.IntakeAgent;
using ChannelCenter.API.Models;
using Xunit;

namespace ChannelCenter.Tests.IntakeAgent;

public class IntakeAgentControllerTests
{
    private static async Task<Patient> CreateSamplePatientAsync(ChannelCenter.API.Data.ApplicationDbContext context)
    {
        var patient = new Patient
        {
            Name = "Kumara Perera",
            NIC = "198012345678",
            PhoneNumber = "+94771122334",
            Email = "kumara@hospital.com",
            EmergencyContact = "+94719988776",
            Gender = "Male",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    [Fact]
    public async Task ProcessIntake_ReturnsOk_WithStructuredExecutionPlan()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = await CreateSamplePatientAsync(context);
        var controller = new IntakeAgentController(context);

        var request = new IntakeProcessRequestDto
        {
            PatientId = patient.Id,
            RawText = "I have a cough and mild fever for 2 days"
        };

        var result = await controller.ProcessIntake(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<IntakeStructuredResponseDto>(okResult.Value);
        Assert.Equal(patient.Id, response.PatientId);
        Assert.NotEmpty(response.ExecutionPlan);
    }

    [Fact]
    public async Task ToolGetPatientHistory_ReturnsJsonContent_WhenPatientExists()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var patient = await CreateSamplePatientAsync(context);
        var controller = new IntakeAgentController(context);

        var result = await controller.ToolGetPatientHistory(new ToolPatientIdRequest { PatientId = patient.Id });

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains(patient.Name, contentResult.Content!);
    }

    [Fact]
    public async Task ToolFormatSummary_ReturnsJsonContent_WithParsedKeywords()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new IntakeAgentController(context);

        var result = await controller.ToolFormatIntakeSummary(new ToolRawTextRequest
        {
            RawText = "Severe back pain since last week"
        });

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("back pain", contentResult.Content!);
    }
}
