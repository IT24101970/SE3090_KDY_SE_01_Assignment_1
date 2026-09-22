using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers;
using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.Models;
using Xunit;

namespace ChannelCenter.Tests.Appointments;

public class AppointmentsControllerTests
{
    [Fact]
    public async Task GetAppointments_ReturnsOk_WithAppointmentsList()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(context);

        var result = await controller.GetAppointments(new AppointmentFilterDto());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsNotFound_WhenDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(context);

        var result = await controller.GetAppointmentById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelAppointment_ReturnsNotFound_WhenAppointmentMissing()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(context);

        var result = await controller.CancelAppointment(999, new CancelAppointmentDto
        {
            CancelReason = "Test cancel"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
