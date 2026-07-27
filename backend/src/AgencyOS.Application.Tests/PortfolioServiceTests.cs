using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class PortfolioServiceTests
{
    private readonly Mock<IPortfolioRepository> _portfolioRepository = new();
    private readonly Mock<IMissionRepository> _missionRepository = new();
    private readonly Mock<IPlanningTemplateRepository> _planningTemplateRepository = new();
    private readonly Mock<ICapacityCalculatorService> _capacityCalculator = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculator = new();
    private readonly Mock<ICapacityHistoryService> _capacityHistory = new();
    private readonly Mock<IWorkloadHistoryService> _workloadHistory = new();
    private readonly Mock<ILogger<PortfolioService>> _logger = new();

    private PortfolioService CreateService() =>
        new(
            _portfolioRepository.Object,
            _missionRepository.Object,
            _planningTemplateRepository.Object,
            _capacityCalculator.Object,
            _workloadCalculator.Object,
            _capacityHistory.Object,
            _workloadHistory.Object,
            new AgencyOS.Application.Audit.NoOpAuditService(),
            new AgencyOS.Application.Audit.NoOpNotificationGenerationService(),
            _logger.Object);

    [Fact]
    public async Task CreateAsync_PersistsPortfolioWithMission()
    {
        var missionId = Guid.NewGuid();
        SetupActiveMission(missionId);
        _portfolioRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Portfolio? persisted = null;
        _portfolioRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Portfolio>(), It.IsAny<CancellationToken>()))
            .Callback<Portfolio, CancellationToken>((portfolio, _) => persisted = portfolio)
            .ReturnsAsync((Portfolio portfolio, CancellationToken _) => portfolio);

        var result = await CreateService().CreateAsync(CreateRequest(missionId));

        Assert.NotNull(persisted);
        Assert.Equal(result.Id, persisted!.Id);
        Assert.Single(result.Missions);
        Assert.Equal(PortfolioStatus.Active, result.Status);
    }

    [Fact]
    public async Task CreateAsync_RejectsInactiveMission()
    {
        var missionId = Guid.NewGuid();
        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission
            {
                Id = missionId,
                MissionStatusId = Guid.Parse("11111111-1111-4111-8121-000000000001")
            });
        _portfolioRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().CreateAsync(CreateRequest(missionId)));
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateName()
    {
        var missionId = Guid.NewGuid();
        SetupActiveMission(missionId);
        _portfolioRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateRequest(missionId)));
    }

    [Fact]
    public async Task DeleteAsync_RejectsActivePortfolio()
    {
        var portfolio = CreatePortfolioEntity();
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().DeleteAsync(portfolio.Id));
    }

    [Fact]
    public async Task CalculateCapacityAsync_UsesCapacityEngine()
    {
        var portfolio = CreatePortfolioEntity();
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);
        _portfolioRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Portfolio>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portfolio item, CancellationToken _) => item);
        _capacityCalculator
            .Setup(service => service.GetSummaryAsync(
                It.IsAny<CapacityQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacitySummaryResponse
            {
                PeriodStartDate = portfolio.PlanningPeriodStart,
                PeriodEndDate = portfolio.PlanningPeriodEnd,
                OverallUtilizationPercentage = 62.5m,
                TotalCapacityHours = 100m
            });

        var result = await CreateService().CalculateCapacityAsync(portfolio.Id);

        Assert.False(string.IsNullOrWhiteSpace(result.CapacitySummary));
        Assert.Contains("62.5", result.CapacitySummary!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetHealthAsync_ComputesDeterministicHealth()
    {
        var portfolio = CreatePortfolioEntity();
        SetupCalculationMocks(portfolio);

        var health = await CreateService().GetHealthAsync(portfolio.Id);

        Assert.Equal(PortfolioHealth.Healthy, health.PortfolioHealth);
        Assert.Equal(60m, health.UtilizationPercentage);
        Assert.Equal(55m, health.WorkloadPercentage);
    }

    [Fact]
    public void Validators_RequireMissionsAndName()
    {
        var createValidator = new CreatePortfolioRequestValidator();
        Assert.False(createValidator.Validate(new CreatePortfolioRequest()).IsValid);

        var updateValidator = new UpdatePortfolioRequestValidator();
        Assert.False(updateValidator.Validate(new UpdatePortfolioRequest()).IsValid);
    }

    private void SetupActiveMission(Guid missionId)
    {
        _missionRepository
            .Setup(repository => repository.GetByIdAsync(missionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mission
            {
                Id = missionId,
                MissionStatusId = PortfolioMissionEligibility.PlannedStatusId
            });
    }

    private void SetupCalculationMocks(Portfolio portfolio)
    {
        _portfolioRepository
            .Setup(repository => repository.GetByIdAsync(portfolio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);
        _capacityCalculator
            .Setup(service => service.GetSummaryAsync(It.IsAny<CapacityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacitySummaryResponse { OverallUtilizationPercentage = 60m });
        _workloadCalculator
            .Setup(service => service.GetSummaryAsync(It.IsAny<WorkloadQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadSummaryResponse { OverallWorkloadPercentage = 55m });
        _capacityHistory
            .Setup(service => service.AggregateAsync(It.IsAny<CapacityHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacityHistoryAggregateResponse { AverageUtilizationPercentage = 58m });
        _workloadHistory
            .Setup(service => service.AggregateAsync(It.IsAny<WorkloadHistoryQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkloadHistoryAggregateResponse { AverageWorkloadPercentage = 52m });
    }

    private static CreatePortfolioRequest CreateRequest(Guid missionId) =>
        new()
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Q3 Portfolio",
            PlanningPeriodStart = new DateOnly(2026, 7, 1),
            PlanningPeriodEnd = new DateOnly(2026, 9, 30),
            Missions = [new PortfolioMissionRequest { MissionId = missionId, Priority = 1 }]
        };

    private static Portfolio CreatePortfolioEntity() =>
        Portfolio.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Q3 Portfolio",
            null,
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 9, 30),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);
}
