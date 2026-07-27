using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class PersonalKpiServiceTests
{
    private readonly PersonalKpiService _service = new();

    private static MyWorkTaskCardResponse CreateTask(bool isOverdue, DateOnly? plannedEnd) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Task",
        IsOverdue = isOverdue,
        PlannedEnd = plannedEnd
    };

    [Fact]
    public void Calculate_CountsAssignedMissionsAndTasks()
    {
        var missions = new List<MyWorkMissionCardResponse> { new(), new() };
        var tasks = new List<MyWorkTaskCardResponse> { CreateTask(false, null) };

        var kpis = _service.Calculate(
            missions,
            tasks,
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse(),
            new MyWorkWorkloadSummaryResponse(),
            new DateOnly(2026, 1, 1));

        Assert.Equal(2, kpis.AssignedMissionCount);
        Assert.Equal(1, kpis.AssignedTaskCount);
    }

    [Fact]
    public void Calculate_CountsPendingRecommendationsAndDecisions()
    {
        var recommendations = new List<MyWorkRecommendationCardResponse> { new(), new(), new() };
        var decisions = new List<MyWorkDecisionCardResponse> { new() };

        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            new List<MyWorkTaskCardResponse>(),
            recommendations,
            decisions,
            new MyWorkCapacitySummaryResponse(),
            new MyWorkWorkloadSummaryResponse(),
            new DateOnly(2026, 1, 1));

        Assert.Equal(3, kpis.PendingRecommendationCount);
        Assert.Equal(1, kpis.PendingDecisionCount);
    }

    [Fact]
    public void Calculate_CountsOverdueTasks()
    {
        var today = new DateOnly(2026, 1, 15);
        var tasks = new List<MyWorkTaskCardResponse>
        {
            CreateTask(true, today.AddDays(-3)),
            CreateTask(true, today.AddDays(-1)),
            CreateTask(false, today.AddDays(5))
        };

        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            tasks,
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse(),
            new MyWorkWorkloadSummaryResponse(),
            today);

        Assert.Equal(2, kpis.OverdueTaskCount);
    }

    [Fact]
    public void Calculate_CountsUpcomingDeadlinesWithinDefaultWindow()
    {
        var today = new DateOnly(2026, 1, 1);
        var tasks = new List<MyWorkTaskCardResponse>
        {
            CreateTask(false, today.AddDays(5)),
            CreateTask(false, today.AddDays(14)),
            CreateTask(false, today.AddDays(15)),
            CreateTask(false, null)
        };

        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            tasks,
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse(),
            new MyWorkWorkloadSummaryResponse(),
            today,
            upcomingDeadlineWindowDays: 14);

        Assert.Equal(2, kpis.UpcomingDeadlineCount);
    }

    [Fact]
    public void Calculate_ExcludesOverdueTasksFromUpcomingCount()
    {
        var today = new DateOnly(2026, 1, 10);
        var tasks = new List<MyWorkTaskCardResponse> { CreateTask(true, today.AddDays(-1)) };

        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            tasks,
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse(),
            new MyWorkWorkloadSummaryResponse(),
            today);

        Assert.Equal(0, kpis.UpcomingDeadlineCount);
        Assert.Equal(1, kpis.OverdueTaskCount);
    }

    [Fact]
    public void Calculate_ReturnsNullUtilizationAndWorkload_WhenSummariesHaveNoData()
    {
        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            new List<MyWorkTaskCardResponse>(),
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse { HasData = false },
            new MyWorkWorkloadSummaryResponse { HasData = false },
            new DateOnly(2026, 1, 1));

        Assert.Null(kpis.UtilizationPercentage);
        Assert.Null(kpis.WorkloadPercentage);
    }

    [Fact]
    public void Calculate_ReturnsUtilizationAndWorkload_WhenSummariesHaveData()
    {
        var kpis = _service.Calculate(
            new List<MyWorkMissionCardResponse>(),
            new List<MyWorkTaskCardResponse>(),
            new List<MyWorkRecommendationCardResponse>(),
            new List<MyWorkDecisionCardResponse>(),
            new MyWorkCapacitySummaryResponse { HasData = true, UtilizationPercentage = 72.5m },
            new MyWorkWorkloadSummaryResponse { HasData = true, WorkloadPercentage = 61.2m },
            new DateOnly(2026, 1, 1));

        Assert.Equal(72.5m, kpis.UtilizationPercentage);
        Assert.Equal(61.2m, kpis.WorkloadPercentage);
    }
}
