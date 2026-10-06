using System;
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

public class DoctorLeavesControllerTests
{
    private DoctorSchedulingService CreateService(ApplicationDbContext context)
    {
        return new DoctorSchedulingService(context);
    }

    [Fact(DisplayName = "CASE 1 — GetLeaves: returns registered doctor leave requests")]
    public async Task GetLeaves_ReturnsOkWithLeaves()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user = new User { Id = 201, FullName = "Dr. Strange", Email = "strange@test.com", Role = UserRole.Doctor };
        var spec = new Specialty { Id = 1, Name = "Surgery" };
        context.Users.Add(user);
        context.Specialties.Add(spec);
        await context.SaveChangesAsync();

        var doc = new Doctor { Id = 10, UserId = 201, SpecialtyId = 1 };
        context.Doctors.Add(doc);
        await context.SaveChangesAsync();

        var leave = new DoctorLeave
        {
            Id = 1,
            DoctorId = 10,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            Reason = "Medical Conference"
        };
        context.DoctorLeaves.Add(leave);
        await context.SaveChangesAsync();

        var controller = new DoctorLeavesController(CreateService(context));

        var result = await controller.GetLeaves(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var leaves = Assert.IsAssignableFrom<IEnumerable<DoctorLeaveDto>>(okResult.Value);
        Assert.Single(leaves);
        Assert.Equal("Medical Conference", leaves.First().Reason);
    }

    [Fact(DisplayName = "CASE 2 — CreateLeave: valid leave request creates record successfully")]
    public async Task CreateLeave_ValidDto_ReturnsCreatedAtAction()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var doc = new Doctor { Id = 12, UserId = 1, SpecialtyId = 1 };
        context.Doctors.Add(doc);
        await context.SaveChangesAsync();

        var dto = new CreateDoctorLeaveDto
        {
            DoctorId = 12,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(7),
            Reason = "Annual Leave"
        };

        var controller = new DoctorLeavesController(CreateService(context));

        var result = await controller.CreateLeave(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdLeave = Assert.IsType<DoctorLeaveDto>(createdResult.Value);
        Assert.Equal(12, createdLeave.DoctorId);
        Assert.Equal("Annual Leave", createdLeave.Reason);
    }
}
