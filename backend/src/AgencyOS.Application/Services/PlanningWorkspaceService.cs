using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Planning Workspace orchestration (US-502 / BR-2501..BR-2510; DEC-502-001). A
/// read-only orchestration facade over existing Planning Template, Portfolio, Capacity/Workload
/// History, and Cross-Portfolio Planning services. Resolves the active Company, defaults the
/// reporting window (trailing 30 days), and delegates every section to the existing engine that
/// already owns it — never calls a Portfolio/Template Update/Add/Delete, never recalculates an
/// engine for storage, and never persists a Scenario. "Execute Capacity/Workload Planning" and
/// "Launch Planning Calculations" are exposed purely as navigation Planning Actions.
/// </summary>
public class PlanningWorkspaceService : IPlanningWorkspaceService
{
    private const int DefaultWindowDays = 30;
    private const int RecentHistoryLimit = 10;

    private readonly IPlanningOverviewService _planningOverviewService;
    private readonly IPlanningNavigationService _planningNavigationService;
    private readonly IPlanningHistoryService _planningHistoryService;
    private readonly IPlanningTemplateService _planningTemplateService;
    private readonly IPortfolioService _portfolioService;
    private readonly ICapacityHistoryService _capacityHistoryService;
    private readonly IWorkloadHistoryService _workloadHistoryService;
    private readonly ICrossPortfolioPlanningService _crossPortfolioPlanningService;
    private readonly IDashboardHealthCalculationService _healthCalculationService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;

    public PlanningWorkspaceService(
        IPlanningOverviewService planningOverviewService,
        IPlanningNavigationService planningNavigationService,
        IPlanningHistoryService planningHistoryService,
        IPlanningTemplateService planningTemplateService,
        IPortfolioService portfolioService,
        ICapacityHistoryService capacityHistoryService,
        IWorkloadHistoryService workloadHistoryService,
        ICrossPortfolioPlanningService crossPortfolioPlanningService,
        IDashboardHealthCalculationService healthCalculationService,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext)
    {
        _planningOverviewService = planningOverviewService;
        _planningNavigationService = planningNavigationService;
        _planningHistoryService = planningHistoryService;
        _planningTemplateService = planningTemplateService;
        _portfolioService = portfolioService;
        _capacityHistoryService = capacityHistoryService;
        _workloadHistoryService = workloadHistoryService;
        _crossPortfolioPlanningService = crossPortfolioPlanningService;
        _healthCalculationService = healthCalculationService;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
    }

