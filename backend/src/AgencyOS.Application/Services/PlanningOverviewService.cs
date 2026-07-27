using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Planning Workspace overview aggregation (US-502 / BR-2501..BR-2510). Aggregates
/// company-scoped counts/summaries directly from existing Planning Template, Portfolio,
/// Capacity/Workload History, and Cross-Portfolio Planning services (DEC-502-001). Never
/// recalculates an engine for storage and never persists a Scenario.
/// </summary>
public class PlanningOverviewService : IPlanningOverviewService
{
    private readonly IPlanningTemplateService _planningTemplateService;
    private readonly IPortfolioService _portfolioService;
    private readonly ICapacityHistoryService _capacityHistoryService;
    private readonly IWorkloadHistoryService _workloadHistoryService;
    private readonly ICrossPortfolioPlanningService _crossPortfolioPlanningService;
    private readonly IDashboardHealthCalculationService _healthCalculationService;

    public PlanningOverviewService(
        IPlanningTemplateService planningTemplateService,
        IPortfolioService portfolioService,
        ICapacityHistoryService capacityHistoryService,
        IWorkloadHistoryService workloadHistoryService,
        ICrossPortfolioPlanningService crossPortfolioPlanningService,
        IDashboardHealthCalculationService healthCalculationService)
    {
        _planningTemplateService = planningTemplateService;
        _portfolioService = portfolioService;
        _capacityHistoryService = capacityHistoryService;
        _workloadHistoryService = workloadHistoryService;
        _crossPortfolioPlanningService = crossPortfolioPlanningService;
        _healthCalculationService = healthCalculationService;
    }

    public async Task<PlanningOverviewResponse> GetOverviewAsync(
        Guid companyId,
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var templatesTask = _planningTemplateService.GetAllAsync(
            new PlanningTemplateQueryParameters { CompanyId = companyId },
            cancellationToken);

        var portfoliosTask = _portfolioService.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var capacityTask = _capacityHistoryService.AggregateAsync(
            new CapacityHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var workloadTask = _workloadHistoryService.AggregateAsync(
            new WorkloadHistoryQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var scenariosTask = _crossPortfolioPlanningService.GetScenariosAsync(
            new CrossPortfolioPlanningQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd,
                From = parameters.From,
                To = parameters.To
            },
            cancellationToken);

        await Task.WhenAll(templatesTask, portfoliosTask, capacityTask, workloadTask, scenariosTask);

        var templates = templatesTask.Result;
        var portfolios = portfoliosTask.Result;
        var capacity = capacityTask.Result;
        var workload = workloadTask.Result;
        var scenarios = scenariosTask.Result;

        var activeTemplateCount = templates.Count(template => PlanningTemplateStatus.IsActive(template.Status));
        var activePortfolioCount = portfolios.Count(portfolio =>
            string.Equals(portfolio.Status, PortfolioStatus.Active, StringComparison.OrdinalIgnoreCase));

        var overallHealth = _healthCalculationService.CalculateOverall(
            portfolios.Select(portfolio => portfolio.PortfolioHealth).ToList());

        return new PlanningOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = parameters.From,
            To = parameters.To,
            PeriodStart = parameters.PeriodStart,
            PeriodEnd = parameters.PeriodEnd,
            InactiveTemplateCount = templates.Count(template => PlanningTemplateStatus.IsInactive(template.Status)),
            PortfolioHealthBreakdown = BuildStatusBreakdown(
                portfolios.Select(portfolio => portfolio.PortfolioHealth)),
            CapacityHistoryRecordCount = capacity.RecordCount,
            WorkloadHistoryRecordCount = workload.RecordCount,
            Kpis = new PlanningKpiSummaryResponse
            {
                TemplateCount = templates.Count,
                ActiveTemplateCount = activeTemplateCount,
                PortfolioCount = portfolios.Count,
                ActivePortfolioCount = activePortfolioCount,
                ScenarioCount = scenarios.Scenarios.Count,
                AverageUtilizationPercentage = capacity.AverageUtilizationPercentage,
                AverageWorkloadPercentage = workload.AverageWorkloadPercentage,
                OverallHealth = overallHealth
            }
        };
    }

    private static IReadOnlyList<StatusCountItem> BuildStatusBreakdown(IEnumerable<string> values)
    {
        return values
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StatusCountItem { Status = group.Key, Count = group.Count() })
            .OrderBy(item => item.Status, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
