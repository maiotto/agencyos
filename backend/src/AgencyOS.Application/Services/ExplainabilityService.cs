using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ExplainabilityService : IExplainabilityService
{
    private readonly IExplainabilityRepository _repository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IAIRecommendationRepository _aiRecommendationRepository;
    private readonly IExplainabilityGenerationService _generationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ExplainabilityService> _logger;

    public ExplainabilityService(
        IExplainabilityRepository repository,
        IRecommendationRepository recommendationRepository,
        IAIRecommendationRepository aiRecommendationRepository,
        IExplainabilityGenerationService generationService,
        IAuditService auditService,
        ILogger<ExplainabilityService> logger)
    {
        _repository = repository;
        _recommendationRepository = recommendationRepository;
        _aiRecommendationRepository = aiRecommendationRepository;
        _generationService = generationService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExplainabilityResponse>> GetAllAsync(
        ExplainabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.QueryAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<ExplainabilityResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(item);
    }

    public async Task<IReadOnlyList<ExplainabilityResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecommendationExistsAsync(recommendationId, cancellationToken);
        var items = await _repository.GetByRecommendationIdAsync(recommendationId, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<ExplainabilityResponse> GenerateAsync(
        GenerateExplainabilityRequest request,
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
                "Explainability cannot be generated for archived Recommendations.");
        }

        var explanationType = ResolveExplanationType(request);

        try
        {
            var generationVersion = await _repository.GetNextGenerationVersionAsync(
                recommendation.Id,
                cancellationToken);

            Explainability generated;
            if (ExplainabilityTypes.IsAIRecommendation(explanationType))
            {
                var aiId = request.AIRecommendationId
                    ?? throw new BusinessRuleException(
                        "AIRecommendationId is required for AIRecommendation explanations.");

                var aiRecommendation = await _aiRecommendationRepository.GetByIdAsync(
                    aiId,
                    cancellationToken);

                if (aiRecommendation is null)
                {
                    throw new NotFoundException($"AI Recommendation with id '{aiId}' was not found.");
                }

                if (aiRecommendation.RecommendationId != recommendation.Id)
                {
                    throw new BusinessRuleException(
                        "AI Recommendation does not belong to the specified Recommendation.");
                }

                generated = _generationService.GenerateForAIRecommendation(
                    recommendation,
                    aiRecommendation,
                    generationVersion,
                    request.GeneratedBy,
                    DateTimeOffset.UtcNow);
            }
            else
            {
                generated = _generationService.GenerateForRecommendation(
                    recommendation,
                    generationVersion,
                    request.GeneratedBy,
                    DateTimeOffset.UtcNow);
            }

            if (!generated.Validate())
            {
                throw new BusinessRuleException("Generated Explainability failed domain validation.");
            }

            var created = await _repository.AddAsync(generated, cancellationToken);
            await RecordAuditAsync(
                created,
                AuditEventTypes.Created,
                "Explainability.Generate",
                request.GeneratedBy,
                previousState: null,
                cancellationToken);

            _logger.LogInformation(
                "Explainability generated {ExplainabilityId} for Recommendation={RecommendationId} Type={ExplanationType} Generation={GenerationVersion}",
                created.Id,
                created.RecommendationId,
                created.ExplanationType,
                created.GenerationVersion);

            return MapToResponse(created);
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            // BR-1705: Explanation generation failures shall not affect Recommendations.
            _logger.LogError(
                ex,
                "Explainability generation failed for Recommendation {RecommendationId}",
                request.RecommendationId);
            throw new BusinessRuleException(
                "Explainability generation failed. The originating Recommendation was not modified.");
        }
    }

    public async Task<ExplainabilityResponse> ArchiveAsync(
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
            "Explainability.Archive",
            updated.GeneratedBy,
            previous,
            cancellationToken);

        return MapToResponse(updated);
    }

    internal static ExplainabilityResponse MapToResponse(Explainability item) =>
        new()
        {
            Id = item.Id,
            RecommendationId = item.RecommendationId,
            AIRecommendationId = item.AIRecommendationId,
            ExplanationType = item.ExplanationType,
            GenerationVersion = item.GenerationVersion,
            ExecutiveSummary = item.ExecutiveSummary,
            DetailedExplanation = item.DetailedExplanation,
            DecisionFactors = item.DecisionFactors,
            Assumptions = item.Assumptions,
            Risks = item.Risks,
            ConfidenceExplanation = item.ConfidenceExplanation,
            CapacityExplanation = item.CapacityExplanation,
            WorkloadExplanation = item.WorkloadExplanation,
            GeneratedAt = item.GeneratedAt,
            GeneratedBy = item.GeneratedBy,
            ModelVersion = item.ModelVersion,
            PromptVersion = item.PromptVersion,
            Status = item.Status,
            ArchivedAt = item.ArchivedAt,
            Archived = item.Archived,
            DecisionProfileId = item.DecisionProfileId,
            DecisionProfileVersion = item.DecisionProfileVersion,
            CompanyId = item.CompanyId
        };

    private static string ResolveExplanationType(GenerateExplainabilityRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ExplanationType))
        {
            return request.ExplanationType.Trim();
        }

        return request.AIRecommendationId.HasValue
            ? ExplainabilityTypes.AIRecommendation
            : ExplainabilityTypes.Recommendation;
    }

    private async Task RecordAuditAsync(
        Explainability item,
        string eventType,
        string action,
        string actor,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Explainability,
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
                    item.AIRecommendationId,
                    item.ExplanationType,
                    item.GenerationVersion,
                    item.Status
                }),
                Metadata = AuditService.SerializeState(new
                {
                    item.ModelVersion,
                    item.PromptVersion,
                    informationalOnly = true
                })
            },
            cancellationToken);
    }

    private async Task<Explainability> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Explainability with id '{id}' was not found.");
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
