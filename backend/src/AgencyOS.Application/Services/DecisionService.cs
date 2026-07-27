using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class DecisionService : IDecisionService, IDecisionTimelineService
{
    private readonly IDecisionRepository _decisionRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IRecommendationWorkflowRepository _workflowRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<DecisionService> _logger;

    public DecisionService(
        IDecisionRepository decisionRepository,
        IRecommendationRepository recommendationRepository,
        IRecommendationWorkflowRepository workflowRepository,
        IAuditService auditService,
        ILogger<DecisionService> logger)
    {
        _decisionRepository = decisionRepository;
        _recommendationRepository = recommendationRepository;
        _workflowRepository = workflowRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DecisionResponse>> GetAllAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _decisionRepository.GetAllAsync(parameters, cancellationToken);
        return decisions.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<DecisionResponse>> FilterAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<DecisionResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(decision);
    }

    public async Task<IReadOnlyList<DecisionTimelineEntryResponse>> GetTimelineAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        return MapTimeline(decision.Timeline);
    }

    Task<IReadOnlyList<DecisionTimelineEntryResponse>> IDecisionTimelineService.GetTimelineAsync(
        Guid decisionId,
        CancellationToken cancellationToken) =>
        GetTimelineAsync(decisionId, cancellationToken);

    public async Task<DecisionResponse> CreateAsync(
        CreateDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(
            request.RecommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException(
                $"Recommendation with id '{request.RecommendationId}' was not found.");
        }

        if (recommendation.Archived)
        {
            throw new BusinessRuleException(
                "Archived recommendations cannot generate Decisions.");
        }

        var approvedWorkflows = await _workflowRepository.GetAllAsync(
            new RecommendationWorkflowQueryParameters
            {
                RecommendationId = recommendation.Id,
                Status = RecommendationWorkflowStatus.Approved
            },
            cancellationToken);

        if (approvedWorkflows.Count == 0)
        {
            throw new BusinessRuleException(
                "Every Approved Recommendation may generate one Decision. No Approved workflow was found for this recommendation.");
        }

        if (await _decisionRepository.ExistsByRecommendationIdAsync(recommendation.Id, cancellationToken))
        {
            throw new BusinessRuleException(
                "A Decision already exists for this Recommendation (BR-1401).");
        }

        Decision decision;
        try
        {
            decision = Decision.Create(
                recommendation.Id,
                recommendation.CompanyId,
                recommendation.MissionId,
                recommendation.ContractId,
                request.CreatedBy,
                request.DecisionDate ?? DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (!decision.Validate())
        {
            throw new BusinessRuleException("Decision failed domain validation.");
        }

        var created = await _decisionRepository.AddAsync(decision, cancellationToken);
        await RecordDecisionAuditAsync(
            created,
            AuditEventTypes.Created,
            "Decision.Create",
            request.CreatedBy,
            previousState: null,
            cancellationToken);
        _logger.LogInformation(
            "Decision Created: {DecisionId} Recommendation={RecommendationId}",
            created.Id,
            created.RecommendationId);

        return MapToResponse(created);
    }

    public async Task<DecisionResponse> StartImplementationAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        var previous = SnapshotDecision(decision);
        try
        {
            decision.StartImplementation(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _decisionRepository.UpdateAsync(decision, cancellationToken);
        await RecordDecisionAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Decision.StartImplementation",
            request.Actor,
            previous,
            cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<DecisionResponse> CompleteAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        var previous = SnapshotDecision(decision);
        try
        {
            decision.Complete(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _decisionRepository.UpdateAsync(decision, cancellationToken);
        await RecordDecisionAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Decision.Complete",
            request.Actor,
            previous,
            cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<DecisionResponse> CancelAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        var previous = SnapshotDecision(decision);
        try
        {
            decision.Cancel(request.Actor, request.Comment, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _decisionRepository.UpdateAsync(decision, cancellationToken);
        await RecordDecisionAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Decision.Cancel",
            request.Actor,
            previous,
            cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<DecisionResponse> RecordOutcomeAsync(
        Guid id,
        RecordDecisionOutcomeRequest request,
        CancellationToken cancellationToken = default)
    {
        var decision = await GetOrThrowAsync(id, cancellationToken);
        var previous = SnapshotDecision(decision);
        try
        {
            decision.RecordOutcome(
                request.Outcome,
                request.BusinessValue,
                request.Actor,
                request.Comment,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _decisionRepository.UpdateAsync(decision, cancellationToken);
        await RecordDecisionAuditAsync(
            updated,
            AuditEventTypes.OutcomeRecorded,
            "Decision.RecordOutcome",
            request.Actor,
            previous,
            cancellationToken);
        return MapToResponse(updated);
    }

    private async Task RecordDecisionAuditAsync(
        Decision decision,
        string eventType,
        string action,
        string actor,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Decision,
                EntityId = decision.Id,
                EventType = eventType,
                Action = action,
                CompanyId = decision.CompanyId,
                UserId = actor,
                UserName = actor,
                Source = AuditSources.Api,
                PreviousState = previousState,
                CurrentState = SnapshotDecision(decision),
                Metadata = AuditService.SerializeState(new { decision.RecommendationId })
            },
            cancellationToken);
    }

    private static string SnapshotDecision(Decision decision) =>
        AuditService.SerializeState(new
        {
            decision.Id,
            decision.RecommendationId,
            decision.DecisionStatus,
            decision.ImplementationStatus,
            decision.Outcome,
            decision.BusinessValue,
            decision.CompletedDate
        });

    private async Task<Decision> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var decision = await _decisionRepository.GetByIdAsync(id, cancellationToken);
        if (decision is null)
        {
            throw new NotFoundException($"Decision with id '{id}' was not found.");
        }

        return decision;
    }

    private static DecisionResponse MapToResponse(Decision decision) =>
        new()
        {
            Id = decision.Id,
            RecommendationId = decision.RecommendationId,
            CompanyId = decision.CompanyId,
            MissionId = decision.MissionId,
            ContractId = decision.ContractId,
            DecisionStatus = decision.DecisionStatus,
            ImplementationStatus = decision.ImplementationStatus,
            DecisionDate = decision.DecisionDate,
            ImplementationDate = decision.ImplementationDate,
            CompletedDate = decision.CompletedDate,
            Outcome = decision.Outcome,
            BusinessValue = decision.BusinessValue,
            CreatedBy = decision.CreatedBy,
            CreatedAt = decision.CreatedAt,
            UpdatedAt = decision.UpdatedAt,
            Timeline = MapTimeline(decision.Timeline)
        };

    private static IReadOnlyList<DecisionTimelineEntryResponse> MapTimeline(
        IEnumerable<DecisionTimelineEntry> timeline) =>
        timeline
            .OrderBy(entry => entry.OccurredAt)
            .ThenBy(entry => entry.Id)
            .Select(entry => new DecisionTimelineEntryResponse
            {
                Id = entry.Id,
                EventType = entry.EventType,
                FromDecisionStatus = entry.FromDecisionStatus,
                ToDecisionStatus = entry.ToDecisionStatus,
                FromImplementationStatus = entry.FromImplementationStatus,
                ToImplementationStatus = entry.ToImplementationStatus,
                Actor = entry.Actor,
                Comment = entry.Comment,
                OccurredAt = entry.OccurredAt
            })
            .ToList();
}
