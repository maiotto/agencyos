using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class EnterpriseWorkloadServiceTests
{
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();

    private EnterpriseWorkloadService CreateService() => new(_workloadCalculatorService.Object);

    [Fact]
    public async Task BuildAsync_ComputesAverageMinMax_AcrossPortfolios()
    {
        var portfolios = new List<Portfolio>
        {
            CreatePortfolio(workload: 30m),
            CreatePortfolio(workload: 90m),
            CreatePortfolio(workload: 60m)
        };

        var service = CreateService();

        var result = await service.BuildAsync(portfolios, null, null);

        Assert.Equal(3, result.PortfolioCount);
        Assert.Equal(60m, result.AverageWorkloadPercentage);
        Assert.Equal(30m, result.MinWorkloadPercentage);
        Assert.Equal(90m, result.MaxWorkloadPercentage);
        Assert.Equal(3, result.Portfolios.Count);
    }

    [Fact]
    public async Task BuildAsync_DoesNotUseWorkloadEngine_WhenPeriodNotProvided()
    {
        var portfolios = new List<Portfolio> { CreatePortfolio(workload: 50m) };
        var service = CreateService();

        var result = await service.BuildAsync(portfolios, null, null);

        Assert.False(result.WorkloadEngineUsed);
        Assert.Null(result.LiveSummary);
        _workloadCalculatorService.Verify(
            calculator => calculator.GetSummaryAsync(It.IsAny<WorkloadQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BuildAsync_UsesLiveWorkloadEngine_WhenPeriodProvided()
    {
        var portfolios = new List<Portfolio> { CreatePortfolio(workload: 50m) };
        var periodStart = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var periodEnd = periodStart.AddDays(30);

        var liveSummary = new WorkloadSummaryResponse
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            OverallWorkloadPercentage = 65m
        };
        _workloadCalculatorService
            .Setup(calculator => calculator.GetSummaryAsync(It.IsAny<WorkloadQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(liveSummary);

        var service = CreateService();

        var result = await service.BuildAsync(portfolios, periodStart, periodEnd);

        Assert.True(result.WorkloadEngineUsed);
        Assert.NotNull(result.LiveSummary);
        Assert.Equal(65m, result.LiveSummary!.OverallWorkloadPercentage);
    }

    [Fact]
    public async Task BuildAsync_ReturnsNullAverages_WhenNoPortfoliosHaveWorkloadSummary()
    {
        var portfolio = Portfolio.Create(
            AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

        var service = CreateService();

        var result = await service.BuildAsync([portfolio], null, null);

        Assert.Null(result.AverageWorkloadPercentage);
        Assert.Null(result.MinWorkloadPercentage);
        Assert.Null(result.MaxWorkloadPercentage);
        Assert.Single(result.Portfolios);
        Assert.Null(result.Portfolios[0].WorkloadPercentage);
    }

    private static Portfolio CreatePortfolio(decimal workload)
    {
        var portfolio = Portfolio.Create(
            AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime),
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(30).UtcDateTime),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

        portfolio.CalculateWorkload(
            $"{{\"overallWorkloadPercentage\":{workload.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);

        return portfolio;
    }
}
