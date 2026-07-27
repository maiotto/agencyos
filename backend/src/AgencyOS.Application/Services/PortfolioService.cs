using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class PortfolioService : IPortfolioService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IMissionRepository _missionRepository;
    private readonly IPlanningTemplateRepository _planningTemplateRepository;
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly ICapacityHistoryService _capacityHistoryService;
    private readonly IWorkloadHistoryService _workloadHistoryService;
    private readonly IAuditService _auditService;
    private readonly INotificationGenerationService _notificationGenerationService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        IMissionRepository missionRepository,
        IPlanningTemplateRepository planningTemplateRepository,
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        ICapacityHistoryService capacityHistoryService,
        IWorkloadHistoryService workloadHistoryService,
        IAuditService auditService,
        INotificationGenerationService notificationGenerationService,
        ILogger<PortfolioService> logger)
    {
        _portfolioRepository = portfolioRepository;
        _missionRepository = missionRepository;
        _planningTemplateRepository = planningTemplateRepository;
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _capacityHistoryService = capacityHistoryService;
        _workloadHistoryService = workloadHistoryService;
        _auditService = auditService;
        _notificationGenerationService = notificationGenerationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PortfolioResponse>> GetAllAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var portfolios = await _portfolioRepository.GetAllAsync(parameters, cancellationToken);
        return portfolios.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<PortfolioResponse>> FilterAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<PortfolioResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(portfolio);
    }

    public async Task<PortfolioResponse> CreateAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(request.CompanyId, request.Name, null, cancellationToken);
        await EnsurePlanningTemplateAsync(request.CompanyId, request.PlanningTemplateId, cancellationToken);

        var missionPairs = new List<(Guid MissionId, int Priority)>();
        foreach (var missionRequest in request.Missions)
        {
            await EnsureActiveMissionAsync(missionRequest.MissionId, cancellationToken);
            missionPairs.Add((missionRequest.MissionId, missionRequest.Priority));
        }

        Portfolio portfolio;
        try
        {
            portfolio = Portfolio.Create(
                request.CompanyId,
                request.Name,
                request.Description,
                request.PlanningPeriodStart,
                request.PlanningPeriodEnd,
                request.PlanningTemplateId,
                missionPairs,
                DateTimeOffset.UtcNow);
            portfolio.Validate();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _portfolioRepository.AddAsync(portfolio, cancellationToken);
        await _notificationGenerationService.GenerateSafeAsync(
            new NotificationGenerationRequest
            {
                CompanyId = created.CompanyId,
                Title = "Portfolio created",
                Message = $"Portfolio '{created.Name}' was created.",
                Category = NotificationCategory.Portfolio,
                Priority = NotificationPriority.Medium,
                SourceEntity = NotificationSourceEntities.Portfolio,
                SourceEntityId = created.Id
            },
            cancellationToken);
        _logger.LogInformation("Portfolio created {PortfolioId} Company={CompanyId}", created.Id, created.CompanyId);
        return MapToResponse(created);
    }

    public async Task<PortfolioResponse> UpdateAsync(
        Guid id,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        await EnsureUniqueNameAsync(portfolio.CompanyId, request.Name, portfolio.Id, cancellationToken);

        try
        {
            portfolio.Update(
                request.Name,
                request.Description,
                request.PlanningPeriodStart,
                request.PlanningPeriodEnd,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            portfolio.EnsureCanDelete();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _portfolioRepository.DeleteAsync(portfolio, cancellationToken);
        _logger.LogInformation("Portfolio deleted {PortfolioId}", id);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            portfolio.Activate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Portfolio,
                EntityId = portfolio.Id,
                EventType = AuditEventTypes.Activated,
                Action = "Portfolio.Activate",
                CompanyId = portfolio.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new { portfolio.Id, portfolio.Name, portfolio.Status })
            },
            cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        portfolio.Deactivate(DateTimeOffset.UtcNow);
        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Portfolio,
                EntityId = portfolio.Id,
                EventType = AuditEventTypes.Deactivated,
                Action = "Portfolio.Deactivate",
                CompanyId = portfolio.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new { portfolio.Id, portfolio.Name, portfolio.Status })
            },
            cancellationToken);
    }

    public async Task<PortfolioResponse> AssociateMissionAsync(
        Guid id,
        PortfolioMissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        await EnsureActiveMissionAsync(request.MissionId, cancellationToken);

        try
        {
            portfolio.AssociateMission(request.MissionId, request.Priority, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<PortfolioResponse> RemoveMissionAsync(
        Guid id,
        Guid missionId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            portfolio.RemoveMission(missionId, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<PortfolioResponse> AssignPlanningTemplateAsync(
        Guid id,
        AssignPortfolioPlanningTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        await EnsurePlanningTemplateAsync(portfolio.CompanyId, request.PlanningTemplateId, cancellationToken);

        try
        {
            portfolio.AssignPlanningTemplate(request.PlanningTemplateId, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<PortfolioSummaryResponse> GetSummaryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        var (capacity, workload, historicalCapacity, historicalWorkload) =
            await LoadCalculationContextAsync(portfolio, cancellationToken);

        return new PortfolioSummaryResponse
        {
            PortfolioId = portfolio.Id,
            Name = portfolio.Name,
            Status = portfolio.Status,
            PlanningPeriodStart = portfolio.PlanningPeriodStart,
            PlanningPeriodEnd = portfolio.PlanningPeriodEnd,
            MissionCount = portfolio.Missions.Count,
            PlanningTemplateId = portfolio.PlanningTemplateId,
            PortfolioHealth = portfolio.PortfolioHealth,
            Capacity = capacity,
            Workload = workload,
            HistoricalCapacity = historicalCapacity,
            HistoricalWorkload = historicalWorkload
        };
    }

    public async Task<PortfolioHealthResponse> GetHealthAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await CalculateHealthInternalAsync(id, persist: false, cancellationToken);
        return new PortfolioHealthResponse
        {
            PortfolioId = portfolio.Id,
            PortfolioHealth = portfolio.PortfolioHealth,
            UtilizationPercentage = TryReadUtilization(portfolio.CapacitySummary),
            WorkloadPercentage = TryReadWorkload(portfolio.WorkloadSummary),
            WarningPercentage = await ResolveWarningPercentageAsync(portfolio, cancellationToken),
            HistoricalAverageUtilizationPercentage = TryReadHistoricalUtilization(portfolio.HealthDetails),
            HistoricalAverageWorkloadPercentage = TryReadHistoricalWorkload(portfolio.HealthDetails),
            HealthDetails = portfolio.HealthDetails,
            CalculatedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<PortfolioResponse> CalculateCapacityAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        var capacity = await _capacityCalculatorService.GetSummaryAsync(
            new CapacityQueryParameters
            {
                PeriodStartDate = portfolio.PlanningPeriodStart,
                PeriodEndDate = portfolio.PlanningPeriodEnd
            },
            cancellationToken);

        try
        {
            portfolio.CalculateCapacity(JsonSerializer.Serialize(capacity, JsonOptions), DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Portfolio,
                EntityId = updated.Id,
                EventType = AuditEventTypes.Executed,
                Action = "Portfolio.CalculateCapacity",
                CompanyId = updated.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new { updated.Id, updated.Name, action = "CalculateCapacity" })
            },
            cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<PortfolioResponse> CalculateWorkloadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        var workload = await _workloadCalculatorService.GetSummaryAsync(
            new WorkloadQueryParameters
            {
                PeriodStartDate = portfolio.PlanningPeriodStart,
                PeriodEndDate = portfolio.PlanningPeriodEnd
            },
            cancellationToken);

        try
        {
            portfolio.CalculateWorkload(JsonSerializer.Serialize(workload, JsonOptions), DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Portfolio,
                EntityId = updated.Id,
                EventType = AuditEventTypes.Executed,
                Action = "Portfolio.CalculateWorkload",
                CompanyId = updated.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new { updated.Id, updated.Name, action = "CalculateWorkload" })
            },
            cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<PortfolioResponse> CalculateHealthAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await CalculateHealthInternalAsync(id, persist: true, cancellationToken);
        return MapToResponse(portfolio);
    }

    private async Task<Portfolio> CalculateHealthInternalAsync(
        Guid id,
        bool persist,
        CancellationToken cancellationToken)
    {
        var portfolio = await GetOrThrowAsync(id, cancellationToken);
        var (capacity, workload, historicalCapacity, historicalWorkload) =
            await LoadCalculationContextAsync(portfolio, cancellationToken);

        var warning = await ResolveWarningPercentageAsync(portfolio, cancellationToken);
        var details = JsonSerializer.Serialize(new
        {
            Capacity = capacity,
            Workload = workload,
            HistoricalCapacity = historicalCapacity,
            HistoricalWorkload = historicalWorkload,
            MissionIds = portfolio.Missions.Select(mission => mission.MissionId).ToList(),
            WarningPercentage = warning
        }, JsonOptions);

        try
        {
            portfolio.CalculateCapacity(JsonSerializer.Serialize(capacity, JsonOptions), DateTimeOffset.UtcNow);
            portfolio.CalculateWorkload(JsonSerializer.Serialize(workload, JsonOptions), DateTimeOffset.UtcNow);
            portfolio.CalculateHealth(
                capacity.OverallUtilizationPercentage,
                workload.OverallWorkloadPercentage,
                warning,
                details,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (persist)
        {
            return await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);
        }

        return portfolio;
    }

    private async Task<(
        CapacitySummaryResponse Capacity,
        WorkloadSummaryResponse Workload,
        CapacityHistoryAggregateResponse HistoricalCapacity,
        WorkloadHistoryAggregateResponse HistoricalWorkload)> LoadCalculationContextAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken)
    {
        var period = new CapacityQueryParameters
        {
            PeriodStartDate = portfolio.PlanningPeriodStart,
            PeriodEndDate = portfolio.PlanningPeriodEnd
        };
        var workloadPeriod = new WorkloadQueryParameters
        {
            PeriodStartDate = portfolio.PlanningPeriodStart,
            PeriodEndDate = portfolio.PlanningPeriodEnd
        };
        var historyQuery = new CapacityHistoryQueryParameters
        {
            CompanyId = portfolio.CompanyId,
            PeriodStart = portfolio.PlanningPeriodStart,
            PeriodEnd = portfolio.PlanningPeriodEnd
        };
        var workloadHistoryQuery = new WorkloadHistoryQueryParameters
        {
            CompanyId = portfolio.CompanyId,
            PeriodStart = portfolio.PlanningPeriodStart,
            PeriodEnd = portfolio.PlanningPeriodEnd
        };

        var capacity = await _capacityCalculatorService.GetSummaryAsync(period, cancellationToken);
        var workload = await _workloadCalculatorService.GetSummaryAsync(workloadPeriod, cancellationToken);
        var historicalCapacity = await _capacityHistoryService.AggregateAsync(historyQuery, cancellationToken);
        var historicalWorkload = await _workloadHistoryService.AggregateAsync(workloadHistoryQuery, cancellationToken);

        return (capacity, workload, historicalCapacity, historicalWorkload);
    }

    private async Task EnsureUniqueNameAsync(
        Guid companyId,
        string name,
        Guid? excludePortfolioId,
        CancellationToken cancellationToken)
    {
        if (await _portfolioRepository.ExistsByCompanyAndNameAsync(
                companyId,
                name,
                excludePortfolioId,
                cancellationToken))
        {
            throw new ConflictException("Portfolio Name must be unique within the Company.");
        }
    }

    private async Task EnsureActiveMissionAsync(Guid missionId, CancellationToken cancellationToken)
    {
        var mission = await _missionRepository.GetByIdAsync(missionId, cancellationToken);
        if (mission is null)
        {
            throw new NotFoundException($"Mission with id '{missionId}' was not found.");
        }

        if (!PortfolioMissionEligibility.IsActiveMissionStatus(mission.MissionStatusId))
        {
            throw new BusinessRuleException("Only Active Missions may be associated.");
        }
    }

    private async Task EnsurePlanningTemplateAsync(
        Guid companyId,
        Guid? planningTemplateId,
        CancellationToken cancellationToken)
    {
        if (!planningTemplateId.HasValue)
        {
            return;
        }

        var template = await _planningTemplateRepository.GetByIdAsync(
            planningTemplateId.Value,
            cancellationToken);

        if (template is null)
        {
            throw new NotFoundException(
                $"Planning template with id '{planningTemplateId}' was not found.");
        }

        if (template.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                "Planning Template must belong to the same Company as the Portfolio.");
        }
    }

    private async Task<decimal?> ResolveWarningPercentageAsync(
        Portfolio portfolio,
        CancellationToken cancellationToken)
    {
        if (!portfolio.PlanningTemplateId.HasValue)
        {
            return 85m;
        }

        var template = await _planningTemplateRepository.GetByIdAsync(
            portfolio.PlanningTemplateId.Value,
            cancellationToken);

        return template?.UtilizationWarningPercentage ?? 85m;
    }

    private async Task<Portfolio> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(id, cancellationToken);
        if (portfolio is null)
        {
            throw new NotFoundException($"Portfolio with id '{id}' was not found.");
        }

        return portfolio;
    }

    private static PortfolioResponse MapToResponse(Portfolio portfolio) =>
        new()
        {
            Id = portfolio.Id,
            CompanyId = portfolio.CompanyId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            PlanningTemplateId = portfolio.PlanningTemplateId,
            PlanningPeriodStart = portfolio.PlanningPeriodStart,
            PlanningPeriodEnd = portfolio.PlanningPeriodEnd,
            CapacitySummary = portfolio.CapacitySummary,
            WorkloadSummary = portfolio.WorkloadSummary,
            PortfolioHealth = portfolio.PortfolioHealth,
            HealthDetails = portfolio.HealthDetails,
            CreatedAt = portfolio.CreatedAt,
            UpdatedAt = portfolio.UpdatedAt,
            Missions = portfolio.Missions
                .OrderBy(mission => mission.Priority)
                .ThenBy(mission => mission.MissionId)
                .Select(mission => new PortfolioMissionResponse
                {
                    Id = mission.Id,
                    MissionId = mission.MissionId,
                    Priority = mission.Priority,
                    IncludedAt = mission.IncludedAt
                })
                .ToList()
        };

    private static decimal? TryReadUtilization(string? json) =>
        TryReadDecimal(json, "overallUtilizationPercentage");

    private static decimal? TryReadWorkload(string? json) =>
        TryReadDecimal(json, "overallWorkloadPercentage");

    private static decimal? TryReadHistoricalUtilization(string? json) =>
        TryReadNestedDecimal(json, "historicalCapacity", "averageUtilizationPercentage");

    private static decimal? TryReadHistoricalWorkload(string? json) =>
        TryReadNestedDecimal(json, "historicalWorkload", "averageWorkloadPercentage");

    private static decimal? TryReadDecimal(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(propertyName, out var property)
                && property.TryGetDecimal(out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static decimal? TryReadNestedDecimal(string? json, string parentName, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(parentName, out var parent)
                && parent.ValueKind == JsonValueKind.Object
                && parent.TryGetProperty(propertyName, out var property)
                && property.TryGetDecimal(out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
