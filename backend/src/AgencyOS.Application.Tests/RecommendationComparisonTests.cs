using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class RecommendationComparisonTests
{
    [Fact]
    public void Compare_HighlightsScalarAndJsonDifferences()
    {
        var left = CreateHistory(
            version: 1,
            score: 80m,
            rank: 2,
            capacity: "{\"hours\":40,\"team\":\"A\"}",
            workload: "{\"allocated\":10}",
            payload: "{\"strategy\":\"human\"}",
            workflowStatus: "Draft");
        var right = CreateHistory(
            version: 2,
            score: 85m,
            rank: 1,
            capacity: "{\"hours\":48,\"team\":\"A\"}",
            workload: "{\"allocated\":10}",
            payload: "{\"strategy\":\"human+ai\"}",
            workflowStatus: "Approved",
            title: "Human + AI v2");

        var comparison = RecommendationComparison.Compare(left, right);

        Assert.True(comparison.HasDifferences);
        Assert.Equal(5m, comparison.ScoreDelta);
        Assert.Equal(-1, comparison.RankDelta);
        Assert.Equal(1, comparison.VersionDelta);
        Assert.Contains(comparison.Differences, item =>
            item.Changed
            && item.Section == RecommendationComparisonSections.Capacity
            && item.Path == "hours"
            && item.LeftValue == "40"
            && item.RightValue == "48");
        Assert.Contains(comparison.Differences, item =>
            item.Changed
            && item.Section == RecommendationComparisonSections.Payload
            && item.Path == "strategy");
        Assert.Contains(comparison.Differences, item =>
            item.Changed
            && item.Section == RecommendationComparisonSections.Workflow
            && item.Path == "workflowStatus");
        Assert.Contains(comparison.Differences, item =>
            !item.Changed
            && item.Section == RecommendationComparisonSections.Workload
            && item.Path == "allocated");
    }

    [Fact]
    public void Compare_RejectsSameHistoryIds()
    {
        var history = CreateHistory(version: 1, score: 80m, rank: 1, capacity: "{}", workload: "{}", payload: "{}");
        Assert.Throws<InvalidOperationException>(() => RecommendationComparison.Compare(history, history));
    }

    private static RecommendationHistory CreateHistory(
        int version,
        decimal score,
        int rank,
        string capacity,
        string workload,
        string payload,
        string? workflowStatus = null,
        string title = "Human + AI") =>
        RecommendationHistory.Create(
            Guid.NewGuid(),
            "REC-COMPARE-1",
            version,
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            title,
            "Summary",
            RecommendationHistoryEventType.VersionCreated,
            RecommendationStatus.Active,
            workflowStatus,
            workflowStatus is null ? null : Guid.NewGuid(),
            workflowStatus == "Approved" ? "approver" : null,
            workflowStatus == "Approved" ? DateTimeOffset.UtcNow : null,
            null,
            score,
            rank,
            null,
            RecommendationVersions.CurrentDecisionEngineVersion,
            capacity,
            workload,
            payload,
            "decision-engine",
            DateTimeOffset.UtcNow);
}
