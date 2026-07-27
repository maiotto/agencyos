using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Builds an enterprise-wide Capacity view across a set of Portfolios (US-405 / BR-2303).
/// Reuses each Portfolio's stored CapacitySummary snapshot (never recalculated) and, when a
/// planning period is supplied, complements it with a single live
/// <c>ICapacityCalculatorService.GetSummaryAsync</c> aggregate for context.
/// </summary>
public class EnterpriseCapacityService : IEnterpriseCapacityService
{
    private readonly ICapacityCalculatorService _capacityCalculatorService;

    public EnterpriseCapacityService(ICapacityCalculatorService capacityCalculatorService)
    {
        _capacityCalculatorService = capacityCalculatorService;
    }

    public async Task<EnterpriseCapacityResponse> BuildAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        CancellationToken cancellationToken = default)
    {
        var items = portfolios
            .Select(portfolio => new EnterpriseCapacityPortfolioItem
            {
                PortfolioId = portfolio.Id,
                Name = portfolio.Name,
                UtilizationPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                    portfolio.CapacitySummary,
                    "overallUtilizationPercentage")
            })
            .ToList();

        var values = items
            .Where(item => item.UtilizationPercentage.HasValue)
            .Select(item => item.UtilizationPercentage!.Value)
            .ToList();

        var response = new EnterpriseCapacityResponse
        {
            PortfolioCount = portfolios.Count,
            Portfolios = items,
            AverageUtilizationPercentage = values.Count > 0 ? Round(values.Average()) : null,
            MinUtilizationPercentage = values.Count > 0 ? Round(values.Min()) : null,
            MaxUtilizationPercentage = values.Count > 0 ? Round(values.Max()) : null
        };

        if (periodStart.HasValue && periodEnd.HasValue)
        {
            response.LiveSummary = await _capacityCalculatorService.GetSummaryAsync(
                new CapacityQueryParameters
                {
                    PeriodStartDate = periodStart.Value,
                    PeriodEndDate = periodEnd.Value
                },
                cancellationToken);
            response.CapacityEngineUsed = true;
        }

        return response;
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
