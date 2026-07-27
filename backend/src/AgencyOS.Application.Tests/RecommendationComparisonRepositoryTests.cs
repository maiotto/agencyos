using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class RecommendationComparisonRepositoryTests
{
    [Fact]
    public async Task GetVersionsByRecommendationNumber_ReturnsVersionCreatedOnly()
    {
        await using var context = CreateContext();
        var v1 = await SeedRecommendationAsync(context, "REC-CMP", 1);
        var v2 = v1.CreateNewVersion(
            "v2", null, null, 90m, 1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null, "{}", "{}", "{\"v\":2}", "planner", DateTimeOffset.UtcNow);
        context.Recommendations.Add(v2);
        await context.SaveChangesAsync();

        var repository = new RecommendationHistoryRepository(context);
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v1, RecommendationHistoryEventType.VersionCreated, "engine", DateTimeOffset.UtcNow.AddMinutes(-2)));
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v2, RecommendationHistoryEventType.VersionCreated, "planner", DateTimeOffset.UtcNow.AddMinutes(-1)));
        await repository.AddAsync(RecommendationHistory.FromRecommendation(
            v2, RecommendationHistoryEventType.WorkflowTransition, "planner", DateTimeOffset.UtcNow,
            RecommendationWorkflowStatus.Draft, Guid.NewGuid()));

        var versions = await repository.GetVersionsByRecommendationNumberAsync("REC-CMP");
        var snapshot = await repository.GetVersionSnapshotByRecommendationIdAsync(v2.Id);

        Assert.Equal(2, versions.Count);
        Assert.NotNull(snapshot);
        Assert.Equal(v2.Id, snapshot!.RecommendationId);
        Assert.Equal(RecommendationHistoryEventType.VersionCreated, snapshot.EventType);
    }

    private static async Task<Recommendation> SeedRecommendationAsync(
        ApplicationDbContext context,
        string number,
        int version)
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            number,
            "Human + AI",
            "Summary",
            "Reason",
            80m,
            1,
            version,
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
