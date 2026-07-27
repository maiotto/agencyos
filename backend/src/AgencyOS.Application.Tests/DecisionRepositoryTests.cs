using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class DecisionRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsWithTimeline()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new DecisionRepository(context);
        var decision = Decision.Create(
            recommendation.Id,
            recommendation.CompanyId,
            recommendation.MissionId,
            recommendation.ContractId,
            "planner",
            DateTimeOffset.UtcNow);

        await repository.AddAsync(decision);
        var loaded = await repository.GetByIdAsync(decision.Id);

        Assert.NotNull(loaded);
        Assert.Equal(decision.RecommendationId, loaded!.RecommendationId);
        Assert.Single(loaded.Timeline);
    }

    [Fact]
    public async Task Update_AppendsTimelineEntries()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new DecisionRepository(context);
        var decision = Decision.Create(
            recommendation.Id,
            recommendation.CompanyId,
            recommendation.MissionId,
            recommendation.ContractId,
            "planner",
            DateTimeOffset.UtcNow);
        await repository.AddAsync(decision);

        decision.StartImplementation("planner", "start", DateTimeOffset.UtcNow);
        await repository.UpdateAsync(decision);

        var loaded = await repository.GetByIdAsync(decision.Id);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Timeline.Count);
        Assert.Equal(DecisionStatus.InProgress, loaded.DecisionStatus);
        Assert.Equal(DecisionImplementationStatus.InProgress, loaded.ImplementationStatus);
    }

    [Fact]
    public async Task ExistsByRecommendationId_AndFilterByStatus()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new DecisionRepository(context);
        var decision = Decision.Create(
            recommendation.Id,
            recommendation.CompanyId,
            recommendation.MissionId,
            recommendation.ContractId,
            "planner",
            DateTimeOffset.UtcNow);
        await repository.AddAsync(decision);

        Assert.True(await repository.ExistsByRecommendationIdAsync(recommendation.Id));
        var filtered = await repository.GetAllAsync(new DecisionQueryParameters
        {
            DecisionStatus = DecisionStatus.Created,
            CompanyId = recommendation.CompanyId
        });
        Assert.Single(filtered);
    }

    private static async Task<Recommendation> SeedRecommendationAsync(ApplicationDbContext context)
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-DEC-REPO",
            "Human + AI",
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
