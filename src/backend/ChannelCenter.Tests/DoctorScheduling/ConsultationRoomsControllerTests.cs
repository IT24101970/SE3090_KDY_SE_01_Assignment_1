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

public class ConsultationRoomsControllerTests
{
    private DoctorSchedulingService CreateService(ApplicationDbContext context)
    {
        return new DoctorSchedulingService(context);
    }

    [Fact(DisplayName = "CASE 1 — GetRooms: returns list of registered consultation rooms")]
    public async Task GetRooms_ReturnsOkWithRoomList()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var room1 = new ConsultationRoom { Id = 1, RoomName = "Room A-101", Floor = "1st Floor" };
        var room2 = new ConsultationRoom { Id = 2, RoomName = "Room B-202", Floor = "2nd Floor" };
        context.ConsultationRooms.AddRange(room1, room2);
        await context.SaveChangesAsync();

        var controller = new ConsultationRoomsController(CreateService(context));

        var result = await controller.GetRooms();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var rooms = Assert.IsAssignableFrom<IEnumerable<ConsultationRoomDto>>(okResult.Value);
        Assert.Equal(2, rooms.Count());
    }

    [Fact(DisplayName = "CASE 2 — CreateRoom: valid room payload creates room and returns CreatedAtAction")]
    public async Task CreateRoom_ValidDto_ReturnsCreatedAtAction()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var dto = new CreateConsultationRoomDto
        {
            RoomName = "Room C-305",
            Floor = "3rd Floor"
        };

        var controller = new ConsultationRoomsController(CreateService(context));

        var result = await controller.CreateRoom(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var room = Assert.IsType<ConsultationRoomDto>(createdResult.Value);
        Assert.Equal("Room C-305", room.RoomName);
        Assert.Equal("3rd Floor", room.Floor);
    }
}
