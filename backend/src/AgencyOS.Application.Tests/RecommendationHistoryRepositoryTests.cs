using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class RecommendationHistoryRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTrips()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new RecommendationHistoryRepository(context);
        var history = RecommendationHistory.FromRecommendation(
            recommendation,
            RecommendationHistoryEventType.VersionCreated,
            "decision-engine",
            DateTimeOffset.UtcNow);

        await repository.AddAsync(history);
        var loaded = await repository.GetByIdAsync(history.Id);

        Assert.NotNull(loaded);
        Assert.Equal(history.RecommendationId, loaded!.RecommendationId);
        Assert.Equal(RecommendationHistoryEventType.VersionCreated, loaded.EventType);
    }

    [Fact]
    public async Task Query_FiltersByCompanyAndSearch()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context, "Alpha Mix");
        var other = await SeedRecommendationAsync(context, "Beta Mix", "REC-OTHER");
        var repository = new RecommendationHistoryRepository(context);

        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            recommendation,
            RecommendationHistoryEventType.VersionCreated,
            "decision-engine",
            DateTimeOffset.UtcNow));
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            other,
            RecommendationHistoryEventType.VersionCreated,
            "decision-engine",
            DateTimeOffset.UtcNow));

        var results = await repository.QueryAsync(new RecommendationHistoryQueryParameters
        {
            CompanyId = recommendation.CompanyId,
            Search = "Alpha"
        });

        Assert.Single(results);
        Assert.Equal(recommendation.Id, results[0].RecommendationId);
    }

    [Fact]
    public async Task GetVersionsAndTimeline_ReturnLineageEvents()
    {
        await using var context = CreateContext();
        var v1 = await SeedRecommendationAsync(context, number: "REC-LINE");
        var v2 = v1.CreateNewVersion(
            "v2", null, null, 90m, 1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null, "{}", "{}", "{\"v\":2}", "planner", DateTimeOffset.UtcNow);
        context.Recommendations.Add(v2);
        await context.SaveChangesAsync();

        var repository = new RecommendationHistoryRepository(context);
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v1, RecommendationHistoryEventType.VersionCreated, "decision-engine", DateTimeOffset.UtcNow.AddMinutes(-2)));
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v2, RecommendationHistoryEventType.VersionCreated, "planner", DateTimeOffset.UtcNow.AddMinutes(-1)));
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v2, RecommendationHistoryEventType.WorkflowTransition, "planner", DateTimeOffset.UtcNow,
            RecommendationWorkflowStatus.Draft, Guid.NewGuid()));

        var versions = await repository.GetVersionsByRecommendationIdAsync(v1.Id);
        var timeline = await repository.GetTimelineByRecommendationIdAsync(v2.Id);

        Assert.Equal(2, versions.Count);
        Assert.Equal(3, timeline.Count);
    }

    private static async Task<Recommendation> SeedRecommendationAsync(
        ApplicationDbContext context,
        string title = "Human + AI",
        string number = "REC-HIST-1")
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            number,
            title,
            "Summary",
            "Reason",
            80m,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);

        context.Recommendations.Add(recommendation);
        await context.SaveChangesAsync();
        return recommendation;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
