using ChannelCenter.API.Controllers.Triage;
using ChannelCenter.API.DTOs.Triage;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Triage;
using ChannelCenter.Tests;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ChannelCenter.Tests.Triage;

public class TriageControllerTests
{
    [Fact]
    public async Task SubmitQuestionnaire_ReturnsCreatedResult()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);
        var controller = new TriageController(service);

        var dto = new CreateQuestionnaireDto { PatientId = 5, ResponsesData = "{\"history\":\"None\"}" };
        var actionResult = await controller.SubmitQuestionnaire(dto);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var response = Assert.IsType<QuestionnaireResponseDto>(created.Value);
        Assert.Equal(5, response.PatientId);
    }

    [Fact]
    public async Task ProcessTriage_ReturnsCreatedTriageAssessment()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);
        var controller = new TriageController(service);

        var dto = new ProcessTriageDto
        {
            AppointmentId = 8,
            RawSymptoms = "Dizziness and severe migraine headache",
            SymptomList = new List<SymptomItemDto>
            {
                new SymptomItemDto { SymptomKeyword = "migraine", SeverityRating = 8, DurationInDays = 2 }
            }
        };

        var actionResult = await controller.ProcessTriage(dto);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var assessment = Assert.IsType<TriageAssessmentDto>(created.Value);
        Assert.Equal(8, assessment.AppointmentId);
        Assert.Equal(UrgencyLevel.Emergency, assessment.UrgencyLevel);
        Assert.Equal("Neurology", assessment.RecommendedSpecialty);
    }

    [Fact]
    public async Task GetSpecialties_ReturnsListOfSpecialtyNames()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);
        var controller = new TriageController(service);

        var actionResult = await controller.GetSpecialties();
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var specialties = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);

        Assert.Contains("Cardiology", specialties);
        Assert.Contains("Neurology", specialties);
    }

    [Fact]
    public async Task UpdateReferralStatus_ReturnsUpdatedReferral()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var referral = new Referral { Id = 1, TriageId = 1, TargetSpecialty = "Cardiology", Status = ReferralStatus.Generated };
        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var service = new TriageService(context);
        var controller = new TriageController(service);

        var actionResult = await controller.UpdateReferralStatus(1, new UpdateReferralStatusDto { Status = ReferralStatus.Assigned });
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<ReferralDto>(okResult.Value);

        Assert.Equal(ReferralStatus.Assigned, dto.Status);
    }
}
