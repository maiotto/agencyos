using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CompanyDecisionProfileDomainTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public void Create_BuildsVersionOneActiveProfile()
    {
        var profile = CreateValidProfile();

        Assert.Equal(1, profile.Version);
        Assert.Equal(CompanyDecisionProfileStatus.Active, profile.Status);
        Assert.NotEqual(Guid.Empty, profile.ProfileFamilyId);
        Assert.Equal(2, profile.Dimensions.Count);
        Assert.False(profile.Archived);
        Assert.True(profile.IsActive);
    }

    [Fact]
    public void Create_ThrowsWhenNoPositiveWeightDimension()
    {
        var weights = CompanyDecisionProfile.SerializeDimensions(
        [
            new DecisionProfileDimensionSetting { Dimension = RankingDimension.EstimatedCost, Weight = 0m }
        ]);

        Assert.Throws<InvalidOperationException>(() => CompanyDecisionProfile.Create(
            CompanyId,
            "Code",
            "Name",
            null,
            weights,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            false,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_ThrowsWhenWeightOutOfRange()
    {
        Assert.Throws<InvalidOperationException>(() => CompanyDecisionProfile.Create(
            CompanyId,
            "Code",
            "Name",
            null,
            ValidWeightsJson(),
            1.5m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            false,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CreateNewVersion_IncrementsVersionAndKeepsFamily()
    {
        var profile = CreateValidProfile();

        var newVersion = profile.CreateNewVersion(
            "Updated Name",
            "Updated description",
            ValidWeightsJson(),
            0.3m,
            0.3m,
            0.1m,
            0.2m,
            0.1m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Equal(2, newVersion.Version);
        Assert.Equal(profile.ProfileFamilyId, newVersion.ProfileFamilyId);
        Assert.NotEqual(profile.Id, newVersion.Id);
        Assert.Equal("Updated Name", newVersion.Name);
        Assert.Equal(CompanyDecisionProfileStatus.Active, newVersion.Status);
    }

    [Fact]
    public void CreateNewVersion_ThrowsWhenArchived()
    {
        var profile = CreateValidProfile();
        profile.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => profile.CreateNewVersion(
            "Name",
            null,
            ValidWeightsJson(),
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            0.2m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Clone_CreatesNewFamilyWithVersionOneAndNoDefault()
    {
        var profile = CreateValidProfile();
        profile.SetDefault(true, DateTimeOffset.UtcNow);

        var clone = profile.Clone("Cloned Name", "ClonedCode", DateTimeOffset.UtcNow);

        Assert.NotEqual(profile.Id, clone.Id);
        Assert.NotEqual(profile.ProfileFamilyId, clone.ProfileFamilyId);
        Assert.Equal(1, clone.Version);
        Assert.False(clone.DefaultProfile);
        Assert.Equal("Cloned Name", clone.Name);
        Assert.Equal("ClonedCode", clone.Code);
    }

    [Fact]
    public void Clone_ThrowsWhenNameOrCodeMissing()
    {
        var profile = CreateValidProfile();

        Assert.Throws<InvalidOperationException>(() => profile.Clone("", "Code", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => profile.Clone("Name", "", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Deactivate_ThrowsWhenDefault()
    {
        var profile = CreateValidProfile();
        profile.SetDefault(true, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => profile.Deactivate(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Archive_ThrowsWhenDefault()
    {
        var profile = CreateValidProfile();
        profile.SetDefault(true, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => profile.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Archive_ThrowsWhenAlreadyArchived()
    {
        var profile = CreateValidProfile();
        profile.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => profile.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void SetDefault_ThrowsWhenProfileNotActive()
    {
        var profile = CreateValidProfile();
        profile.SetDefault(true, DateTimeOffset.UtcNow);
        profile.ClearDefault(DateTimeOffset.UtcNow);
        profile.Deactivate(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => profile.SetDefault(true, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ClearDefault_RemovesDefaultFlag()
    {
        var profile = CreateValidProfile();
        profile.SetDefault(true, DateTimeOffset.UtcNow);

        profile.ClearDefault(DateTimeOffset.UtcNow);

        Assert.False(profile.DefaultProfile);
    }

    [Fact]
    public void CreateRankingView_BuildsLightweightSnapshot()
    {
        var dimensions = new List<DecisionProfileDimensionSetting>
        {
            new() { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m, PreferHigherValues = false }
        };

        var view = CompanyDecisionProfile.CreateRankingView(Guid.NewGuid(), "Code", "Name", dimensions);

        Assert.Single(view.Dimensions);
        Assert.Equal(CompanyDecisionProfileStatus.Active, view.Status);
    }

    private static CompanyDecisionProfile CreateValidProfile() =>
        CompanyDecisionProfile.Create(
            CompanyId,
            "TestCode",
            "Test Profile",
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
            false,
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
