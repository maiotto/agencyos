using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class RecommendationHistoryTests
{
    [Fact]
    public void Create_PersistsImmutableSnapshot()
    {
        var history = CreateValid();

        Assert.Equal(RecommendationHistoryEventType.VersionCreated, history.EventType);
        Assert.Equal(1, history.RecommendationVersion);
        Assert.Equal(RecommendationStatus.Active, history.RecommendationStatus);
    }

    [Fact]
    public void Create_RejectsEmptyMandatoryFields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CreateValid(recommendationId: Guid.Empty));
        Assert.Throws<InvalidOperationException>(() =>
            CreateValid(payload: " "));
        Assert.Throws<InvalidOperationException>(() =>
            CreateValid(eventType: "UnknownEvent"));
    }

    [Fact]
    public void FromRecommendation_CopiesSnapshotFields()
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-HIST-1",
            "Human + AI",
            "Summary",
            "Reason",
            80m,
            1,
            2,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);

        var history = RecommendationHistory.FromRecommendation(
            recommendation,
            RecommendationHistoryEventType.VersionCreated,
            "decision-engine",
            DateTimeOffset.UtcNow);

        Assert.Equal(recommendation.Id, history.RecommendationId);
        Assert.Equal(2, history.RecommendationVersion);
        Assert.Equal(recommendation.RecommendationPayload, history.RecommendationPayload);
    }

    private static RecommendationHistory CreateValid(
        Guid? recommendationId = null,
        string payload = "{\"ok\":true}",
        string eventType = RecommendationHistoryEventType.VersionCreated) =>
        RecommendationHistory.Create(
            recommendationId ?? Guid.NewGuid(),
            "REC-HIST-1",
            1,
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Human + AI",
            "Summary",
            eventType,
            RecommendationStatus.Active,
            null,
            null,
            null,
            null,
            null,
            80m,
            1,
            null,
            RecommendationVersions.CurrentDecisionEngineVersion,
            "{}",
            "{}",
            payload,
            "decision-engine",
            DateTimeOffset.UtcNow);
}
