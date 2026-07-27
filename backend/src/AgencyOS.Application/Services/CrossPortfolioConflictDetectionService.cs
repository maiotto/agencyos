using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Detects Portfolio-level Mission overlap and Resource conflicts across a Cross-Portfolio Plan
/// selection (US-405 / BR-2305). Resource conflicts are delegated entirely to the existing
/// <c>IAllocationConflictDetectionService</c> — never re-implemented here. Purely advisory: no
/// conflict is ever auto-resolved.
/// </summary>
public class CrossPortfolioConflictDetectionService : ICrossPortfolioConflictDetectionService
{
    private const int TopResourceConflictCount = 10;

    private readonly IAllocationConflictDetectionService _allocationConflictDetectionService;

    public CrossPortfolioConflictDetectionService(
        IAllocationConflictDetectionService allocationConflictDetectionService)
    {
        _allocationConflictDetectionService = allocationConflictDetectionService;
    }

    public async Task<ConflictSummaryResponse> DetectAsync(
        IReadOnlyList<Portfolio> portfolios,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        var portfolioConflicts = BuildPortfolioConflicts(portfolios);

        var parameters = new AllocationConflictQueryParameters
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd
        };

        var summary = await _allocationConflictDetectionService.GetSummaryAsync(parameters, cancellationToken);
        var allConflicts = await _allocationConflictDetectionService.GetAllAsync(parameters, cancellationToken);

        var resourceConflicts = allConflicts
            .Take(TopResourceConflictCount)
            .Select(conflict => new ResourceConflictItem
            {
                ConflictId = conflict.ConflictId,
                ConflictType = conflict.ConflictType,
                ExecutionResourceId = conflict.ExecutionResourceId,
                ExecutionResourceName = conflict.ExecutionResourceName,
                Severity = conflict.Severity,
                Description = conflict.Description
            })
            .ToList();

        return new ConflictSummaryResponse
        {
            PortfolioConflictCount = portfolioConflicts.Count,
            PortfolioConflicts = portfolioConflicts,
            ResourceConflictCount = summary.TotalConflictCount,
            ResourceConflicts = resourceConflicts,
            Severity = DetermineOverallSeverity(summary, portfolioConflicts.Count)
        };
    }

    private static IReadOnlyList<PortfolioConflictItem> BuildPortfolioConflicts(
        IReadOnlyList<Portfolio> portfolios)
    {
        return portfolios
            .SelectMany(portfolio => portfolio.Missions.Select(mission => new
            {
                mission.MissionId,
                Portfolio = portfolio
            }))
            .GroupBy(entry => entry.MissionId)
            .Where(group => group.Select(entry => entry.Portfolio.Id).Distinct().Count() > 1)
            .Select(group => new PortfolioConflictItem
            {
                MissionId = group.Key,
                PortfolioIds = group.Select(entry => entry.Portfolio.Id).Distinct().ToList(),
                PortfolioNames = group.Select(entry => entry.Portfolio.Name).Distinct().ToList()
            })
            .OrderBy(item => item.MissionId)
            .ToList();
    }

    private static string DetermineOverallSeverity(
        AllocationConflictSummaryResponse summary,
        int portfolioConflictCount)
    {
        if (summary.CriticalConflictCount > 0)
        {
            return AllocationConflictCalculation.Severity.Critical;
        }

        if (summary.HighConflictCount > 0)
        {
            return AllocationConflictCalculation.Severity.High;
        }

        if (summary.MediumConflictCount > 0 || portfolioConflictCount > 0)
        {
            return AllocationConflictCalculation.Severity.Medium;
        }

        if (summary.LowConflictCount > 0)
        {
            return AllocationConflictCalculation.Severity.Low;
        }

        return "None";
    }
}
