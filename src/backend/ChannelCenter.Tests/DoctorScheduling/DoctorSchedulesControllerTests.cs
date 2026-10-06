using System;
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

public class DoctorSchedulesControllerTests
{
    private DoctorSchedulingService CreateService(ApplicationDbContext context)
    {
        return new DoctorSchedulingService(context);
    }

    [Fact(DisplayName = "CASE 1 — GetSchedules: returns list of active doctor schedules")]
    public async Task GetSchedules_ReturnsOkResultWithSchedules()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        
        var user = new User { Id = 10, FullName = "Dr. Sarah Connor", Email = "sarah@test.com", Role = UserRole.Doctor };
        var spec = new Specialty { Id = 1, Name = "Cardiology" };
        context.Users.Add(user);
        context.Specialties.Add(spec);
        await context.SaveChangesAsync();

        var doc = new Doctor { Id = 1, UserId = 10, SpecialtyId = 1, Qualifications = "MBBS, MD" };
        var room = new ConsultationRoom { Id = 1, RoomName = "Room 101", Floor = "1st Floor" };
        context.Doctors.Add(doc);
        context.ConsultationRooms.Add(room);
        await context.SaveChangesAsync();

        var sched = new DoctorSchedule
        {
            Id = 100,
            DoctorId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            MaxPatients = 10
        };
        context.DoctorSchedules.Add(sched);
        await context.SaveChangesAsync();

        var controller = new DoctorSchedulesController(CreateService(context));

        var result = await controller.GetSchedules(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var schedules = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<DoctorScheduleDto>>(okResult.Value);
        Assert.Single(schedules);
        Assert.Equal(100, schedules.First().Id);
        Assert.Equal("Dr. Sarah Connor", schedules.First().DoctorName);
    }

    [Fact(DisplayName = "CASE 2 — CreateSchedule: valid payload returns CreatedAtAction (201)")]
    public async Task CreateSchedule_ValidDto_ReturnsCreatedAtAction()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user = new User { Id = 20, FullName = "Dr. Alan Grant", Email = "alan@test.com", Role = UserRole.Doctor };
        var spec = new Specialty { Id = 2, Name = "Orthopedics" };
        context.Users.Add(user);
        context.Specialties.Add(spec);
        await context.SaveChangesAsync();

        var doc = new Doctor { Id = 2, UserId = 20, SpecialtyId = 2 };
        var room = new ConsultationRoom { Id = 2, RoomName = "Room 202", Floor = "2nd Floor" };
        context.Doctors.Add(doc);
        context.ConsultationRooms.Add(room);
        await context.SaveChangesAsync();

        var dto = new CreateDoctorScheduleDto
        {
            DoctorId = 2,
            RoomId = 2,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxPatients = 15
        };

        var controller = new DoctorSchedulesController(CreateService(context));

        var result = await controller.CreateSchedule(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdSchedule = Assert.IsType<DoctorScheduleDto>(createdResult.Value);
        Assert.Equal(2, createdSchedule.DoctorId);
        Assert.Equal(2, createdSchedule.RoomId);
        Assert.Equal(15, createdSchedule.MaxPatients);
    }

    [Fact(DisplayName = "CASE 3 — CreateSchedule: conflicting room booking returns 409 Conflict")]
    public async Task CreateSchedule_RoomConflict_ReturnsConflict()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var user1 = new User { Id = 31, FullName = "Dr. Doc One", Email = "doc1@test.com", Role = UserRole.Doctor };
        var user2 = new User { Id = 32, FullName = "Dr. Doc Two", Email = "doc2@test.com", Role = UserRole.Doctor };
        context.Users.AddRange(user1, user2);

        var doc1 = new Doctor { Id = 31, UserId = 31, SpecialtyId = 1 };
        var doc2 = new Doctor { Id = 32, UserId = 32, SpecialtyId = 1 };
        var room = new ConsultationRoom { Id = 3, RoomName = "Shared Room 303", Floor = "3rd Floor" };
        context.Doctors.AddRange(doc1, doc2);
        context.ConsultationRooms.Add(room);
        await context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2);
        var endTime = startTime.AddHours(2);

        // Doctor 1 already booked room 3 during this time
        context.DoctorSchedules.Add(new DoctorSchedule
        {
            Id = 301, DoctorId = 31, RoomId = 3, StartTime = startTime, EndTime = endTime, MaxPatients = 10
        });
        await context.SaveChangesAsync();

        var controller = new DoctorSchedulesController(CreateService(context));

        // Doctor 2 attempts to book room 3 at overlapping time
        var conflictingDto = new CreateDoctorScheduleDto
        {
            DoctorId = 32,
            RoomId = 3,
            StartTime = startTime.AddMinutes(30),
            EndTime = endTime.AddMinutes(30),
            MaxPatients = 10
        };

        var result = await controller.CreateSchedule(conflictingDto);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.NotNull(conflictResult.Value);
    }

    [Fact(DisplayName = "CASE 4 — GetScheduleById: nonexistent ID returns 404 NotFound")]
    public async Task GetScheduleById_NonexistentId_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var controller = new DoctorSchedulesController(CreateService(context));

        var result = await controller.GetScheduleById(99999);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact(DisplayName = "CASE 5 — DeleteSchedule: existing schedule deletes successfully returning 204 NoContent")]
    public async Task DeleteSchedule_ExistingSchedule_ReturnsNoContent()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        
        var doc = new Doctor { Id = 50, UserId = 50, SpecialtyId = 1 };
        var room = new ConsultationRoom { Id = 50, RoomName = "Room 50", Floor = "5th Floor" };
        context.Doctors.Add(doc);
        context.ConsultationRooms.Add(room);
        await context.SaveChangesAsync();

        var sched = new DoctorSchedule
        {
            Id = 500, DoctorId = 50, RoomId = 50, StartTime = DateTime.UtcNow.AddDays(5), EndTime = DateTime.UtcNow.AddDays(5).AddHours(2)
        };
        context.DoctorSchedules.Add(sched);
        await context.SaveChangesAsync();

        var controller = new DoctorSchedulesController(CreateService(context));

        var result = await controller.DeleteSchedule(500);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await context.DoctorSchedules.FindAsync(500));
    }
}
