using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class RecommendationHistoryService : IRecommendationHistoryService
{
    private readonly IRecommendationHistoryRepository _repository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly ILogger<RecommendationHistoryService> _logger;

    public RecommendationHistoryService(
        IRecommendationHistoryRepository repository,
        IRecommendationRepository recommendationRepository,
        ILogger<RecommendationHistoryService> logger)
    {
        _repository = repository;
        _recommendationRepository = recommendationRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RecommendationHistoryResponse>> GetAllAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.QueryAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<RecommendationHistoryResponse>> FilterAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<RecommendationHistoryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var history = await _repository.GetByIdAsync(id, cancellationToken);
        if (history is null)
        {
            throw new NotFoundException($"Recommendation history with id '{id}' was not found.");
        }

        return MapToResponse(history);
    }

    public async Task<IReadOnlyList<RecommendationHistoryResponse>> GetVersionsAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecommendationExistsAsync(recommendationId, cancellationToken);
        var versions = await _repository.GetVersionsByRecommendationIdAsync(
            recommendationId,
            cancellationToken);
        return versions.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<RecommendationHistoryTimelineEntryResponse>> GetTimelineAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecommendationExistsAsync(recommendationId, cancellationToken);
        var timeline = await _repository.GetTimelineByRecommendationIdAsync(
            recommendationId,
            cancellationToken);

        return timeline.Select(history => new RecommendationHistoryTimelineEntryResponse
        {
            Id = history.Id,
            EventType = history.EventType,
            RecommendationVersion = history.RecommendationVersion,
            RecommendationStatus = history.RecommendationStatus,
            WorkflowStatus = history.WorkflowStatus,
            WorkflowId = history.WorkflowId,
            Approver = history.Approver,
            ApprovalComment = history.ApprovalComment,
            CreatedBy = history.CreatedBy,
            CreatedAt = history.CreatedAt,
            Title = history.Title
        }).ToList();
    }

    public async Task PersistFromRecommendationAsync(
        Recommendation recommendation,
        string eventType,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var history = RecommendationHistory.FromRecommendation(
            recommendation,
            eventType,
            createdBy,
            DateTimeOffset.UtcNow);

        await _repository.AddAsync(history, cancellationToken);
        _logger.LogInformation(
            "Recommendation history appended {HistoryId} Recommendation={RecommendationId} Event={EventType}",
            history.Id,
            recommendation.Id,
            eventType);
    }

    public async Task PersistRangeFromRecommendationsAsync(
        IReadOnlyList<Recommendation> recommendations,
        string eventType,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (recommendations.Count == 0)
        {
            return;
        }

        var createdAt = DateTimeOffset.UtcNow;
        var histories = recommendations
            .Select(recommendation => RecommendationHistory.FromRecommendation(
                recommendation,
                eventType,
                createdBy,
                createdAt))
            .ToList();

        await _repository.AddRangeAsync(histories, cancellationToken);
        _logger.LogInformation(
            "Appended {Count} recommendation history rows for event {EventType}",
            histories.Count,
            eventType);
    }

    public async Task PersistWorkflowTransitionAsync(
        Recommendation recommendation,
        RecommendationWorkflow workflow,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var history = RecommendationHistory.FromRecommendation(
            recommendation,
            RecommendationHistoryEventType.WorkflowTransition,
            createdBy,
            DateTimeOffset.UtcNow,
            workflow.Status,
            workflow.Id,
            workflow.Approver,
            workflow.ApprovalDate,
            workflow.ApprovalComment);

        await _repository.AddAsync(history, cancellationToken);
        _logger.LogInformation(
            "Recommendation workflow history appended {HistoryId} Workflow={WorkflowId} Status={Status}",
            history.Id,
            workflow.Id,
            workflow.Status);
    }

    private async Task EnsureRecommendationExistsAsync(
        Guid recommendationId,
        CancellationToken cancellationToken)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(
            recommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException($"Recommendation with id '{recommendationId}' was not found.");
        }
    }

    private static RecommendationHistoryResponse MapToResponse(RecommendationHistory history) =>
        new()
        {
            Id = history.Id,
            RecommendationId = history.RecommendationId,
            RecommendationNumber = history.RecommendationNumber,
            RecommendationVersion = history.RecommendationVersion,
            CompanyId = history.CompanyId,
            MissionId = history.MissionId,
            ContractId = history.ContractId,
            DeliveryStrategyId = history.DeliveryStrategyId,
            Title = history.Title,
            Summary = history.Summary,
            EventType = history.EventType,
            RecommendationStatus = history.RecommendationStatus,
            WorkflowStatus = history.WorkflowStatus,
            WorkflowId = history.WorkflowId,
            Approver = history.Approver,
            ApprovalDate = history.ApprovalDate,
            ApprovalComment = history.ApprovalComment,
            Score = history.Score,
            Rank = history.Rank,
            PlanningTemplateId = history.PlanningTemplateId,
            DecisionEngineVersion = history.DecisionEngineVersion,
            CapacitySnapshot = history.CapacitySnapshot,
            WorkloadSnapshot = history.WorkloadSnapshot,
            RecommendationPayload = history.RecommendationPayload,
            CreatedBy = history.CreatedBy,
            CreatedAt = history.CreatedAt,
            DecisionProfileId = history.DecisionProfileId,
            DecisionProfileVersion = history.DecisionProfileVersion
        };
}
