using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class RecommendationRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTrips()
    {
        await using var context = CreateContext();
        var repository = new RecommendationRepository(context);
        var recommendation = CreateRecommendation("Alpha Mix");

        await repository.AddAsync(recommendation);
        var loaded = await repository.GetByIdAsync(recommendation.Id);

        Assert.NotNull(loaded);
        Assert.Equal(recommendation.RecommendationNumber, loaded!.RecommendationNumber);
        Assert.Equal(1, loaded.Version);
    }

    [Fact]
    public async Task Query_FiltersBySearchAndExcludesArchivedByDefault()
    {
        await using var context = CreateContext();
        var repository = new RecommendationRepository(context);

        var active = CreateRecommendation("Alpha Mix", "REC-ALPHA-1");
        var archived = CreateRecommendation("Beta Mix", "REC-BETA-1");
        archived.Archive(DateTimeOffset.UtcNow);

        await repository.AddAsync(active);
        await repository.AddAsync(archived);

        var results = await repository.QueryAsync(new RecommendationQueryParameters
        {
            Search = "Alpha"
        });

        Assert.Single(results);
        Assert.Equal(active.Id, results[0].Id);
    }

    [Fact]
    public async Task Update_PersistsArchiveFlag()
    {
        await using var context = CreateContext();
        var repository = new RecommendationRepository(context);
        var recommendation = CreateRecommendation();
        await repository.AddAsync(recommendation);

        recommendation.Archive(DateTimeOffset.UtcNow);
        await repository.UpdateAsync(recommendation);

        context.ChangeTracker.Clear();
        var loaded = await repository.GetByIdAsync(recommendation.Id);
        Assert.True(loaded!.Archived);
        Assert.Equal(RecommendationStatus.Archived, loaded.Status);
    }

    [Fact]
    public async Task GetVersionsByNumber_ReturnsOrderedVersions()
    {
        await using var context = CreateContext();
        var repository = new RecommendationRepository(context);
        var v1 = CreateRecommendation(number: "REC-LINEAGE-1");
        var v2 = v1.CreateNewVersion(
            null, null, null, 90m, 1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null, "{}", "{}", "{\"v\":2}", "planner", DateTimeOffset.UtcNow);

        await repository.AddAsync(v1);
        await repository.AddAsync(v2);

        var versions = await repository.GetVersionsByNumberAsync("REC-LINEAGE-1");
        Assert.Equal(2, versions.Count);
        Assert.Equal(1, versions[0].Version);
        Assert.Equal(2, versions[1].Version);
    }

    private static Recommendation CreateRecommendation(
        string title = "Human + AI",
        string number = "REC-TEST-001") =>
        Recommendation.Create(
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

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
