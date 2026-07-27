namespace AgencyOS.Application.DTOs;

public class PortfolioQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? Status { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public Guid? MissionId { get; set; }

    public string? Search { get; set; }

    public DateOnly? PeriodStart { get; set; }

    public DateOnly? PeriodEnd { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class PortfolioMissionRequest
{
    public Guid MissionId { get; set; }

    public int Priority { get; set; } = 1;
}

public class CreatePortfolioRequest
{
    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public IReadOnlyList<PortfolioMissionRequest> Missions { get; set; } = [];
}

public class UpdatePortfolioRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }
}

public class AssignPortfolioPlanningTemplateRequest
{
    public Guid? PlanningTemplateId { get; set; }
}

public class PortfolioMissionResponse
{
    public Guid Id { get; set; }

    public Guid MissionId { get; set; }

    public int Priority { get; set; }

    public DateTimeOffset IncludedAt { get; set; }
}

public class PortfolioResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? PlanningTemplateId { get; set; }

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }

    public string? CapacitySummary { get; set; }

    public string? WorkloadSummary { get; set; }

    public string PortfolioHealth { get; set; } = string.Empty;

    public string? HealthDetails { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IReadOnlyList<PortfolioMissionResponse> Missions { get; set; } = [];
}

public class PortfolioSummaryResponse
{
    public Guid PortfolioId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly PlanningPeriodStart { get; set; }

    public DateOnly PlanningPeriodEnd { get; set; }

    public int MissionCount { get; set; }

    public Guid? PlanningTemplateId { get; set; }

    public string PortfolioHealth { get; set; } = string.Empty;

    public CapacitySummaryResponse? Capacity { get; set; }

    public WorkloadSummaryResponse? Workload { get; set; }

    public CapacityHistoryAggregateResponse? HistoricalCapacity { get; set; }

    public WorkloadHistoryAggregateResponse? HistoricalWorkload { get; set; }
}

public class PortfolioHealthResponse
{
    public Guid PortfolioId { get; set; }

    public string PortfolioHealth { get; set; } = string.Empty;

    public decimal? UtilizationPercentage { get; set; }

    public decimal? WorkloadPercentage { get; set; }

    public decimal? WarningPercentage { get; set; }

    public decimal? HistoricalAverageUtilizationPercentage { get; set; }

    public decimal? HistoricalAverageWorkloadPercentage { get; set; }

    public string? HealthDetails { get; set; }

    public DateTimeOffset CalculatedAt { get; set; }
}
