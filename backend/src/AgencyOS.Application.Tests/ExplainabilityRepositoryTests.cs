using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class ExplainabilityRepositoryTests
{
    [Fact]
    public async Task AddGetAndVersioning_Work()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new ExplainabilityRepository(context);
        var generator = new ExplainabilityGenerationService();

        var v1 = generator.GenerateForRecommendation(recommendation, 1, "planner", DateTimeOffset.UtcNow);
        await repository.AddAsync(v1);

        var next = await repository.GetNextGenerationVersionAsync(recommendation.Id);
        var v2 = generator.GenerateForRecommendation(
            recommendation,
            next,
            "planner",
            DateTimeOffset.UtcNow.AddMinutes(1));
        await repository.AddAsync(v2);

        var byRecommendation = await repository.GetByRecommendationIdAsync(recommendation.Id);
        var loaded = await repository.GetByIdAsync(v1.Id);

        Assert.Equal(2, next);
        Assert.Equal(2, byRecommendation.Count);
        Assert.NotNull(loaded);
        Assert.Equal(v1.RecommendationId, loaded!.RecommendationId);
    }

    [Fact]
    public async Task Query_FiltersByTypeAndStatus()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new ExplainabilityRepository(context);
        var generator = new ExplainabilityGenerationService();
        var item = generator.GenerateForRecommendation(recommendation, 1, "planner", DateTimeOffset.UtcNow);
        await repository.AddAsync(item);

        var results = await repository.QueryAsync(new ExplainabilityQueryParameters
        {
            Status = ExplainabilityStatus.Active,
            ExplanationType = ExplainabilityTypes.Recommendation,
            Search = "explanation"
        });

        Assert.Single(results);
    }

    private static async Task<Recommendation> SeedRecommendationAsync(ApplicationDbContext context)
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-EXP-REPO",
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