    public async Task<PlanningWorkspaceResponse> GetWorkspaceAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);

        var overview = await _planningOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
        var templates = await BuildTemplatesSectionAsync(companyId, cancellationToken);
        var capacity = await BuildCapacitySectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
        var workload = await BuildWorkloadSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
        var portfolios = await BuildPortfoliosSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
        var history = await _planningHistoryService.GetHistoryAsync(
            companyId,
            resolved.From,
            resolved.To,
            cancellationToken);
        var scenarios = await BuildScenariosSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            resolved.From,
            resolved.To,
            cancellationToken);
        var navigation = _planningNavigationService.GetNavigation(companyId);

        var kpis = new PlanningKpiSummaryResponse
        {
            TemplateCount = overview.Kpis.TemplateCount,
            ActiveTemplateCount = overview.Kpis.ActiveTemplateCount,
            PortfolioCount = overview.Kpis.PortfolioCount,
            ActivePortfolioCount = overview.Kpis.ActivePortfolioCount,
            ScenarioCount = overview.Kpis.ScenarioCount,
            AverageUtilizationPercentage = overview.Kpis.AverageUtilizationPercentage,
            AverageWorkloadPercentage = overview.Kpis.AverageWorkloadPercentage,
            OverallHealth = overview.Kpis.OverallHealth,
            PlanningHistoryEventCount = history.Items.Count
        };

        return new PlanningWorkspaceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            From = resolved.From,
            To = resolved.To,
            PeriodStart = resolved.PeriodStart,
            PeriodEnd = resolved.PeriodEnd,
            Kpis = kpis,
            Overview = overview,
            Templates = templates,
            Capacity = capacity,
            Workload = workload,
            Portfolios = portfolios,
            History = history,
            Scenarios = scenarios,
            Navigation = navigation
        };
    }

    public async Task<PlanningOverviewResponse> GetOverviewAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _planningOverviewService.GetOverviewAsync(companyId, resolved, cancellationToken);
    }

    public async Task<PlanningTemplatesSectionResponse> GetTemplatesAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        return await BuildTemplatesSectionAsync(companyId, cancellationToken);
    }

    public async Task<PlanningCapacitySectionResponse> GetCapacityAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await BuildCapacitySectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
    }

    public async Task<PlanningWorkloadSectionResponse> GetWorkloadAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await BuildWorkloadSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
    }

    public async Task<PlanningPortfoliosSectionResponse> GetPortfoliosAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await BuildPortfoliosSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            cancellationToken);
    }

    public async Task<PlanningHistoryResponse> GetHistoryAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await _planningHistoryService.GetHistoryAsync(companyId, resolved.From, resolved.To, cancellationToken);
    }

    public async Task<PlanningScenariosSectionResponse> GetScenariosAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters, cancellationToken);
        var resolved = ResolveWindow(parameters);
        return await BuildScenariosSectionAsync(
            companyId,
            resolved.PeriodStart!.Value,
            resolved.PeriodEnd!.Value,
            resolved.From,
            resolved.To,
            cancellationToken);
    }

    private async Task<PlanningTemplatesSectionResponse> BuildTemplatesSectionAsync(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var templates = await _planningTemplateService.GetAllAsync(
            new PlanningTemplateQueryParameters { CompanyId = companyId },
            cancellationToken);

        var cards = templates
            .Select(template => new PlanningTemplateCardResponse
            {
                Id = template.Id,
                Name = template.Name,
                Status = template.Status,
                IsActive = PlanningTemplateStatus.IsActive(template.Status),
                ResourceAvailabilityStrategy = template.ResourceAvailabilityStrategy,
                DefaultPlanningWindowDays = template.DefaultPlanningWindowDays,
                DrillDownPath = $"/planning-templates/{template.Id}"
            })
            .ToList();

        return new PlanningTemplatesSectionResponse
        {
            CompanyId = companyId,
            Templates = cards,
            ManageAction = new PlanningActionResponse
            {
                Key = "manage-templates",
                Label = "Manage Templates",
                Category = "Templates",
                DrillDownPath = $"/planning-templates?companyId={companyId}",
                Description = "Create, edit, activate, and deactivate Planning Templates (BR-2506)."
            },
            ApplyAction = new PlanningActionResponse
            {
                Key = "apply-template",
                Label = "Apply Template",
                Category = "Templates",
                DrillDownPath = $"/planning-templates?companyId={companyId}",
                Description = "Select a Template from the list, then apply it via its own /planning-templates/{id}/apply action."
            }
        };
    }

    private async Task<PlanningCapacitySectionResponse> BuildCapacitySectionAsync(
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        var queryParameters = new CapacityHistoryQueryParameters
        {
            CompanyId = companyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        var aggregate = await _capacityHistoryService.AggregateAsync(queryParameters, cancellationToken);
        var recent = (await _capacityHistoryService.QueryAsync(queryParameters, cancellationToken))
            .OrderByDescending(history => history.CalculationDate)
            .Take(RecentHistoryLimit)
            .ToList();

        return new PlanningCapacitySectionResponse
        {
            CompanyId = companyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            RecordCount = aggregate.RecordCount,
            TotalCapacityHours = aggregate.TotalCapacityHours,
            TotalAllocatedHours = aggregate.TotalAllocatedHours,
            AverageUtilizationPercentage = aggregate.AverageUtilizationPercentage,
            UtilizationHealth = _healthCalculationService.CalculateFromUtilization(
                aggregate.AverageUtilizationPercentage,
                0m),
            RecentHistory = recent,
            Action = new PlanningActionResponse
            {
                Key = "capacity-history",
                Label = "Capacity History",
                Category = "Capacity",
                DrillDownPath = $"/capacity/history?companyId={companyId}",
                Description = "Browse immutable Capacity History records — never recalculated by the Workspace."
            }
        };
    }

    private async Task<PlanningWorkloadSectionResponse> BuildWorkloadSectionAsync(
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        var queryParameters = new WorkloadHistoryQueryParameters
        {
            CompanyId = companyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        var aggregate = await _workloadHistoryService.AggregateAsync(queryParameters, cancellationToken);
        var recent = (await _workloadHistoryService.QueryAsync(queryParameters, cancellationToken))
            .OrderByDescending(history => history.CalculationDate)
            .Take(RecentHistoryLimit)
            .ToList();

        return new PlanningWorkloadSectionResponse
        {
            CompanyId = companyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            RecordCount = aggregate.RecordCount,
            TotalAllocatedHours = aggregate.TotalAllocatedHours,
            TotalCapacityHours = aggregate.TotalCapacityHours,
            AverageWorkloadPercentage = aggregate.AverageWorkloadPercentage,
            WorkloadHealth = _healthCalculationService.CalculateFromUtilization(
                0m,
                aggregate.AverageWorkloadPercentage),
            RecentHistory = recent,
            Action = new PlanningActionResponse
            {
                Key = "workload-history",
                Label = "Workload History",
                Category = "Workload",
                DrillDownPath = $"/workload/history?companyId={companyId}",
                Description = "Browse immutable Workload History records — never recalculated by the Workspace."
            }
        };
    }

    private async Task<PlanningPortfoliosSectionResponse> BuildPortfoliosSectionAsync(
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        var portfolios = await _portfolioService.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            },
            cancellationToken);

        var cards = portfolios
            .Select(portfolio => new PlanningPortfolioCardResponse
            {
                Id = portfolio.Id,
                Name = portfolio.Name,
                Status = portfolio.Status,
                PortfolioHealth = portfolio.PortfolioHealth,
                PlanningPeriodStart = portfolio.PlanningPeriodStart,
                PlanningPeriodEnd = portfolio.PlanningPeriodEnd,
                UtilizationPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                    portfolio.CapacitySummary,
                    "overallUtilizationPercentage"),
                WorkloadPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                    portfolio.WorkloadSummary,
                    "overallWorkloadPercentage"),
                PlanningTemplateId = portfolio.PlanningTemplateId,
                DrillDownPath = $"/portfolios/{portfolio.Id}"
            })
            .ToList();

        return new PlanningPortfoliosSectionResponse
        {
            CompanyId = companyId,
            Portfolios = cards,
            Action = new PlanningActionResponse
            {
                Key = "portfolios",
                Label = "Portfolios",
                Category = "Portfolio",
                DrillDownPath = $"/portfolios?companyId={companyId}",
                Description = "Manage Portfolios and their Mission assignments."
            }
        };
    }

    private async Task<PlanningScenariosSectionResponse> BuildScenariosSectionAsync(
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var scenarioList = await _crossPortfolioPlanningService.GetScenariosAsync(
            new CrossPortfolioPlanningQueryParameters
            {
                CompanyId = companyId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                From = from,
                To = to
            },
            cancellationToken);

        var scenarios = scenarioList.Scenarios
            .Select(scenario => new PlanningScenarioSummaryResponse
            {
                ScenarioId = scenario.ScenarioId,
                ScenarioName = scenario.ScenarioName,
                CreatedAt = scenario.CreatedAt,
                PortfolioCount = scenario.PortfolioCount,
                AverageUtilizationPercentage = scenario.AverageUtilizationPercentage,
                AverageWorkloadPercentage = scenario.AverageWorkloadPercentage,
                ConflictCount = scenario.ConflictCount
            })
            .ToList();

        return new PlanningScenariosSectionResponse
        {
            CompanyId = companyId,
            GeneratedAt = scenarioList.GeneratedAt,
            Scenarios = scenarios,
            ComparisonAction = new PlanningActionResponse
            {
                Key = "cross-portfolio-planning",
                Label = "Cross-Portfolio Planning",
                Category = "Scenarios",
                DrillDownPath = $"/cross-portfolio-planning?companyId={companyId}",
                Description = "Advisory-only, temporary multi-Portfolio simulation and scenario comparison. Requires human approval (BR-2507).",
                IsAdvisory = true
            }
        };
    }

    /// <summary>Resolves CompanyId per DEC-502-001 and validates it references an existing Company.</summary>
    private async Task<Guid> ResolveAndValidateCompanyIdAsync(
        PlanningWorkspaceQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var companyId = parameters.CompanyId ?? _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;

        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{companyId}' was not found.");
        }

        return companyId;
    }

    /// <summary>
    /// Resolves the From/To reporting window (trailing 30 days by default) and derives
    /// PeriodStart/PeriodEnd from the same calendar window when not explicitly supplied.
    /// </summary>
    private static PlanningWorkspaceQueryParameters ResolveWindow(PlanningWorkspaceQueryParameters parameters)
    {
        var to = parameters.To ?? DateTimeOffset.UtcNow;
        var from = parameters.From ?? to.AddDays(-DefaultWindowDays);

        var periodEnd = parameters.PeriodEnd ?? DateOnly.FromDateTime(to.UtcDateTime);
        var periodStart = parameters.PeriodStart ?? DateOnly.FromDateTime(from.UtcDateTime);

        return new PlanningWorkspaceQueryParameters
        {
            CompanyId = parameters.CompanyId,
            From = from,
            To = to,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };
    }
}
