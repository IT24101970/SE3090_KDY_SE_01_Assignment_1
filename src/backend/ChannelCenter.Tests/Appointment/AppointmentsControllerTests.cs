using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChannelCenter.API.Controllers.Appointment;
using ChannelCenter.API.DTOs.Appointment;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Appointment;
using ChannelCenter.API.Services.Patient;
using Xunit;

namespace ChannelCenter.Tests.Appointments;

public class AppointmentsControllerTests
{
    private static ClaimsPrincipal CreateUserPrincipal(int userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"User_{userId}"),
            new Claim(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    [Fact]
    public async Task GetAppointments_ReturnsOk_WithAppointmentsList()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(new AppointmentService(context));

        var result = await controller.GetAppointments(new AppointmentFilterDto());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsNotFound_WhenDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(new AppointmentService(context));

        var result = await controller.GetAppointmentById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CancelAppointment_ReturnsNotFound_WhenAppointmentMissing()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(new AppointmentService(context));

        var result = await controller.CancelAppointment(999, new CancelAppointmentDto
        {
            CancelReason = "Test cancel"
        });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact(DisplayName = "CASE 2 — Unauthenticated user: appointment creation rejected with 401 Unauthorized")]
    public async Task CreateAppointment_UnauthenticatedUser_ReturnsUnauthorized()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new AppointmentsController(new AppointmentService(context), new PatientService(context));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user identity claims
        };

        var result = await controller.CreateAppointment(new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            ScheduleId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            ReasonForVisit = "Headache"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact(DisplayName = "CASE 3 — Unregistered patient: user without patient profile rejected with BadRequest")]
    public async Task CreateAppointment_UnregisteredPatientUser_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var user = new User { Id = 10, FullName = "No Profile User", Email = "noprofile@test.com", Role = UserRole.Patient };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(new AppointmentService(context), new PatientService(context));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateUserPrincipal(10, "Patient")
            }
        };

        var result = await controller.CreateAppointment(new CreateAppointmentDto
        {
            PatientId = 10,
            DoctorId = 1,
            ScheduleId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            ReasonForVisit = "I need a checkup"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact(DisplayName = "CASE 12 — Patient A tries to use Patient B's ID: rejected with Forbid")]
    public async Task CreateAppointment_PatientMismatch_ReturnsForbid()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var userA = new User { Id = 1, FullName = "Patient A", Email = "a@test.com", Role = UserRole.Patient };
        var userB = new User { Id = 2, FullName = "Patient B", Email = "b@test.com", Role = UserRole.Patient };
        context.Users.AddRange(userA, userB);

        var patientA = new Patient { Id = 1, UserId = 1, Name = "Patient A", NIC = "111111111V", PhoneNumber = "0771111111" };
        var patientB = new Patient { Id = 2, UserId = 2, Name = "Patient B", NIC = "222222222V", PhoneNumber = "0772222222" };
        context.Patients.AddRange(patientA, patientB);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(new AppointmentService(context), new PatientService(context));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateUserPrincipal(1, "Patient") // Logged in as User 1 (Patient A)
            }
        };

        // Patient A tries to book for Patient B (ID 2)
        var result = await controller.CreateAppointment(new CreateAppointmentDto
        {
            PatientId = 2,
            DoctorId = 1,
            ScheduleId = 1,
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            ReasonForVisit = "I have a headache"
        });

        Assert.IsType<ForbidResult>(result);
    }
}
