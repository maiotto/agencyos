namespace AgencyOS.Application.DTOs;

/// <summary>
/// Advisory disclaimers shared by every Cross-Portfolio Planning response (US-405 / BR-2301..BR-2310).
/// Cross-Portfolio Planning never mutates a Portfolio and every response requires human approval.
/// </summary>
public static class CrossPortfolioPlanningConstants
{
    public const string AdvisoryDisclaimer =
        "Advisory only — human approval required. Simulations never modify Portfolios.";
}

/// <summary>
/// Query filters for the Cross-Portfolio Planning overview and scenario list (US-405 / BR-2307).
/// Read-only; never triggers recalculation of Capacity/Workload engines.
/// </summary>
public class CrossPortfolioPlanningQueryParameters
{
    /// <summary>Defaults to the active <c>ICompanyContext</c> Company, then <c>AgencyOSCompanies.DefaultCompanyId</c>.</summary>
    public Guid? CompanyId { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }
}

/// <summary>
/// Query filters for endpoints that operate against an explicit Portfolio selection
/// (conflicts/balance). <c>PortfolioIds</c> accepts either repeated query keys or a
/// comma-separated value (default ASP.NET Core collection binding for simple types).
/// </summary>
public class CrossPortfolioSelectionQueryParameters : CrossPortfolioPlanningQueryParameters
{
    public List<Guid> PortfolioIds { get; set; } = [];
}

public class SimulateCrossPortfolioPlanRequest
{
    public Guid? CompanyId { get; set; }

    public List<Guid> PortfolioIds { get; set; } = [];

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public string? ScenarioName { get; set; }
}

public class CompareCrossPortfolioScenariosRequest
{
    public Guid? CompanyId { get; set; }

    public Guid LeftScenarioId { get; set; }

    public Guid RightScenarioId { get; set; }
}

public class PortfolioParticipationMissionItem
{
    public Guid MissionId { get; set; }

    /// <summary>Portfolio mission priority, preserved as-is — never reordered (BR-2306).</summary>
    public int Priority { get; set; }
}

/// <summary>
/// A single Portfolio's read-only participation snapshot within a Cross-Portfolio Plan
/// (US-405 / BR-2306). Missions are always ordered by ascending Priority.
/// </summary>
public class PortfolioParticipationResponse
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public HealthIndicator Health { get; set; } = new();

    public int MissionCount { get; set; }

    public IReadOnlyList<PortfolioParticipationMissionItem> Missions { get; set; } = [];

    /// <summary>Parsed from <c>Portfolio.CapacitySummary</c> JSON (BR-2303).</summary>
    public decimal? UtilizationPercentage { get; set; }

    /// <summary>Parsed from <c>Portfolio.WorkloadSummary</c> JSON (BR-2304).</summary>
    public decimal? WorkloadPercentage { get; set; }

    public string DrillDownPath { get; set; } = string.Empty;
}

/// <summary>
/// Read-only Cross-Portfolio Planning overview: active Portfolios in the resolved Company,
/// presented as participation candidates for a simulation (US-405).
/// </summary>
public class CrossPortfolioOverviewResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public int PortfolioCount { get; set; }

    public IReadOnlyList<PortfolioParticipationResponse> Portfolios { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } = CrossPortfolioPlanningConstants.AdvisoryDisclaimer;
}

public class EnterpriseCapacityPortfolioItem
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal? UtilizationPercentage { get; set; }
}

/// <summary>
/// Enterprise-wide Capacity view built from the selected Portfolios' stored CapacitySummary
/// snapshots (BR-2303), optionally complemented by a live Capacity Engine aggregate for the
/// planning period (<see cref="CapacityEngineUsed"/>). Never recalculates Portfolio Capacity.
/// </summary>
public class EnterpriseCapacityResponse
{
    public int PortfolioCount { get; set; }

    public decimal? AverageUtilizationPercentage { get; set; }

    public decimal? MinUtilizationPercentage { get; set; }

