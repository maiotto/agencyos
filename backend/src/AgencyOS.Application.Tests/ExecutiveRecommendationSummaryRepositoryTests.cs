using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class ExecutiveRecommendationSummaryRepositoryTests
{
    [Fact]
    public async Task AddGetAndVersioning_Work()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new ExecutiveRecommendationSummaryRepository(context);
        var generator = new ExecutiveRecommendationSummaryGenerationService();

        var v1 = generator.Generate(recommendation, null, null, null, 1, "exec", DateTimeOffset.UtcNow);
        await repository.AddAsync(v1);

        var next = await repository.GetNextSummaryVersionAsync(recommendation.Id);
        var v2 = generator.Generate(
            recommendation,
            null,
            null,
            null,
            next,
            "exec",
            DateTimeOffset.UtcNow.AddMinutes(1));
        await repository.AddAsync(v2);

        var byRecommendation = await repository.GetByRecommendationIdAsync(recommendation.Id);
        var loaded = await repository.GetByIdAsync(v1.Id);

        Assert.Equal(2, next);
        Assert.Equal(2, byRecommendation.Count);
        Assert.NotNull(loaded);
    }

    [Fact]
    public async Task Query_IncludesArchivedByDefault()
    {
        await using var context = CreateContext();
        var recommendation = await SeedRecommendationAsync(context);
        var repository = new ExecutiveRecommendationSummaryRepository(context);
        var generator = new ExecutiveRecommendationSummaryGenerationService();
        var item = generator.Generate(recommendation, null, null, null, 1, "exec", DateTimeOffset.UtcNow);
        await repository.AddAsync(item);
        item.Archive(DateTimeOffset.UtcNow);
        await repository.UpdateAsync(item);

        var results = await repository.QueryAsync(new ExecutiveRecommendationSummaryQueryParameters
        {
            RecommendationId = recommendation.Id,
            IncludeArchived = true
        });

        Assert.Single(results);
        Assert.True(results[0].Archived);
    }

    private static async Task<Recommendation> SeedRecommendationAsync(ApplicationDbContext context)
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-EXEC-REPO",
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
