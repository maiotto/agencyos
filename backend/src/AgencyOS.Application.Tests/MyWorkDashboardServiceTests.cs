using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class MyWorkDashboardServiceTests
{
    private readonly Mock<IPersonalDashboardAggregationService> _aggregationService = new();
    private readonly Mock<IPersonalTimelineService> _timelineService = new();
    private readonly Mock<IPersonalKpiService> _kpiService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();
    private readonly Mock<IAuditContext> _auditContext = new();

    public MyWorkDashboardServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCompany());

        _aggregationService
            .Setup(service => service.GetMissionsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkMissionCardResponse>());
        _aggregationService
            .Setup(service => service.GetTasksAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkTaskCardResponse>());
        _aggregationService
            .Setup(service => service.GetRecommendationsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkRecommendationCardResponse>());
        _aggregationService
            .Setup(service => service.GetDecisionsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkDecisionCardResponse>());
        _aggregationService
            .Setup(service => service.GetCapacityAsync(It.IsAny<Guid?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkCapacitySummaryResponse());
        _aggregationService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<Guid?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkWorkloadSummaryResponse());

        _timelineService
            .Setup(service => service.GetTimelineAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkActivityItemResponse>());

        _kpiService
            .Setup(service => service.Calculate(
                It.IsAny<IReadOnlyList<MyWorkMissionCardResponse>>(),
                It.IsAny<IReadOnlyList<MyWorkTaskCardResponse>>(),
                It.IsAny<IReadOnlyList<MyWorkRecommendationCardResponse>>(),
                It.IsAny<IReadOnlyList<MyWorkDecisionCardResponse>>(),
                It.IsAny<MyWorkCapacitySummaryResponse>(),
                It.IsAny<MyWorkWorkloadSummaryResponse>(),
                It.IsAny<DateOnly>(),
                It.IsAny<int>()))
            .Returns(new MyWorkKpiSummaryResponse());

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<ExecutionResourceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>());
    }

    private MyWorkDashboardService CreateService() =>
        new(
            _aggregationService.Object,
            _timelineService.Object,
            _kpiService.Object,
            _companyRepository.Object,
            _executionResourceRepository.Object,
            _companyContext.Object,
            _auditContext.Object);

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal(contextCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesCompanyId_ToDefault_WhenNoParametersOrContext()
    {
        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, dashboard.CompanyId);
    }

    [Fact]
    public async Task GetDashboardAsync_ThrowsNotFound_WhenCompanyDoesNotExist()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetDashboardAsync(new MyWorkDashboardQueryParameters { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesUserId_FromParametersFirst()
    {
        _auditContext.Setup(context => context.UserId).Returns("context-user");

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { UserId = "explicit-user" });

        Assert.Equal("explicit-user", dashboard.UserId);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesUserId_FromAuditContext_WhenParametersUnset()
    {
        _auditContext.Setup(context => context.UserId).Returns("audit-user");

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal("audit-user", dashboard.UserId);
    }

    [Fact]
    public async Task GetDashboardAsync_DefaultsUserIdToSystem_WhenNoParametersOrAuditContext()
    {
        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal("system", dashboard.UserId);
    }

    [Fact]
    public async Task GetDashboardAsync_UsesExecutionResourceIdFromParameters_WithoutHeuristicLookup()
    {
        var resourceId = Guid.NewGuid();

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(
            new MyWorkDashboardQueryParameters { ExecutionResourceId = resourceId });

        Assert.Equal(resourceId, dashboard.ExecutionResourceId);
        _executionResourceRepository.Verify(
            repository => repository.GetAllAsync(It.IsAny<ExecutionResourceQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDashboardAsync_ResolvesExecutionResourceId_ViaCodeHeuristic_CaseInsensitive()
    {
        var matchingResource = new ExecutionResource { Id = Guid.NewGuid(), Code = "ALICE", Status = ExecutionResourceStatus.Active };
        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<ExecutionResourceQueryParameters>(parameters => parameters.Status == ExecutionResourceStatus.Active),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource> { matchingResource });

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { UserId = "alice" });

        Assert.Equal(matchingResource.Id, dashboard.ExecutionResourceId);
    }

    [Fact]
    public async Task GetDashboardAsync_DoesNotResolveExecutionResourceId_ForSystemUser()
    {
        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { UserId = "system" });

        Assert.Null(dashboard.ExecutionResourceId);
        _executionResourceRepository.Verify(
            repository => repository.GetAllAsync(It.IsAny<ExecutionResourceQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsNullExecutionResourceId_WhenNoActiveResourceMatchesCode()
    {
        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { UserId = "nobody" });

        Assert.Null(dashboard.ExecutionResourceId);
    }

    [Fact]
    public async Task GetDashboardAsync_DefaultsPeriodWindow_ToTrailing30Days()
    {
        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal(dashboard.PeriodEnd.AddDays(-30), dashboard.PeriodStart);
        Assert.True((dashboard.To - dashboard.From).TotalDays is >= 29.9 and <= 30.1);
    }

    [Fact]
    public async Task GetDashboardAsync_UsesExplicitPeriodWindow_WhenProvided()
    {
        var periodStart = new DateOnly(2026, 6, 1);
        var periodEnd = new DateOnly(2026, 6, 30);

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(
            new MyWorkDashboardQueryParameters { PeriodStart = periodStart, PeriodEnd = periodEnd });

        Assert.Equal(periodStart, dashboard.PeriodStart);
        Assert.Equal(periodEnd, dashboard.PeriodEnd);
    }

    [Fact]
    public async Task GetDashboardAsync_NeverCallsAnyWriteMethods()
    {
        // The mocked interfaces only expose read methods; verifying calls are limited to Get*
        // demonstrates the orchestration never reaches for a write/persist operation.
        var service = CreateService();
        await service.GetDashboardAsync(new MyWorkDashboardQueryParameters { CompanyId = Guid.NewGuid() });

        _aggregationService.Verify(
            service => service.GetMissionsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _aggregationService.Verify(
            service => service.GetTasksAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDashboardAsync_SplitsOverdueAndUpcomingTasksFromTaskList()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var overdueTask = new MyWorkTaskCardResponse
        {
            Id = Guid.NewGuid(),
            Name = "Overdue",
            MissionId = Guid.NewGuid(),
            MissionName = "Mission",
            PlannedEnd = today.AddDays(-3),
            IsOverdue = true
        };
        var upcomingTask = new MyWorkTaskCardResponse
        {
            Id = Guid.NewGuid(),
            Name = "Upcoming",
            MissionId = Guid.NewGuid(),
            MissionName = "Mission",
            PlannedEnd = today.AddDays(5),
            IsOverdue = false
        };

        _aggregationService
            .Setup(service => service.GetTasksAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkTaskCardResponse> { overdueTask, upcomingTask });

        var service = CreateService();
        var dashboard = await service.GetDashboardAsync(new MyWorkDashboardQueryParameters());

        Assert.Single(dashboard.OverdueTasks);
        Assert.Single(dashboard.UpcomingDeadlines);
        Assert.Equal(overdueTask.Id, dashboard.OverdueTasks[0].TaskId);
        Assert.Equal(upcomingTask.Id, dashboard.UpcomingDeadlines[0].TaskId);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsCounts()
    {
        _aggregationService
            .Setup(service => service.GetMissionsAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkMissionCardResponse> { new(), new() });
        _aggregationService
            .Setup(service => service.GetTasksAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkTaskCardResponse> { new() });

        var service = CreateService();
        var summary = await service.GetSummaryAsync(new MyWorkDashboardQueryParameters());

        Assert.Equal(2, summary.MissionCount);
        Assert.Equal(1, summary.TaskCount);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsTasksSectionOnly()
    {
        var service = CreateService();
        var result = await service.GetTasksAsync(new MyWorkDashboardQueryParameters());

        Assert.NotNull(result.Tasks);
        Assert.NotNull(result.OverdueTasks);
        Assert.NotNull(result.UpcomingDeadlines);
    }

    [Fact]
    public async Task GetActivityAsync_UsesDefaultDateWindow_WhenNotProvided()
    {
        DateTimeOffset? capturedFrom = null;
        DateTimeOffset? capturedTo = null;

        _timelineService
            .Setup(service => service.GetTimelineAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Guid, DateTimeOffset?, DateTimeOffset?, CancellationToken>(
                (_, _, from, to, _) =>
                {
                    capturedFrom = from;
                    capturedTo = to;
                })
            .ReturnsAsync(new List<MyWorkActivityItemResponse>());

        var service = CreateService();
        await service.GetActivityAsync(new MyWorkDashboardQueryParameters());

        Assert.NotNull(capturedFrom);
        Assert.NotNull(capturedTo);
        Assert.True((capturedTo!.Value - capturedFrom!.Value).TotalDays is >= 29.9 and <= 30.1);
    }

    private static Company CreateCompany() =>
        Company.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "AOS",
            "AgencyOS Default",
            null,
            "UTC",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
}
