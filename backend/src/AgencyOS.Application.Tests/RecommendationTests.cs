using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class RecommendationTests
{
    [Fact]
    public void Create_StartsActive_NotArchived()
    {
        var recommendation = CreateValid();

        Assert.Equal(RecommendationStatus.Active, recommendation.Status);
        Assert.False(recommendation.Archived);
        Assert.Equal(1, recommendation.Version);
        Assert.Null(recommendation.ArchivedAt);
    }

    [Fact]
    public void Create_RejectsEmptyMandatoryFields()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(companyId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() => CreateValid(title: " "));
        Assert.Throws<InvalidOperationException>(() => CreateValid(payload: " "));
        Assert.Throws<InvalidOperationException>(() => CreateValid(version: 0));
    }

    [Fact]
    public void Archive_Then_Restore()
    {
        var recommendation = CreateValid();
        recommendation.Archive(DateTimeOffset.UtcNow);

        Assert.True(recommendation.Archived);
        Assert.Equal(RecommendationStatus.Archived, recommendation.Status);
        Assert.NotNull(recommendation.ArchivedAt);

        recommendation.Restore();

        Assert.False(recommendation.Archived);
        Assert.Equal(RecommendationStatus.Active, recommendation.Status);
        Assert.Null(recommendation.ArchivedAt);
    }

    [Fact]
    public void Archive_Twice_Throws()
    {
        var recommendation = CreateValid();
        recommendation.Archive(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => recommendation.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CreateNewVersion_IncrementsVersion_SameNumber()
    {
        var source = CreateValid();
        var versioned = source.CreateNewVersion(
            "Revised",
            "Updated summary",
            "Updated reason",
            0.95m,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"v\":2}",
            "planner",
            DateTimeOffset.UtcNow);

        Assert.Equal(source.RecommendationNumber, versioned.RecommendationNumber);
        Assert.Equal(2, versioned.Version);
        Assert.NotEqual(source.Id, versioned.Id);
        Assert.Equal("Revised", versioned.Title);
        Assert.Equal(RecommendationStatus.Active, versioned.Status);
    }

    private static Recommendation CreateValid(
        Guid? companyId = null,
        string title = "Human + AI",
        string payload = "{\"ok\":true}",
        int version = 1) =>
        Recommendation.Create(
            companyId ?? Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-20260726-TEST01",
            title,
            "Summary",
            "Reason",
            82.5m,
            1,
            version,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            payload,
            "decision-engine",
            DateTimeOffset.UtcNow);
}
