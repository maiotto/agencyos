using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class EnterpriseCapacityServiceTests
{
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();

    private EnterpriseCapacityService CreateService() => new(_capacityCalculatorService.Object);

    [Fact]
    public async Task BuildAsync_ComputesAverageMinMax_AcrossPortfolios()
    {
        var portfolios = new List<Portfolio>
        {
            CreatePortfolio(utilization: 60m),
            CreatePortfolio(utilization: 80m),
            CreatePortfolio(utilization: 40m)
        };

        var service = CreateService();

        var result = await service.BuildAsync(portfolios, null, null);

        Assert.Equal(3, result.PortfolioCount);
        Assert.Equal(60m, result.AverageUtilizationPercentage);
        Assert.Equal(40m, result.MinUtilizationPercentage);
        Assert.Equal(80m, result.MaxUtilizationPercentage);
        Assert.Equal(3, result.Portfolios.Count);
    }

    [Fact]
    public async Task BuildAsync_DoesNotUseCapacityEngine_WhenPeriodNotProvided()
    {
        var portfolios = new List<Portfolio> { CreatePortfolio(utilization: 50m) };
        var service = CreateService();

        var result = await service.BuildAsync(portfolios, null, null);

        Assert.False(result.CapacityEngineUsed);
        Assert.Null(result.LiveSummary);
        _capacityCalculatorService.Verify(
            calculator => calculator.GetSummaryAsync(It.IsAny<CapacityQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BuildAsync_UsesLiveCapacityEngine_WhenPeriodProvided()
    {
        var portfolios = new List<Portfolio> { CreatePortfolio(utilization: 50m) };
        var periodStart = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodEnd = periodStart.AddDays(30);

        var liveSummary = new CapacitySummaryResponse
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            OverallUtilizationPercentage = 75m
        };
        _capacityCalculatorService
            .Setup(calculator => calculator.GetSummaryAsync(It.IsAny<CapacityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(liveSummary);

        var service = CreateService();

        var result = await service.BuildAsync(portfolios, periodStart, periodEnd);

        Assert.True(result.CapacityEngineUsed);
        Assert.NotNull(result.LiveSummary);
        Assert.Equal(75m, result.LiveSummary!.OverallUtilizationPercentage);
    }

    [Fact]
    public async Task BuildAsync_ReturnsNullAverages_WhenNoPortfoliosHaveCapacitySummary()
    {
        var portfolio = Portfolio.Create(
            AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

        var service = CreateService();

        var result = await service.BuildAsync([portfolio], null, null);

        Assert.Null(result.AverageUtilizationPercentage);
        Assert.Null(result.MinUtilizationPercentage);
        Assert.Null(result.MaxUtilizationPercentage);
        Assert.Single(result.Portfolios);
        Assert.Null(result.Portfolios[0].UtilizationPercentage);
    }

    [Fact]
    public async Task BuildAsync_ReturnsEmpty_WhenNoPortfoliosSelected()
    {
        var service = CreateService();

        var result = await service.BuildAsync([], null, null);

        Assert.Equal(0, result.PortfolioCount);
        Assert.Empty(result.Portfolios);
        Assert.Null(result.AverageUtilizationPercentage);
    }

    private static Portfolio CreatePortfolio(decimal utilization)
    {
        var portfolio = Portfolio.Create(
            AgencyOSCompanies.DefaultCompanyId,
            $"Portfolio {Guid.NewGuid():N}",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

        portfolio.CalculateCapacity(
            $"{{\"overallUtilizationPercentage\":{utilization.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}",
            DateTimeOffset.UtcNow);

        return portfolio;
    }
}
