using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class CompanyDecisionProfileRepositoryTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public async Task AddAndGetById_RoundTripsProfile()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var profile = CreateProfile();

        await repository.AddAsync(profile);
        var loaded = await repository.GetByIdAsync(profile.Id);

        Assert.NotNull(loaded);
        Assert.Equal(profile.Name, loaded!.Name);
        Assert.Equal(profile.Code, loaded.Code);
    }

    [Fact]
    public async Task QueryAsync_ReturnsOnlyLatestVersionPerFamily()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var v1 = CreateProfile(code: "Family1");
        await repository.AddAsync(v1);

        var v2 = v1.CreateNewVersion(
            "Family1 v2",
            null,
            v1.PriorityWeights,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
        await repository.AddAsync(v2);

        var results = await repository.QueryAsync(new CompanyDecisionProfileQueryParameters { CompanyId = CompanyId });

        var family = Assert.Single(results, profile => profile.ProfileFamilyId == v1.ProfileFamilyId);
        Assert.Equal(2, family.Version);
        Assert.Equal("Family1 v2", family.Name);
    }

    [Fact]
    public async Task GetDefaultActiveAsync_ReturnsOnlyDefaultAndActive()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var defaultProfile = CreateProfile(code: "Default1", defaultProfile: true);
        var otherProfile = CreateProfile(code: "Other1");
        await repository.AddAsync(defaultProfile);
        await repository.AddAsync(otherProfile);

        var found = await repository.GetDefaultActiveAsync(CompanyId);

        Assert.NotNull(found);
        Assert.Equal(defaultProfile.Id, found!.Id);
    }

    [Fact]
    public async Task GetLatestByFamilyAsync_ReturnsHighestVersion()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var v1 = CreateProfile(code: "Family2");
        await repository.AddAsync(v1);
        var v2 = v1.CreateNewVersion(
            null,
            null,
            v1.PriorityWeights,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
        await repository.AddAsync(v2);

        var latest = await repository.GetLatestByFamilyAsync(v1.ProfileFamilyId);

        Assert.NotNull(latest);
        Assert.Equal(2, latest!.Version);
        Assert.Equal(v2.Id, latest.Id);
    }

    [Fact]
    public async Task ExistsActiveNameAsync_DetectsDuplicateAmongLatestVersions()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var profile = CreateProfile(code: "Family3", name: "Balanced Strategy");
        await repository.AddAsync(profile);

        var exists = await repository.ExistsActiveNameAsync(CompanyId, "balanced strategy");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsActiveNameAsync_ExcludesOwnFamily()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var profile = CreateProfile(code: "Family4", name: "Balanced Strategy");
        await repository.AddAsync(profile);

        var exists = await repository.ExistsActiveNameAsync(
            CompanyId, "Balanced Strategy", excludeProfileFamilyId: profile.ProfileFamilyId);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsCodeAsync_DetectsDuplicate()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var profile = CreateProfile(code: "UniqueCode5");
        await repository.AddAsync(profile);

        var exists = await repository.ExistsCodeAsync(CompanyId, "uniquecode5");

        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange()
    {
        await using var context = CreateContext();
        var repository = new CompanyDecisionProfileRepository(context);
        var profile = CreateProfile(code: "Family6");
        await repository.AddAsync(profile);

        profile.Deactivate(DateTimeOffset.UtcNow);
        await repository.UpdateAsync(profile);

        var reloaded = await repository.GetByIdAsync(profile.Id);
        Assert.Equal(CompanyDecisionProfileStatus.Inactive, reloaded!.Status);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static CompanyDecisionProfile CreateProfile(
        string code = "TestCode",
        string name = "Test Profile",
        bool defaultProfile = false) =>
        CompanyDecisionProfile.Create(
            CompanyId,
            code,
            name,
            "Description",
            ValidWeightsJson(),
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            defaultProfile,
            DateTimeOffset.UtcNow);

    private static string ValidWeightsJson() =>
        CompanyDecisionProfile.SerializeDimensions(
        [
            new DecisionProfileDimensionSetting
            {
                Dimension = RankingDimension.EstimatedCost,
                Weight = 0.5m,
                PreferHigherValues = false
            },
            new DecisionProfileDimensionSetting
            {
                Dimension = RankingDimension.OperationalRisk,
                Weight = 0.5m,
                PreferHigherValues = false
            }
        ]);
}
