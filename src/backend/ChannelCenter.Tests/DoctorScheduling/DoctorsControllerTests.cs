using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ChannelCenter.API.Controllers.DoctorScheduling;
using ChannelCenter.API.Data;
using ChannelCenter.API.DTOs.DoctorScheduling;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.DoctorScheduling;

namespace ChannelCenter.Tests.DoctorScheduling;

public class DoctorsControllerTests
{
    private DoctorSchedulingService CreateService(ApplicationDbContext context)
    {
        return new DoctorSchedulingService(context);
    }

    [Fact(DisplayName = "CASE 1 — GetDoctors: returns list of registered doctors with specialty and user names")]
    public async Task GetDoctors_ReturnsOkWithDoctorList()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user = new User { Id = 101, FullName = "Dr. House", Email = "house@test.com", Role = UserRole.Doctor };
        var spec = new Specialty { Id = 10, Name = "Diagnostics" };
        context.Users.Add(user);
        context.Specialties.Add(spec);
        await context.SaveChangesAsync();

        var doc = new Doctor { Id = 1, UserId = 101, SpecialtyId = 10, Qualifications = "MD, Board Certified" };
        context.Doctors.Add(doc);
        await context.SaveChangesAsync();

        var controller = new DoctorsController(CreateService(context));

        var result = await controller.GetDoctors();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var doctors = Assert.IsAssignableFrom<IEnumerable<DoctorDto>>(okResult.Value);
        Assert.Single(doctors);
        Assert.Equal("Dr. House", doctors.First().DoctorName);
        Assert.Equal("Diagnostics", doctors.First().SpecialtyName);
    }

    [Fact(DisplayName = "CASE 2 — CreateDoctor: valid payload creates Doctor profile and user account")]
    public async Task CreateDoctor_ValidDto_ReturnsCreatedAtAction()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var spec = new Specialty { Id = 5, Name = "Neurology" };
        context.Specialties.Add(spec);
        await context.SaveChangesAsync();

        var dto = new CreateDoctorDto
        {
            DoctorName = "Michael Chen",
            Email = "mchen@hospital.com",
            Password = "DocPassword123!",
            SpecialtyId = 5,
            Qualifications = "MBBS, FRCS"
        };

        var controller = new DoctorsController(CreateService(context));

        var result = await controller.CreateDoctor(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdDoctor = Assert.IsType<DoctorDto>(createdResult.Value);
        Assert.Equal("Dr. Michael Chen", createdDoctor.DoctorName);
        Assert.Equal("Neurology", createdDoctor.SpecialtyName);
    }

    [Fact(DisplayName = "CASE 3 — GetDoctorById: returns 404 for unknown doctor ID")]
    public async Task GetDoctorById_UnknownId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new DoctorsController(CreateService(context));

        var result = await controller.GetDoctorById(9999);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFoundResult.Value);
    }
}
