using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ExecutiveRecommendationSummaryTests
{
    [Fact]
    public void Generate_CreatesActiveSummary()
    {
        var item = CreateValid();

        Assert.Equal(ExecutiveRecommendationSummaryStatus.Active, item.Status);
        Assert.Equal(1, item.SummaryVersion);
        Assert.True(item.ConfidenceLevel is >= 0 and <= 100);
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
    public void CreateNewVersion_CreatesHigherVersionWithoutMutatingSource()
    {
        var item = CreateValid();
        var next = item.CreateNewVersion(
            2,
            DateTimeOffset.UtcNow,
            "exec",
            "Updated briefing",
            "[\"f\"]",
            "business",
            "capacity",
            "workload",
            "[\"r\"]",
            "[\"a\"]",
            88m,
            "[\"action\"]",
            null,
            null,
            ExecutiveRecommendationSummaryVersions.CurrentModelVersion,
            ExecutiveRecommendationSummaryVersions.CurrentPromptVersion);

        Assert.Equal(1, item.SummaryVersion);
        Assert.Equal(2, next.SummaryVersion);
        Assert.NotEqual(item.Id, next.Id);
        Assert.Equal(item.RecommendationId, next.RecommendationId);
    }

    [Fact]
    public void Compare_HighlightsDifferencesAgainstRecommendation()
    {
        var recommendation = CreateRecommendation();
        var summary = CreateValid(recommendation.Id);
        var comparison = summary.Compare(recommendation);

        Assert.True(comparison.HasDifferences);
        Assert.Contains(comparison.Differences, item => item.Path == "title");
    }

    [Fact]
    public void Generate_RejectsMissingMandatoryFields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExecutiveRecommendationSummary.Generate(
                Guid.NewGuid(),
                null,
                null,
                1,
                DateTimeOffset.UtcNow,
                "exec",
                " ",
                "[]",
                "business",
                "capacity",
                "workload",
                "[]",
                "[]",
                80m,
                "[]",
                "model",
                "prompt"));

        Assert.Throws<InvalidOperationException>(() =>
            ExecutiveRecommendationSummary.Generate(
                Guid.NewGuid(),
                null,
                null,
                1,
                DateTimeOffset.UtcNow,
                "exec",
                "summary",
                "[]",
                "business",
                "capacity",
                "workload",
                "[]",
                "[]",
                120m,
                "[]",
                "model",
                "prompt"));
    }

    private static ExecutiveRecommendationSummary CreateValid(Guid? recommendationId = null) =>
        ExecutiveRecommendationSummary.Generate(
            recommendationId ?? Guid.NewGuid(),
            null,
            null,
            1,
            DateTimeOffset.UtcNow,
            "exec",
            "Executive briefing",
            "[\"factor\"]",
            "Business impact",
            "Capacity impact",
            "Workload impact",
            "[\"risk\"]",
            "[\"assumption\"]",
            80m,
            "[\"action\"]",
            ExecutiveRecommendationSummaryVersions.CurrentModelVersion,
            ExecutiveRecommendationSummaryVersions.CurrentPromptVersion);

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-EXEC-1",
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
