using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class PersonalMetricsServiceTests
{
    private readonly PersonalMetricsService _service = new();

    [Fact]
    public void BuildKpis_ComputesCompletionRateFromPendingAndCompleted()
    {
        var kpis = _service.BuildKpis(
            new MyWorkKpiSummaryResponse
            {
                AssignedTaskCount = 2,
                PendingDecisionCount = 2,
                UtilizationPercentage = 70m,
                WorkloadPercentage = 55m
            },
            completedDecisionCount: 2,
            activityEventCount: 5);

        Assert.Equal(2, kpis.CompletedDecisionCount);
        Assert.Equal(5, kpis.ActivityEventCount);
        Assert.Equal(33.33m, kpis.CompletionRatePercentage);
    }

    [Fact]
    public void BuildPerformance_SetsFocusHintForOverdue()
    {
        var performance = _service.BuildPerformance(
            new PersonalProductivityKpiResponse { OverdueTaskCount = 1 },
            new List<MyWorkTaskCardResponse>
            {
                new() { IsOverdue = true, Name = "Late" },
                new() { IsOverdue = false, Name = "Ok" }
            });

        Assert.Equal(50m, performance.OnTimeTaskPercentage);
        Assert.Equal("Address overdue work", performance.FocusHint);
    }

    [Fact]
    public void BuildStatistics_IncludesNavigationLinks()
    {
        var stats = _service.BuildStatistics(
            new PersonalProductivityKpiResponse { AssignedMissionCount = 1, AssignedTaskCount = 1 },
            new List<MyWorkTaskCardResponse> { new() { AssignmentPlannedHours = 4m } },
            new MyWorkCapacitySummaryResponse { HasData = true, TotalCapacityHours = 40m },
            new MyWorkWorkloadSummaryResponse { HasData = true, TotalPlannedHours = 20m });

        Assert.Equal(4m, stats.TotalPendingPlannedHours);
        Assert.Contains(stats.NavigationLinks, link => link.Path == "/my-work");
        Assert.Contains(stats.NavigationLinks, link => link.Path == "/notifications");
    }
}

public class PersonalTrendServiceTests
{
    private readonly PersonalTrendService _service = new();

    [Fact]
    public void BuildTrends_MarksUpDownStable()
    {
        var trends = _service.BuildTrends(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 30),
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 30),
            new MyWorkCapacitySummaryResponse { HasData = true, UtilizationPercentage = 80m },
            new MyWorkWorkloadSummaryResponse { HasData = true, WorkloadPercentage = 40m },
            new MyWorkCapacitySummaryResponse { HasData = true, UtilizationPercentage = 60m },
            new MyWorkWorkloadSummaryResponse { HasData = true, WorkloadPercentage = 45m },
            currentActivityCount: 10,
            previousActivityCount: 10,
            currentCompletedCount: 3,
            previousCompletedCount: 1);

        Assert.Equal("Up", trends.Points.Single(point => point.Metric == "CapacityUtilization").Direction);
        Assert.Equal("Down", trends.Points.Single(point => point.Metric == "WorkloadUtilization").Direction);
        Assert.Equal("Stable", trends.Points.Single(point => point.Metric == "ActivityEvents").Direction);
        Assert.Equal("Up", trends.Points.Single(point => point.Metric == "CompletedDecisions").Direction);
    }
}

public class ActivitySummaryServiceTests
{
    [Fact]
    public void Build_MapsPendingAndCompleted()
    {
        var summary = new ActivitySummaryService().Build(
            new List<MyWorkTaskCardResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Task A",
                    Status = "Active",
                    DrillDownPath = "/recommendations?missionId=1"
                }
            },
            new List<MyWorkRecommendationCardResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Rec",
                    Status = "PendingApproval",
                    CreatedAt = DateTimeOffset.UtcNow,
                    DrillDownPath = "/recommendations/workflow/1"
                }
            },
            new List<MyWorkDecisionCardResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DecisionStatus = "Created",
                    DecisionDate = DateTimeOffset.UtcNow,
                    DrillDownPath = "/decisions/1"
                }
            },
            new List<PersonalProductivityActivityItemResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Kind = "Decision",
                    Title = "Done",
                    Status = "Completed",
                    DrillDownPath = "/decisions/2"
                }
            },
            new List<MyWorkActivityItemResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Action = "Decision.Complete",
                    Summary = "Completed",
                    DrillDownPath = "/audit/1"
                }
            });

        Assert.Equal(3, summary.Pending.Count);
        Assert.Single(summary.Completed);
        Assert.Single(summary.Timeline);
    }
}

public class PersonalProductivityDashboardQueryParametersValidatorTests
{
    private readonly PersonalProductivityDashboardQueryParametersValidator _validator = new();

    [Fact]
    public void AcceptsValidWindow()
    {
        var result = _validator.Validate(new PersonalProductivityDashboardQueryParameters
        {
            From = DateTimeOffset.UtcNow.AddDays(-7),
            To = DateTimeOffset.UtcNow,
            PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            PeriodEnd = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsInvertedDates()
    {
        var result = _validator.Validate(new PersonalProductivityDashboardQueryParameters
        {
            From = DateTimeOffset.UtcNow,
            To = DateTimeOffset.UtcNow.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }
}
