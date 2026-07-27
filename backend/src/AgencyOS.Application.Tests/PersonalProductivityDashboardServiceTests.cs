using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class PersonalProductivityDashboardServiceTests
{
    private readonly Mock<IPersonalDashboardAggregationService> _aggregationService = new();
    private readonly Mock<IPersonalTimelineService> _timelineService = new();
    private readonly Mock<IPersonalKpiService> _kpiService = new();
    private readonly Mock<IDecisionService> _decisionService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();
    private readonly Mock<IAuditContext> _auditContext = new();
    private readonly Mock<IAuditService> _auditService = new();

    public PersonalProductivityDashboardServiceTests()
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
            .ReturnsAsync(new MyWorkCapacitySummaryResponse { HasData = true, UtilizationPercentage = 70m });
        _aggregationService
            .Setup(service => service.GetWorkloadAsync(It.IsAny<Guid?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MyWorkWorkloadSummaryResponse { HasData = true, WorkloadPercentage = 55m });

        _timelineService
            .Setup(service => service.GetTimelineAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MyWorkActivityItemResponse>
            {
                new() { Id = Guid.NewGuid(), Action = "Decision.Create", Summary = "Created" }
            });

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
            .Returns(new MyWorkKpiSummaryResponse
            {
                AssignedTaskCount = 1,
                PendingDecisionCount = 1,
                UtilizationPercentage = 70m,
                WorkloadPercentage = 55m
            });

        _decisionService
            .Setup(service => service.FilterAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DecisionResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DecisionStatus = DecisionStatus.Completed,
                    ImplementationStatus = DecisionImplementationStatus.Completed,
                    CreatedBy = "planner",
                    DecisionDate = DateTimeOffset.UtcNow.AddDays(-2),
                    CompletedDate = DateTimeOffset.UtcNow.AddDays(-1)
                }
            });

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<ExecutionResourceQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>());

        _auditService
            .Setup(service => service.RecordSafeAsync(It.IsAny<AuditEventWriteRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private PersonalProductivityDashboardService CreateService() =>
        new(
            _aggregationService.Object,
            _timelineService.Object,
            _kpiService.Object,
            new PersonalMetricsService(),
            new PersonalTrendService(),
            new ActivitySummaryService(),
            _decisionService.Object,
            _companyRepository.Object,
            _executionResourceRepository.Object,
            _companyContext.Object,
            _auditContext.Object,
            _auditService.Object,
            NullLogger<PersonalProductivityDashboardService>.Instance);

    [Fact]
    public async Task GetDashboardAsync_ReturnsSectionsAndRecordsUsage()
    {
        _auditContext.SetupGet(context => context.UserId).Returns("planner");

        var dashboard = await CreateService().GetDashboardAsync(new PersonalProductivityDashboardQueryParameters());

        Assert.Equal("planner", dashboard.UserId);
        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, dashboard.CompanyId);
        Assert.NotEmpty(dashboard.Trends.Points);
        Assert.True(dashboard.Capacity.HasData);
        Assert.Single(dashboard.Activity.Completed);
        Assert.Contains(dashboard.Statistics.NavigationLinks, link => link.Path == "/my-work");

        _auditService.Verify(service => service.RecordSafeAsync(
            It.Is<AuditEventWriteRequest>(request =>
                request.EntityType == AuditEntityTypes.PersonalProductivityDashboard
                && request.Action == "PersonalProductivityDashboard.View"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboardAsync_ThrowsWhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetDashboardAsync(new PersonalProductivityDashboardQueryParameters()));
    }

    [Fact]
    public async Task GetTrendsAsync_ComparesCurrentAndPreviousPeriods()
    {
        var trends = await CreateService().GetTrendsAsync(new PersonalProductivityDashboardQueryParameters
        {
            PeriodStart = new DateOnly(2026, 7, 1),
            PeriodEnd = new DateOnly(2026, 7, 30)
        });

        Assert.Equal(new DateOnly(2026, 7, 1), trends.CurrentPeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 1), trends.PreviousPeriodStart);
        Assert.Equal(4, trends.Points.Count);
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
