using ChannelCenter.API.DTOs.Triage;
using ChannelCenter.API.Models;
using ChannelCenter.API.Services.Triage;
using ChannelCenter.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChannelCenter.Tests.Triage;

public class TriageServiceTests
{
    [Fact]
    public async Task SubmitQuestionnaireAsync_SavesQuestionnaireAndReturnsDto()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);

        var dto = new CreateQuestionnaireDto
        {
            PatientId = 10,
            ResponsesData = "{\"allergies\":[\"Penicillin\"],\"medicalHistory\":[\"Hypertension\"]}"
        };

        var result = await service.SubmitQuestionnaireAsync(dto);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(10, result.PatientId);
        Assert.Contains("Penicillin", result.ResponsesData);
    }

    [Fact]
    public async Task ProcessTriageAsync_EmergencyChestPain_AssignsEmergencyLevelAndCardiologySpecialty()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);

        var dto = new ProcessTriageDto
        {
            AppointmentId = 42,
            RawSymptoms = "Acute chest pain with shortness of breath and left arm numbness",
            SymptomList = new List<SymptomItemDto>
            {
                new SymptomItemDto { SymptomKeyword = "chest pain", SeverityRating = 9, DurationInDays = 1 },
                new SymptomItemDto { SymptomKeyword = "shortness of breath", SeverityRating = 8, DurationInDays = 1 }
            }
        };

        var result = await service.ProcessTriageAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(42, result.AppointmentId);
        Assert.Equal(UrgencyLevel.Emergency, result.UrgencyLevel);
        Assert.True(result.UrgencyScore >= 80);
        Assert.Equal("Cardiology", result.RecommendedSpecialty);
        Assert.Contains("Chest pain", result.ReasoningTrace, StringComparison.OrdinalIgnoreCase);

        // Verify referral generated
        var referral = await context.Referrals.FirstOrDefaultAsync(r => r.TriageId == result.Id);
        Assert.NotNull(referral);
        Assert.Equal("Cardiology", referral.TargetSpecialty);
        Assert.Equal(ReferralStatus.Generated, referral.Status);
    }

    [Fact]
    public async Task ProcessTriageAsync_SkinRash_AssignsLowMediumAndDermatologySpecialty()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new TriageService(context);

        var dto = new ProcessTriageDto
        {
            AppointmentId = 43,
            RawSymptoms = "Mild red skin rash on forearm with minor itching",
            SymptomList = new List<SymptomItemDto>
            {
                new SymptomItemDto { SymptomKeyword = "skin rash", SeverityRating = 3, DurationInDays = 4 }
            }
        };

        var result = await service.ProcessTriageAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(43, result.AppointmentId);
        Assert.Equal(UrgencyLevel.Medium, result.UrgencyLevel);
        Assert.Equal("Dermatology", result.RecommendedSpecialty);
    }

    [Fact]
    public async Task UpdateReferralStatusAsync_UpdatesStatusSuccessfully()
    {
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var assessment = new TriageAssessment { Id = 1, AppointmentId = 42, UrgencyScore = 50, UrgencyLevel = UrgencyLevel.Medium, RawSymptoms = "Fever", RecommendedSpecialty = "General Medicine" };
        var referral = new Referral { Id = 1, TriageId = 1, TargetSpecialty = "General Medicine", Status = ReferralStatus.Generated };

        context.TriageAssessments.Add(assessment);
        context.Referrals.Add(referral);
        await context.SaveChangesAsync();

        var service = new TriageService(context);
        var updated = await service.UpdateReferralStatusAsync(1, new UpdateReferralStatusDto { Status = ReferralStatus.Reviewed });

        Assert.NotNull(updated);
        Assert.Equal(ReferralStatus.Reviewed, updated.Status);

        var dbReferral = await context.Referrals.FindAsync(1);
        Assert.Equal(ReferralStatus.Reviewed, dbReferral?.Status);
    }
}