    public decimal? MaxUtilizationPercentage { get; set; }

    public IReadOnlyList<EnterpriseCapacityPortfolioItem> Portfolios { get; set; } = [];

    /// <summary>True when a live Capacity Engine aggregate was retrieved for the planning period.</summary>
    public bool CapacityEngineUsed { get; set; }

    public CapacitySummaryResponse? LiveSummary { get; set; }
}

public class EnterpriseWorkloadPortfolioItem
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal? WorkloadPercentage { get; set; }
}

/// <summary>
/// Enterprise-wide Workload view built from the selected Portfolios' stored WorkloadSummary
/// snapshots (BR-2304), optionally complemented by a live Workload Engine aggregate for the
/// planning period (<see cref="WorkloadEngineUsed"/>). Never recalculates Portfolio Workload.
/// </summary>
public class EnterpriseWorkloadResponse
{
    public int PortfolioCount { get; set; }

    public decimal? AverageWorkloadPercentage { get; set; }

    public decimal? MinWorkloadPercentage { get; set; }

    public decimal? MaxWorkloadPercentage { get; set; }

    public IReadOnlyList<EnterpriseWorkloadPortfolioItem> Portfolios { get; set; } = [];

    /// <summary>True when a live Workload Engine aggregate was retrieved for the planning period.</summary>
    public bool WorkloadEngineUsed { get; set; }

    public WorkloadSummaryResponse? LiveSummary { get; set; }
}

/// <summary>A Mission assigned to more than one selected Portfolio (BR-2305).</summary>
public class PortfolioConflictItem
{
    public Guid MissionId { get; set; }

    public IReadOnlyList<Guid> PortfolioIds { get; set; } = [];

    public IReadOnlyList<string> PortfolioNames { get; set; } = [];
}

/// <summary>Condensed projection of an <see cref="AllocationConflictResponse"/> for cross-portfolio reporting.</summary>
public class ResourceConflictItem
{
    public Guid ConflictId { get; set; }

