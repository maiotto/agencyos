using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only, advisory Cross-Portfolio Planning orchestration (US-405 / BR-2301..BR-2310 / DEC-405-001).
/// Resolves the active Company, loads Portfolios read-only (never <c>UpdateAsync</c>/<c>AddAsync</c>/
/// <c>DeleteAsync</c>), and delegates to the Enterprise Capacity/Workload, Conflict Detection, and
/// Balancing services. Scenarios are TEMPORARY in-memory records held by
/// <see cref="ICrossPortfolioScenarioStore"/> — the durable record of every simulation is the Audit
/// trail written via <see cref="IAuditService.RecordSafeAsync"/> (BR-2309). Every response requires
/// human approval; nothing here is executed automatically (BR-2308).
/// </summary>
public class CrossPortfolioPlanningService : ICrossPortfolioPlanningService
{
    private const int DefaultPeriodDays = 90;

    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyContext _companyContext;
    private readonly IDashboardHealthCalculationService _healthCalculationService;
    private readonly IEnterpriseCapacityService _enterpriseCapacityService;
    private readonly IEnterpriseWorkloadService _enterpriseWorkloadService;
    private readonly ICrossPortfolioConflictDetectionService _conflictDetectionService;
    private readonly ICrossPortfolioBalancingService _balancingService;
    private readonly ICrossPortfolioScenarioComparisonService _scenarioComparisonService;
    private readonly ICrossPortfolioScenarioStore _scenarioStore;
    private readonly IAuditService _auditService;

    public CrossPortfolioPlanningService(
        IPortfolioRepository portfolioRepository,
        ICompanyRepository companyRepository,
        ICompanyContext companyContext,
        IDashboardHealthCalculationService healthCalculationService,
        IEnterpriseCapacityService enterpriseCapacityService,
        IEnterpriseWorkloadService enterpriseWorkloadService,
        ICrossPortfolioConflictDetectionService conflictDetectionService,
        ICrossPortfolioBalancingService balancingService,
        ICrossPortfolioScenarioComparisonService scenarioComparisonService,
        ICrossPortfolioScenarioStore scenarioStore,
        IAuditService auditService)
    {
        _portfolioRepository = portfolioRepository;
        _companyRepository = companyRepository;
        _companyContext = companyContext;
        _healthCalculationService = healthCalculationService;
        _enterpriseCapacityService = enterpriseCapacityService;
        _enterpriseWorkloadService = enterpriseWorkloadService;
        _conflictDetectionService = conflictDetectionService;
        _balancingService = balancingService;
        _scenarioComparisonService = scenarioComparisonService;
        _scenarioStore = scenarioStore;
        _auditService = auditService;
    }

    public async Task<CrossPortfolioOverviewResponse> GetOverviewAsync(
        CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);

        var portfolios = await _portfolioRepository.GetAllAsync(
            new PortfolioQueryParameters
            {
                CompanyId = companyId,
                Status = PortfolioStatus.Active,
                PeriodStart = parameters.PeriodStart,
                PeriodEnd = parameters.PeriodEnd
            },
            cancellationToken);

        var participations = portfolios.Select(BuildParticipation).ToList();

        return new CrossPortfolioOverviewResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            PortfolioCount = participations.Count,
            Portfolios = participations,
            RequiresHumanApproval = true
        };
    }

    public async Task<CrossPortfolioScenarioListResponse> GetScenariosAsync(
        CrossPortfolioPlanningQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);

        var scenarios = _scenarioStore.GetAll(companyId)
            .Select(record => new CrossPortfolioScenarioSummaryItem
            {
                ScenarioId = record.ScenarioId,
                ScenarioName = record.ScenarioName,
                CreatedAt = record.CreatedAt,
                PortfolioCount = record.Scenario.Portfolios.Count,
                AverageUtilizationPercentage = record.Scenario.Capacity.AverageUtilizationPercentage,
                AverageWorkloadPercentage = record.Scenario.Workload.AverageWorkloadPercentage,
                ConflictCount = record.Scenario.Conflicts.PortfolioConflictCount
                    + record.Scenario.Conflicts.ResourceConflictCount
            })
            .ToList();

        return new CrossPortfolioScenarioListResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            Scenarios = scenarios
        };
    }

    public async Task<ConflictSummaryResponse> GetConflictsAsync(
        CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var portfolios = await LoadPortfoliosAsync(companyId, parameters.PortfolioIds, cancellationToken);
        var (periodStart, periodEnd) = ResolvePeriod(parameters.PeriodStart, parameters.PeriodEnd);

        return await _conflictDetectionService.DetectAsync(portfolios, periodStart, periodEnd, cancellationToken);
    }

    public async Task<CrossPortfolioBalanceResponse> GetBalanceAsync(
        CrossPortfolioSelectionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companyId = await ResolveAndValidateCompanyIdAsync(parameters.CompanyId, cancellationToken);
        var portfolios = await LoadPortfoliosAsync(companyId, parameters.PortfolioIds, cancellationToken);
        var (periodStart, periodEnd) = ResolvePeriod(parameters.PeriodStart, parameters.PeriodEnd);

        var participations = portfolios.Select(BuildParticipation).ToList();
        var capacity = await _enterpriseCapacityService.BuildAsync(portfolios, periodStart, periodEnd, cancellationToken);
        var workload = await _enterpriseWorkloadService.BuildAsync(portfolios, periodStart, periodEnd, cancellationToken);
        var recommendations = _balancingService.BuildRecommendations(participations);

        return new CrossPortfolioBalanceResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Capacity = capacity,
            Workload = workload,
            Portfolios = participations,
            Recommendations = recommendations,
            RequiresHumanApproval = true
        };
    }

    public async Task<SimulationSummaryResponse> SimulateAsync(
        SimulateCrossPortfolioPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var companyId = await ResolveAndValidateCompanyIdAsync(request.CompanyId, cancellationToken);

        var distinctPortfolioIds = (request.PortfolioIds ?? []).Distinct().ToList();
        if (distinctPortfolioIds.Count < 2)
        {
            throw new BusinessRuleException(
                "At least two distinct Portfolios are required to simulate a Cross-Portfolio Plan.");
        }

        var portfolios = await LoadPortfoliosAsync(companyId, distinctPortfolioIds, cancellationToken);
        var (periodStart, periodEnd) = ResolvePeriod(request.PeriodStart, request.PeriodEnd);

        var participations = portfolios.Select(BuildParticipation).ToList();
        var capacity = await _enterpriseCapacityService.BuildAsync(portfolios, periodStart, periodEnd, cancellationToken);
        var workload = await _enterpriseWorkloadService.BuildAsync(portfolios, periodStart, periodEnd, cancellationToken);
        var conflicts = await _conflictDetectionService.DetectAsync(portfolios, periodStart, periodEnd, cancellationToken);
        var recommendations = _balancingService.BuildRecommendations(participations);

        var scenarioId = Guid.NewGuid();
        var simulatedAt = DateTimeOffset.UtcNow;

        var scenario = new CrossPortfolioScenarioResponse
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            ScenarioName = request.ScenarioName,
            CreatedAt = simulatedAt,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PortfolioIds = portfolios.Select(portfolio => portfolio.Id).ToList(),
            Portfolios = participations,
            Capacity = capacity,
            Workload = workload,
            Conflicts = conflicts,
            Recommendations = recommendations,
            RequiresHumanApproval = true,
            AdvisoryOnly = true
        };

        _scenarioStore.Add(new CrossPortfolioScenarioRecord
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            ScenarioName = request.ScenarioName,
            CreatedAt = simulatedAt,
            Scenario = scenario
        });

        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.CrossPortfolioPlan,
                EntityId = scenarioId,
                EventType = AuditEventTypes.Simulated,
                Action = "CrossPortfolioPlanning.Simulate",
                CompanyId = companyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new
                {
                    scenarioId,
                    portfolioIds = scenario.PortfolioIds,
                    conflictCount = conflicts.PortfolioConflictCount + conflicts.ResourceConflictCount,
                    averageUtilizationPercentage = capacity.AverageUtilizationPercentage,
                    averageWorkloadPercentage = workload.AverageWorkloadPercentage
                }),
                Metadata = AuditService.SerializeState(new { RequiresHumanApproval = true, AdvisoryOnly = true })
            },
            cancellationToken);

        return new SimulationSummaryResponse
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            SimulatedAt = simulatedAt,
            Scenario = scenario,
            RequiresHumanApproval = true,
            AdvisoryOnly = true
        };
    }

    public async Task<ScenarioComparisonResponse> CompareAsync(
        CompareCrossPortfolioScenariosRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var companyId = await ResolveAndValidateCompanyIdAsync(request.CompanyId, cancellationToken);

        var left = GetScenarioRecordOrThrow(request.LeftScenarioId, companyId);
        var right = GetScenarioRecordOrThrow(request.RightScenarioId, companyId);

        return _scenarioComparisonService.Compare(left, right);
    }

    private CrossPortfolioScenarioRecord GetScenarioRecordOrThrow(Guid scenarioId, Guid companyId)
    {
        var record = _scenarioStore.Get(scenarioId);
        if (record is null)
        {
            throw new NotFoundException($"Cross-Portfolio Plan scenario with id '{scenarioId}' was not found.");
        }

        if (record.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                $"Scenario '{scenarioId}' does not belong to the resolved Company.");
        }

        return record;
    }

    private async Task<IReadOnlyList<Portfolio>> LoadPortfoliosAsync(
        Guid companyId,
        IReadOnlyCollection<Guid> portfolioIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = portfolioIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            throw new BusinessRuleException("At least one PortfolioId must be provided.");
        }

        var portfolios = new List<Portfolio>();
        foreach (var portfolioId in distinctIds)
        {
            var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, cancellationToken);
            if (portfolio is null)
            {
                throw new NotFoundException($"Portfolio with id '{portfolioId}' was not found.");
            }

            if (portfolio.CompanyId != companyId)
            {
                throw new BusinessRuleException(
                    $"Portfolio '{portfolioId}' does not belong to the resolved Company.");
            }

            portfolios.Add(portfolio);
        }

        return portfolios;
    }

    private PortfolioParticipationResponse BuildParticipation(Portfolio portfolio)
    {
        var health = _healthCalculationService.CalculateOverall([portfolio.PortfolioHealth]);

        return new PortfolioParticipationResponse
        {
            PortfolioId = portfolio.Id,
            Name = portfolio.Name,
            Status = portfolio.Status,
            Health = health,
            MissionCount = portfolio.Missions.Count,
            Missions = portfolio.Missions
                .OrderBy(mission => mission.Priority)
                .ThenBy(mission => mission.MissionId)
                .Select(mission => new PortfolioParticipationMissionItem
                {
                    MissionId = mission.MissionId,
                    Priority = mission.Priority
                })
                .ToList(),
            UtilizationPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                portfolio.CapacitySummary,
                "overallUtilizationPercentage"),
            WorkloadPercentage = PortfolioAnalyticsSnapshotBuilder.TryReadDecimal(
                portfolio.WorkloadSummary,
                "overallWorkloadPercentage"),
            DrillDownPath = $"/portfolios/{portfolio.Id}"
        };
    }

    private async Task<Guid> ResolveAndValidateCompanyIdAsync(Guid? companyId, CancellationToken cancellationToken)
    {
        var resolvedCompanyId = companyId ?? _companyContext.CompanyId ?? AgencyOSCompanies.DefaultCompanyId;

        var company = await _companyRepository.GetByIdAsync(resolvedCompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{resolvedCompanyId}' was not found.");
        }

        return resolvedCompanyId;
    }

    private static (DateOnly Start, DateOnly End) ResolvePeriod(DateOnly? periodStart, DateOnly? periodEnd)
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        var start = periodStart ?? today;
        var end = periodEnd ?? start.AddDays(DefaultPeriodDays);
        return (start, end);
    }
}
