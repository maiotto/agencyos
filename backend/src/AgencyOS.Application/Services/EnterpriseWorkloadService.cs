using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Builds an enterprise-wide Workload view across a set of Portfolios (US-405 / BR-2304).
/// Reuses each Portfolio's stored WorkloadSummary snapshot (never recalculated) and, when a
/// planning period is supplied, complements it with a single live
/// <c>IWorkloadCalculatorService.GetSummaryAsync</c> aggregate for context.
/// </summary>
public class EnterpriseWorkloadService : IEnterpriseWorkloadService
{
    private readonly IWorkloadCalculatorService _workloadCalculatorService;

    public EnterpriseWorkloadService(IWorkloadCalculatorService workloadCalculatorService)
    {
        _workloadCalculatorService = workloadCalculatorService;
    }

    public async Task<EnterpriseWorkloadResponse> BuildAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        CancellationToken cancellationToken = default)
    {
        var items = portfolios
            .Select(portfolio => new EnterpriseWorkloadPortfolioItem
            {
                PortfolioId = portfolio.Id,
                Name = portfolio.Name,
                WorkloadPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                    portfolio.WorkloadSummary,
                    "overallWorkloadPercentage")
            })
            .ToList();

        var values = items
            .Where(item => item.WorkloadPercentage.HasValue)
            .Select(item => item.WorkloadPercentage!.Value)
            .ToList();

        var response = new EnterpriseWorkloadResponse
        {
            PortfolioCount = portfolios.Count,
            Portfolios = items,
            AverageWorkloadPercentage = values.Count > 0 ? Round(values.Average()) : null,
            MinWorkloadPercentage = values.Count > 0 ? Round(values.Min()) : null,
            MaxWorkloadPercentage = values.Count > 0 ? Round(values.Max()) : null
        };

        if (periodStart.HasValue && periodEnd.HasValue)
        {
            response.LiveSummary = await _workloadCalculatorService.GetSummaryAsync(
                new WorkloadQueryParameters
                {
                    PeriodStartDate = periodStart.Value,
                    PeriodEndDate = periodEnd.Value
                },
                cancellationToken);
            response.WorkloadEngineUsed = true;
        }

        return response;
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