    public string ConflictType { get; set; } = string.Empty;

    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceName { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resource conflicts (BR-2305, delegated to <c>IAllocationConflictDetectionService</c>) and
/// Portfolio-level Mission overlap conflicts across a Cross-Portfolio Plan selection. Purely
/// informative — conflicts are never auto-resolved.
/// </summary>
public class ConflictSummaryResponse
{
    public int PortfolioConflictCount { get; set; }

    public IReadOnlyList<PortfolioConflictItem> PortfolioConflicts { get; set; } = [];

    public int ResourceConflictCount { get; set; }

    public IReadOnlyList<ResourceConflictItem> ResourceConflicts { get; set; } = [];

    /// <summary>Overall severity band: None, Low, Medium, High, or Critical.</summary>
    public string Severity { get; set; } = "None";
}

/// <summary>
/// A single advisory balancing suggestion (US-405 / BR-2306). Purely informational — Cross-Portfolio
/// Planning never executes reallocation, hiring, or financial optimization changes.
/// </summary>
public class BalancingRecommendationResponse
{
    public Guid? SourcePortfolioId { get; set; }

    public string? SourcePortfolioName { get; set; }

    public Guid? TargetPortfolioId { get; set; }

    public string? TargetPortfolioName { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public string Rationale { get; set; } = string.Empty;
}

/// <summary>
/// Enterprise Capacity/Workload balance view plus advisory balancing recommendations for a
/// selected set of Portfolios (US-405 / BR-2303, BR-2304, BR-2306).
/// </summary>
public class CrossPortfolioBalanceResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public EnterpriseCapacityResponse Capacity { get; set; } = new();

    public EnterpriseWorkloadResponse Workload { get; set; } = new();

    public IReadOnlyList<PortfolioParticipationResponse> Portfolios { get; set; } = [];

    public IReadOnlyList<BalancingRecommendationResponse> Recommendations { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } = CrossPortfolioPlanningConstants.AdvisoryDisclaimer;
}

/// <summary>
/// A full, temporary Cross-Portfolio Plan scenario (US-405 / DEC-405-001). Never persisted to a
/// database table — held in-memory by <c>ICrossPortfolioScenarioStore</c> for the process lifetime.
/// The durable record of a simulation is the Audit trail (BR-2309).
/// </summary>
public class CrossPortfolioScenarioResponse
{
    public Guid ScenarioId { get; set; }

    public Guid CompanyId { get; set; }

    public string? ScenarioName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public IReadOnlyList<Guid> PortfolioIds { get; set; } = [];

    public IReadOnlyList<PortfolioParticipationResponse> Portfolios { get; set; } = [];

    public EnterpriseCapacityResponse Capacity { get; set; } = new();

    public EnterpriseWorkloadResponse Workload { get; set; } = new();

    public ConflictSummaryResponse Conflicts { get; set; } = new();

    public IReadOnlyList<BalancingRecommendationResponse> Recommendations { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public bool AdvisoryOnly { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } = CrossPortfolioPlanningConstants.AdvisoryDisclaimer;
}

public class CrossPortfolioScenarioSummaryItem
{
    public Guid ScenarioId { get; set; }

    public string? ScenarioName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public int PortfolioCount { get; set; }

    public decimal? AverageUtilizationPercentage { get; set; }

    public decimal? AverageWorkloadPercentage { get; set; }

    public int ConflictCount { get; set; }
}

/// <summary>In-memory scenario listing for a Company (US-405 / DEC-405-001). Scenarios are temporary.</summary>
public class CrossPortfolioScenarioListResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public IReadOnlyList<CrossPortfolioScenarioSummaryItem> Scenarios { get; set; } = [];
}

/// <summary>
/// Response returned by <c>POST /cross-portfolio-planning/simulate</c> (US-405 / BR-2308, BR-2309).
/// Every simulation is audited via <c>IAuditService.RecordSafeAsync</c> and requires human approval —
/// nothing here is executed automatically.
/// </summary>
public class SimulationSummaryResponse
{
    public Guid ScenarioId { get; set; }

    public Guid CompanyId { get; set; }

    public DateTimeOffset SimulatedAt { get; set; }

    public CrossPortfolioScenarioResponse Scenario { get; set; } = new();

    public bool RequiresHumanApproval { get; set; } = true;

    public bool AdvisoryOnly { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } = CrossPortfolioPlanningConstants.AdvisoryDisclaimer;
}

public class ScenarioComparisonFieldDiff
{
    public string Field { get; set; } = string.Empty;

    public string? LeftValue { get; set; }

    public string? RightValue { get; set; }

    public decimal? Delta { get; set; }
}

/// <summary>Side-by-side comparison of two in-memory Cross-Portfolio Plan scenarios (US-405).</summary>
public class ScenarioComparisonResponse
{
    public Guid CompanyId { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public CrossPortfolioScenarioResponse Left { get; set; } = new();

    public CrossPortfolioScenarioResponse Right { get; set; } = new();

    public IReadOnlyList<ScenarioComparisonFieldDiff> FieldDiffs { get; set; } = [];

    public bool RequiresHumanApproval { get; set; } = true;

    public string AdvisoryDisclaimer { get; set; } = CrossPortfolioPlanningConstants.AdvisoryDisclaimer;
}

/// <summary>
/// In-memory record held by <c>ICrossPortfolioScenarioStore</c> (US-405 / DEC-405-001).
/// Scenarios are TEMPORARY: no database table backs this type, and the store may expire or
/// evict records at any time. The durable record of a simulation is the Audit trail (BR-2309).
/// </summary>
public class CrossPortfolioScenarioRecord
{
    public Guid ScenarioId { get; set; }

    public Guid CompanyId { get; set; }

    public string? ScenarioName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public CrossPortfolioScenarioResponse Scenario { get; set; } = new();
}
