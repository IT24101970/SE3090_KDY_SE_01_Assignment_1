using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers;
using ChannelCenter.API.DTOs.Common;
using ChannelCenter.API.DTOs.Patient;
using ChannelCenter.API.Models;
using Xunit;

namespace ChannelCenter.Tests.Patients;

public class PatientsControllerTests
{
    [Fact]
    public async Task GetPatients_ReturnsOk_WithPagedResult()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Patients.Add(new Patient
        {
            Name = "Alice Silva",
            NIC = "920001111V",
            Email = "alice@example.com",
            PhoneNumber = "0770001111",
            Gender = "Female",
            DateOfBirth = DateTime.UtcNow.AddYears(-30)
        });
        await context.SaveChangesAsync();

        var controller = new PatientsController(context);

        var result = await controller.GetPatients(new PatientFilterDto());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var paged = Assert.IsType<PagedResult<PatientResponseDto>>(okResult.Value);
        Assert.Single(paged.Items);
    }

    [Fact]
    public async Task GetPatientById_ReturnsNotFound_WhenDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new PatientsController(context);

        var result = await controller.GetPatientById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreatePatient_ReturnsCreatedAtAction_WhenValid()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new PatientsController(context);

        var dto = new CreatePatientDto
        {
            Name = "Bob Builder",
            NIC = "881234567V",
            Email = "bob@example.com",
            PhoneNumber = "0712345678",
            EmergencyContact = "0719998877",
            Gender = "Male",
            DateOfBirth = new DateTime(1988, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await controller.CreatePatient(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<PatientResponseDto>(created.Value);
        Assert.Equal("Bob Builder", response.Name);
    }
}
