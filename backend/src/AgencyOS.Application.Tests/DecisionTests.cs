using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class DecisionTests
{
    [Fact]
    public void Create_StartsCreatedAndNotStarted_WithTimeline()
    {
        var decision = CreateValid();

        Assert.Equal(DecisionStatus.Created, decision.DecisionStatus);
        Assert.Equal(DecisionImplementationStatus.NotStarted, decision.ImplementationStatus);
        Assert.Single(decision.Timeline);
        Assert.Equal(DecisionTimelineEventType.Created, decision.Timeline.First().EventType);
        Assert.True(decision.Validate());
    }

    [Fact]
    public void Lifecycle_StartCompleteRecordOutcome()
    {
        var decision = CreateValid();

        decision.StartImplementation("planner", "go", DateTimeOffset.UtcNow);
        Assert.Equal(DecisionStatus.InProgress, decision.DecisionStatus);
        Assert.Equal(DecisionImplementationStatus.InProgress, decision.ImplementationStatus);
        Assert.NotNull(decision.ImplementationDate);

        decision.Complete("planner", "done", DateTimeOffset.UtcNow);
        Assert.Equal(DecisionStatus.Completed, decision.DecisionStatus);
        Assert.NotNull(decision.CompletedDate);

        decision.RecordOutcome("Delivered on time", "High", "planner", null, DateTimeOffset.UtcNow);
        Assert.Equal("Delivered on time", decision.Outcome);
        Assert.Equal("High", decision.BusinessValue);
        Assert.Equal(4, decision.Timeline.Count);
    }

    [Fact]
    public void Complete_CannotReturnToInProgress()
    {
        var decision = CreateValid();
        decision.StartImplementation("planner", null, DateTimeOffset.UtcNow);
        decision.Complete("planner", null, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            decision.StartImplementation("planner", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RecordOutcome_OnlyAfterCompletion()
    {
        var decision = CreateValid();
        Assert.Throws<InvalidOperationException>(() =>
            decision.RecordOutcome("early", null, "planner", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Cancel_FromCreated()
    {
        var decision = CreateValid();
        decision.Cancel("planner", "stop", DateTimeOffset.UtcNow);

        Assert.Equal(DecisionStatus.Cancelled, decision.DecisionStatus);
        Assert.Equal(DecisionImplementationStatus.Cancelled, decision.ImplementationStatus);
    }

    [Fact]
    public void Create_RejectsEmptyRecommendationId()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Decision.Create(
                Guid.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "planner",
                DateTimeOffset.UtcNow));
    }

    private static Decision CreateValid() =>
        Decision.Create(
            Guid.NewGuid(),
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "planner",
            DateTimeOffset.UtcNow);
}
