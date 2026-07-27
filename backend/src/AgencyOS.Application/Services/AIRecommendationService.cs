using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class AIRecommendationService : IAIRecommendationService
{
    private readonly IAIRecommendationRepository _repository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IAIRecommendationGenerationService _generationService;
    private readonly IAIRecommendationComparisonService _comparisonService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AIRecommendationService> _logger;

    public AIRecommendationService(
        IAIRecommendationRepository repository,
        IRecommendationRepository recommendationRepository,
        IAIRecommendationGenerationService generationService,
        IAIRecommendationComparisonService comparisonService,
        IAuditService auditService,
        ILogger<AIRecommendationService> logger)
    {
        _repository = repository;
        _recommendationRepository = recommendationRepository;
        _generationService = generationService;
        _comparisonService = comparisonService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AIRecommendationResponse>> GetAllAsync(
        AIRecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.QueryAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<AIRecommendationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(item);
    }

    public async Task<IReadOnlyList<AIRecommendationResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecommendationExistsAsync(recommendationId, cancellationToken);
        var items = await _repository.GetByRecommendationIdAsync(recommendationId, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<AIRecommendationResponse> GenerateAsync(
        GenerateAIRecommendationRequest request,
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
                "AI Recommendations cannot be generated for archived Recommendations.");
        }

        try
        {
            var generationVersion = await _repository.GetNextGenerationVersionAsync(
                recommendation.Id,
                cancellationToken);

            var generated = _generationService.Generate(
                recommendation,
                generationVersion,
                request.GeneratedBy,
                DateTimeOffset.UtcNow);

            if (!generated.Validate())
            {
                throw new BusinessRuleException("Generated AI Recommendation failed domain validation.");
            }

            var created = await _repository.AddAsync(generated, cancellationToken);
            await RecordAuditAsync(
                created,
                AuditEventTypes.Created,
                "AIRecommendation.Generate",
                request.GeneratedBy,
                previousState: null,
                cancellationToken);

            _logger.LogInformation(
                "AI Recommendation generated {AIRecommendationId} for Recommendation={RecommendationId} Generation={GenerationVersion}",
                created.Id,
                created.RecommendationId,
                created.GenerationVersion);

            return MapToResponse(created);
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            // BR-1608: AI generation failures shall not affect Recommendation lifecycle.
            _logger.LogError(
                ex,
                "AI Recommendation generation failed for Recommendation {RecommendationId}",
                request.RecommendationId);
            throw new BusinessRuleException(
                "AI Recommendation generation failed. The originating Recommendation was not modified.");
        }
    }

    public async Task<AIRecommendationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOrThrowAsync(id, cancellationToken);
        var previous = AuditService.SerializeState(new { item.Status });

        try
        {
            item.Archive(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(item, cancellationToken);
        await RecordAuditAsync(
            updated,
            AuditEventTypes.Archived,
            "AIRecommendation.Archive",
            updated.GeneratedBy,
            previous,
            cancellationToken);

        return MapToResponse(updated);
    }

    public async Task<AIRecommendationComparisonResponse> CompareAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var aiRecommendation = await GetOrThrowAsync(id, cancellationToken);
        var recommendation = await _recommendationRepository.GetByIdAsync(
            aiRecommendation.RecommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException(
                $"Recommendation with id '{aiRecommendation.RecommendationId}' was not found.");
        }

        try
        {
            return _comparisonService.Compare(aiRecommendation, recommendation);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    internal static AIRecommendationResponse MapToResponse(AIRecommendation item) =>
        new()
        {
            Id = item.Id,
            RecommendationId = item.RecommendationId,
            RecommendationVersion = item.RecommendationVersion,
            GenerationVersion = item.GenerationVersion,
            GeneratedAt = item.GeneratedAt,
            GeneratedBy = item.GeneratedBy,
            ConfidenceScore = item.ConfidenceScore,
            ExecutiveSummary = item.ExecutiveSummary,
            Reasoning = item.Reasoning,
            Assumptions = item.Assumptions,
            Risks = item.Risks,
            Alternatives = item.Alternatives,
            SuggestedDeliveryStrategy = item.SuggestedDeliveryStrategy,
            SuggestedCapacityImpact = item.SuggestedCapacityImpact,
            SuggestedWorkloadImpact = item.SuggestedWorkloadImpact,
            ModelVersion = item.ModelVersion,
            PromptVersion = item.PromptVersion,
            Status = item.Status,
            ArchivedAt = item.ArchivedAt,
            Archived = item.Archived,
            DecisionProfileId = item.DecisionProfileId,
            DecisionProfileVersion = item.DecisionProfileVersion,
            CompanyId = item.CompanyId
        };

    private async Task RecordAuditAsync(
        AIRecommendation item,
        string eventType,
        string action,
        string actor,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.AIRecommendation,
                EntityId = item.Id,
                EntityVersion = item.GenerationVersion.ToString(),
                EventType = eventType,
                Action = action,
                UserId = actor,
                UserName = actor,
                Source = AuditSources.System,
                PreviousState = previousState,
                CurrentState = AuditService.SerializeState(new
                {
                    item.Id,
                    item.RecommendationId,
                    item.GenerationVersion,
                    item.ConfidenceScore,
                    item.Status,
                    item.SuggestedDeliveryStrategy
                }),
                Metadata = AuditService.SerializeState(new
                {
                    item.ModelVersion,
                    item.PromptVersion,
                    advisoryOnly = true
                })
            },
            cancellationToken);
    }

    private async Task<AIRecommendation> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"AI Recommendation with id '{id}' was not found.");
        }

        return item;
    }

    private async Task EnsureRecommendationExistsAsync(Guid recommendationId, CancellationToken cancellationToken)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(recommendationId, cancellationToken);
        if (recommendation is null)
        {
            throw new NotFoundException($"Recommendation with id '{recommendationId}' was not found.");
        }
    }
}
