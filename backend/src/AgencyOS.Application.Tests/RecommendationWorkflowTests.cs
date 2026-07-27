using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class RecommendationWorkflowTests
{
    [Fact]
    public void Create_StartsInDraft_WithTimelineEntry()
    {
        var workflow = CreateValid();

        Assert.Equal(RecommendationWorkflowStatus.Draft, workflow.Status);
        Assert.Single(workflow.Transitions);
        Assert.Equal(RecommendationWorkflowStatus.Draft, workflow.Transitions.First().ToStatus);
    }

    [Fact]
    public void Submit_FromDraft_MovesToPendingApproval()
    {
        var workflow = CreateValid();
        workflow.Submit("planner", "Please review", DateTimeOffset.UtcNow);

        Assert.Equal(RecommendationWorkflowStatus.PendingApproval, workflow.Status);
        Assert.Equal(2, workflow.Transitions.Count);
    }

    [Fact]
    public void Approve_RequiresPendingApproval_AndBecomesImmutable()
    {
        var workflow = CreateValid();
        workflow.Submit("planner", null, DateTimeOffset.UtcNow);
        workflow.Approve("approver", DateTimeOffset.UtcNow, "Looks good", DateTimeOffset.UtcNow);

        Assert.Equal(RecommendationWorkflowStatus.Approved, workflow.Status);
        Assert.True(workflow.IsImmutable);
        Assert.Equal("approver", workflow.Approver);
        Assert.Throws<InvalidOperationException>(() =>
            workflow.Reject("approver", "late", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reject_Then_Reopen_Then_Submit()
    {
        var workflow = CreateValid();
        workflow.Submit("planner", null, DateTimeOffset.UtcNow);
        workflow.Reject("approver", "Need more capacity", DateTimeOffset.UtcNow);
        workflow.Reopen("planner", "Adjusted mix", DateTimeOffset.UtcNow);
        workflow.Submit("planner", "Resubmitted", DateTimeOffset.UtcNow);

        Assert.Equal(RecommendationWorkflowStatus.PendingApproval, workflow.Status);
        Assert.True(workflow.Transitions.Count >= 5);
    }

    [Fact]
    public void Cancelled_CannotBeReopened()
    {
        var workflow = CreateValid();
        workflow.Cancel("planner", "Withdrawn", DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            workflow.Reopen("planner", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void OnlyDraftOrReopened_CanBeSubmitted()
    {
        var workflow = CreateValid();
        workflow.Submit("planner", null, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            workflow.Submit("planner", null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ValidateTransition_EnforcesAllowedPaths()
    {
        Assert.True(RecommendationWorkflowStatus.ValidateTransition(
            RecommendationWorkflowStatus.Draft,
            RecommendationWorkflowStatus.PendingApproval));
        Assert.False(RecommendationWorkflowStatus.ValidateTransition(
            RecommendationWorkflowStatus.Approved,
            RecommendationWorkflowStatus.Rejected));
        Assert.False(RecommendationWorkflowStatus.ValidateTransition(
            RecommendationWorkflowStatus.Cancelled,
            RecommendationWorkflowStatus.Reopened));
    }

    private static RecommendationWorkflow CreateValid() =>
        RecommendationWorkflow.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Balanced delivery mix",
            "Top ranked strategy",
            "planner@agencyos.local",
            1,
            0.91m,
            DateTimeOffset.UtcNow);
}
