using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class CapacityHistoryService : ICapacityHistoryService
{
    private readonly ICapacityHistoryRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<CapacityHistoryService> _logger;

    public CapacityHistoryService(
        ICapacityHistoryRepository repository,
        IAuditService auditService,
        ILogger<CapacityHistoryService> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task PersistCompletedCalculationAsync(
        CapacityResponse capacity,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var calculationDate = DateTimeOffset.UtcNow;
        var history = CapacityHistoryMappings.CreateHistory(capacity, companyId, calculationDate);
        await _repository.AddAsync(history, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.CapacityHistory,
                EntityId = history.Id,
                EntityVersion = history.CalculationVersion,
                EventType = AuditEventTypes.Calculated,
                Action = "Capacity.Calculate",
                CompanyId = companyId,
                UserId = "capacity-engine",
                UserName = "capacity-engine",
                Source = AuditSources.System,
                CurrentState = AuditService.SerializeState(new
                {
                    history.Id,
                    history.ExecutionResourceId,
                    history.CapacityHours,
                    history.PeriodStart,
                    history.PeriodEnd
                })
            },
            cancellationToken);

        _logger.LogInformation(
            "Persisted capacity history {HistoryId} for execution resource {ExecutionResourceId} period {PeriodStart}-{PeriodEnd} version {CalculationVersion}",
            history.Id,
            history.ExecutionResourceId,
            history.PeriodStart,
            history.PeriodEnd,
            history.CalculationVersion);
    }

    public async Task PersistCompletedCalculationsAsync(
        IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)> capacities,
        CancellationToken cancellationToken = default)
    {
        if (capacities.Count == 0)
        {
            return;
        }

        var calculationDate = DateTimeOffset.UtcNow;
        var histories = capacities
            .Select(item => CapacityHistoryMappings.CreateHistory(item.Capacity, item.CompanyId, calculationDate))
            .ToList();

        await _repository.AddRangeAsync(histories, cancellationToken);

        foreach (var history in histories)
        {
            await _auditService.RecordSafeAsync(
                new AuditEventWriteRequest
                {
                    EntityType = AuditEntityTypes.CapacityHistory,
                    EntityId = history.Id,
                    EntityVersion = history.CalculationVersion,
                    EventType = AuditEventTypes.Calculated,
                    Action = "Capacity.CalculateBatch",
                    CompanyId = history.CompanyId,
                    UserId = "capacity-engine",
                    UserName = "capacity-engine",
                    Source = AuditSources.System,
                    CurrentState = AuditService.SerializeState(new
                    {
                        history.Id,
                        history.ExecutionResourceId,
                        history.CapacityHours
                    })
                },
                cancellationToken);
        }

        _logger.LogInformation(
            "Persisted {HistoryCount} capacity history records for calculation version {CalculationVersion}",
            histories.Count,
            Domain.Entities.CapacityHistoryVersions.Current);
    }

    public async Task<IReadOnlyList<CapacityHistoryResponse>> QueryAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.QueryAsync(parameters, cancellationToken);
        return histories.Select(CapacityHistoryMappings.ToResponse).ToList();
    }

    public async Task<CapacityHistoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var history = await _repository.GetByIdAsync(id, cancellationToken);
        if (history is null)
        {
            throw new NotFoundException($"Capacity history with id '{id}' was not found.");
        }

        return CapacityHistoryMappings.ToResponse(history);
    }

    public async Task<IReadOnlyList<CapacityHistoryResponse>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.GetByExecutionResourceIdAsync(
            executionResourceId,
            parameters,
            cancellationToken);

        return histories.Select(CapacityHistoryMappings.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<CapacityHistoryResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.GetByCompanyIdAsync(companyId, parameters, cancellationToken);
        return histories.Select(CapacityHistoryMappings.ToResponse).ToList();
    }

    public async Task<CapacityHistoryCompareResponse> CompareAsync(
        CapacityHistoryCompareQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var left = await _repository.GetByIdAsync(parameters.LeftHistoryId, cancellationToken);
        if (left is null)
        {
            throw new NotFoundException(
                $"Capacity history with id '{parameters.LeftHistoryId}' was not found.");
        }

        var right = await _repository.GetByIdAsync(parameters.RightHistoryId, cancellationToken);
        if (right is null)
        {
            throw new NotFoundException(
                $"Capacity history with id '{parameters.RightHistoryId}' was not found.");
        }

        return CapacityHistoryMappings.ToCompare(left, right);
    }

    public async Task<CapacityHistoryAggregateResponse> AggregateAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var histories = await _repository.QueryAsync(parameters, cancellationToken);
        return CapacityHistoryMappings.ToAggregate(histories, parameters);
    }
}
