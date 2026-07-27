using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class AIRecommendationTests
{
    [Fact]
    public void Generate_CreatesActiveAdvisoryRecommendation()
    {
        var item = CreateValid();

        Assert.Equal(AIRecommendationStatus.Active, item.Status);
        Assert.Equal(1, item.GenerationVersion);
        Assert.True(item.ConfidenceScore is >= 0 and <= 100);
        Assert.True(item.Validate());
    }

    [Fact]
    public void Archive_MarksArchived()
    {
        var item = CreateValid();
        item.Archive(DateTimeOffset.UtcNow);

        Assert.True(item.Archived);
        Assert.NotNull(item.ArchivedAt);
        Assert.Throws<InvalidOperationException>(() => item.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Compare_HighlightsDifferencesAgainstRecommendation()
    {
        var recommendation = CreateRecommendation();
        var ai = AIRecommendation.Generate(
            recommendation.Id,
            recommendation.Version,
            1,
            DateTimeOffset.UtcNow,
            "ai-advisor",
            80m,
            "summary",
            "reasoning",
            "[\"a\"]",
            "[\"r\"]",
            "[\"alt\"]",
            "Hybrid delivery",
            "{\"delta\":-5}",
            "{\"delta\":-8}",
            AIRecommendationVersions.CurrentModelVersion,
            AIRecommendationVersions.CurrentPromptVersion);

        var comparison = ai.Compare(recommendation);

        Assert.True(comparison.HasDifferences);
        Assert.Contains(comparison.Differences, item => item.Path == "title" && item.Changed);
    }

    [Fact]
    public void Generate_RejectsMissingConfidenceAndVersions()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AIRecommendation.Generate(
                Guid.NewGuid(),
                1,
                1,
                DateTimeOffset.UtcNow,
                "ai",
                120m,
                "summary",
                "reasoning",
                "[]",
                "[]",
                "[]",
                "strategy",
                "{}",
                "{}",
                "model",
                "prompt"));

        Assert.Throws<InvalidOperationException>(() =>
            AIRecommendation.Generate(
                Guid.NewGuid(),
                1,
                1,
                DateTimeOffset.UtcNow,
                "ai",
                50m,
                "summary",
                "reasoning",
                "[]",
                "[]",
                "[]",
                "strategy",
                "{}",
                "{}",
                " ",
                "prompt"));
    }

    private static AIRecommendation CreateValid() =>
        AIRecommendation.Generate(
            Guid.NewGuid(),
            1,
            1,
            DateTimeOffset.UtcNow,
            "ai-advisor",
            75m,
            "Executive summary",
            "Reasoning text",
            "[\"assumption\"]",
            "[\"risk\"]",
            "[\"alternative\"]",
            "Balanced human + AI delivery mix",
            "{\"advisoryDeltaPercent\":-5}",
            "{\"advisoryDeltaPercent\":-8}",
            AIRecommendationVersions.CurrentModelVersion,
            AIRecommendationVersions.CurrentPromptVersion);

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-AI-1",
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
}
