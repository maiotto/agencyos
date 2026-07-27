using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class WorkloadHistoryService : IWorkloadHistoryService
{
    private readonly IWorkloadHistoryRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<WorkloadHistoryService> _logger;

    public WorkloadHistoryService(
        IWorkloadHistoryRepository repository,
        IAuditService auditService,
        ILogger<WorkloadHistoryService> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task PersistCompletedCalculationAsync(
        WorkloadHistoryPersistModel model,
        CancellationToken cancellationToken = default)
    {
        var calculationDate = DateTimeOffset.UtcNow;
        var history = WorkloadHistoryMappings.CreateHistory(model, calculationDate);
        await _repository.AddAsync(history, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.WorkloadHistory,
                EntityId = history.Id,
                EntityVersion = history.CalculationVersion,
                EventType = AuditEventTypes.Calculated,
                Action = "Workload.Calculate",
                CompanyId = history.CompanyId,
                UserId = "workload-engine",
                UserName = "workload-engine",
                Source = AuditSources.System,
                CurrentState = AuditService.SerializeState(new
                {
                    history.Id,
                    history.ExecutionResourceId,
                    history.AllocatedHours,
                    history.PeriodStart,
                    history.PeriodEnd
                })
            },
            cancellationToken);

        _logger.LogInformation(
            "Persisted workload history {HistoryId} for execution resource {ExecutionResourceId} period {PeriodStart}-{PeriodEnd} version {CalculationVersion}",
            history.Id,
            history.ExecutionResourceId,
            history.PeriodStart,
            history.PeriodEnd,
            history.CalculationVersion);
    }

    public async Task PersistCompletedCalculationsAsync(
        IReadOnlyList<WorkloadHistoryPersistModel> models,
        CancellationToken cancellationToken = default)
    {
        if (models.Count == 0)
        {
            return;
        }

        var calculationDate = DateTimeOffset.UtcNow;
        var histories = models
            .Select(model => WorkloadHistoryMappings.CreateHistory(model, calculationDate))
            .ToList();

        await _repository.AddRangeAsync(histories, cancellationToken);

        foreach (var history in histories)
        {
            await _auditService.RecordSafeAsync(
                new AuditEventWriteRequest
                {
                    EntityType = AuditEntityTypes.WorkloadHistory,
                    EntityId = history.Id,
                    EntityVersion = history.CalculationVersion,
                    EventType = AuditEventTypes.Calculated,
                    Action = "Workload.CalculateBatch",
                    CompanyId = history.CompanyId,
                    UserId = "workload-engine",
                    UserName = "workload-engine",
                    Source = AuditSources.System,
                    CurrentState = AuditService.SerializeState(new
                    {
                        history.Id,
                        history.ExecutionResourceId,
                        history.AllocatedHours
                    })
                },
                cancellationToken);
        }

        _logger.LogInformation(
            "Persisted {HistoryCount} workload history records for calculation version {CalculationVersion}",
            histories.Count,
            Domain.Entities.WorkloadHistoryVersions.Current);
    }

    public async Task<IReadOnlyList<WorkloadHistoryResponse>> QueryAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.QueryAsync(parameters, cancellationToken);
        return histories.Select(WorkloadHistoryMappings.ToResponse).ToList();
    }

    public async Task<WorkloadHistoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var history = await _repository.GetByIdAsync(id, cancellationToken);
        if (history is null)
        {
            throw new NotFoundException($"Workload history with id '{id}' was not found.");
        }

        return WorkloadHistoryMappings.ToResponse(history);
    }

    public async Task<IReadOnlyList<WorkloadHistoryResponse>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.GetByExecutionResourceIdAsync(
            executionResourceId,
            parameters,
            cancellationToken);

        return histories.Select(WorkloadHistoryMappings.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<WorkloadHistoryResponse>> GetByCompanyIdAsync(
        Guid companyId,
        WorkloadHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.GetByCompanyIdAsync(companyId, parameters, cancellationToken);
        return histories.Select(WorkloadHistoryMappings.ToResponse).ToList();
    }

    public async Task<WorkloadHistoryCompareResponse> CompareAsync(
        WorkloadHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var left = await _repository.GetByIdAsync(parameters.LeftHistoryId, cancellationToken);
        if (left is null)
        {
            throw new NotFoundException(
                $"Workload history with id '{parameters.LeftHistoryId}' was not found.");
        }

        var right = await _repository.GetByIdAsync(parameters.RightHistoryId, cancellationToken);
        if (right is null)
        {
            throw new NotFoundException(
                $"Workload history with id '{parameters.RightHistoryId}' was not found.");
        }

        return WorkloadHistoryMappings.ToCompare(left, right);
    }

    public async Task<WorkloadHistoryAggregateResponse> AggregateAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.QueryAsync(parameters, cancellationToken);
        return WorkloadHistoryMappings.ToAggregate(histories, parameters);
    }

    public async Task<WorkloadHistoryTrendResponse> TrendsAsync(
        WorkloadHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.QueryAsync(parameters, cancellationToken);
        return WorkloadHistoryMappings.ToTrend(histories, parameters);
    }
}
