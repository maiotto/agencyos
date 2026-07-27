using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ExecutiveRecommendationSummaryService : IExecutiveRecommendationSummaryService
{
    private readonly IExecutiveRecommendationSummaryRepository _repository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IAIRecommendationRepository _aiRecommendationRepository;
    private readonly IExplainabilityRepository _explainabilityRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly IExecutiveRecommendationSummaryGenerationService _generationService;
    private readonly IExecutiveRecommendationSummaryComparisonService _comparisonService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ExecutiveRecommendationSummaryService> _logger;

    public ExecutiveRecommendationSummaryService(
        IExecutiveRecommendationSummaryRepository repository,
        IRecommendationRepository recommendationRepository,
        IAIRecommendationRepository aiRecommendationRepository,
        IExplainabilityRepository explainabilityRepository,
        IDecisionRepository decisionRepository,
        IExecutiveRecommendationSummaryGenerationService generationService,
        IExecutiveRecommendationSummaryComparisonService comparisonService,
        IAuditService auditService,
        ILogger<ExecutiveRecommendationSummaryService> logger)
    {
        _repository = repository;
        _recommendationRepository = recommendationRepository;
        _aiRecommendationRepository = aiRecommendationRepository;
        _explainabilityRepository = explainabilityRepository;
        _decisionRepository = decisionRepository;
        _generationService = generationService;
        _comparisonService = comparisonService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExecutiveRecommendationSummaryResponse>> GetAllAsync(
        ExecutiveRecommendationSummaryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _repository.QueryAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<ExecutiveRecommendationSummaryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(item);
    }

    public async Task<IReadOnlyList<ExecutiveRecommendationSummaryResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRecommendationExistsAsync(recommendationId, cancellationToken);
        var items = await _repository.GetByRecommendationIdAsync(recommendationId, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<ExecutiveRecommendationSummaryResponse> GenerateAsync(
        GenerateExecutiveRecommendationSummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await GetActiveRecommendationOrThrowAsync(
            request.RecommendationId,
            cancellationToken);

        try
        {
            var context = await ResolveContextAsync(
                recommendation,
                request.AIRecommendationId,
                request.ExplainabilityId,
                cancellationToken);

            var summaryVersion = await _repository.GetNextSummaryVersionAsync(
                recommendation.Id,
                cancellationToken);

            var generated = _generationService.Generate(
                recommendation,
                context.AiRecommendation,
                context.Explainability,
                context.Decision,
                summaryVersion,
                request.GeneratedBy,
                DateTimeOffset.UtcNow);

            if (!generated.Validate())
            {
                throw new BusinessRuleException(
                    "Generated Executive Recommendation Summary failed domain validation.");
            }

            var created = await _repository.AddAsync(generated, cancellationToken);
            await RecordAuditAsync(
                created,
                AuditEventTypes.Created,
                "ExecutiveRecommendationSummary.Generate",
                request.GeneratedBy,
                previousState: null,
                cancellationToken);

            _logger.LogInformation(
                "Executive Summary generated {SummaryId} for Recommendation={RecommendationId} Version={SummaryVersion}",
                created.Id,
                created.RecommendationId,
                created.SummaryVersion);

            return MapToResponse(created);
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            // BR-1805: Generation failures shall never affect Recommendations.
            _logger.LogError(
                ex,
                "Executive Summary generation failed for Recommendation {RecommendationId}",
                request.RecommendationId);
            throw new BusinessRuleException(
                "Executive Summary generation failed. The originating Recommendation was not modified.");
        }
    }

    public async Task<ExecutiveRecommendationSummaryResponse> CreateNewVersionAsync(
        Guid id,
        CreateExecutiveRecommendationSummaryVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetOrThrowAsync(id, cancellationToken);
        var recommendation = await GetActiveRecommendationOrThrowAsync(
            existing.RecommendationId,
            cancellationToken);

        try
        {
            var aiId = request.AIRecommendationId ?? existing.AIRecommendationId;
            var explainabilityId = request.ExplainabilityId ?? existing.ExplainabilityId;
            var context = await ResolveContextAsync(
                recommendation,
                aiId,
                explainabilityId,
                cancellationToken);

            var nextVersion = await _repository.GetNextSummaryVersionAsync(
                recommendation.Id,
                cancellationToken);

            var regenerated = _generationService.Generate(
                recommendation,
                context.AiRecommendation,
                context.Explainability,
                context.Decision,
                nextVersion,
                request.GeneratedBy,
                DateTimeOffset.UtcNow);

            // Domain CreateNewVersion validates version increment semantics against the source.
            _ = existing.CreateNewVersion(
                nextVersion,
                regenerated.GeneratedAt,
                regenerated.GeneratedBy,
                regenerated.ExecutiveSummary,
                regenerated.KeyDecisionFactors,
                regenerated.BusinessImpact,
                regenerated.CapacityImpact,
                regenerated.WorkloadImpact,
                regenerated.Risks,
                regenerated.Assumptions,
                regenerated.ConfidenceLevel,
                regenerated.RecommendedActions,
                regenerated.AIRecommendationId,
                regenerated.ExplainabilityId,
                regenerated.ModelVersion,
                regenerated.PromptVersion);

            if (!regenerated.Validate())
            {
                throw new BusinessRuleException(
                    "Regenerated Executive Recommendation Summary failed domain validation.");
            }

            var created = await _repository.AddAsync(regenerated, cancellationToken);
            await RecordAuditAsync(
                created,
                AuditEventTypes.VersionCreated,
                "ExecutiveRecommendationSummary.CreateNewVersion",
                request.GeneratedBy,
                previousState: AuditService.SerializeState(new
                {
                    sourceId = existing.Id,
                    sourceVersion = existing.SummaryVersion
                }),
                cancellationToken);

            return MapToResponse(created);
        }
        catch (Exception ex) when (ex is not BusinessRuleException and not NotFoundException)
        {
            _logger.LogError(
                ex,
                "Executive Summary versioning failed for Recommendation {RecommendationId}",
                existing.RecommendationId);
            throw new BusinessRuleException(
                "Executive Summary versioning failed. The originating Recommendation was not modified.");
        }
    }

    public async Task<ExecutiveRecommendationSummaryResponse> ArchiveAsync(
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
            "ExecutiveRecommendationSummary.Archive",
            updated.GeneratedBy,
            previous,
            cancellationToken);

        return MapToResponse(updated);
    }

    public async Task<ExecutiveRecommendationSummaryComparisonResponse> CompareAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetOrThrowAsync(id, cancellationToken);
        var recommendation = await _recommendationRepository.GetByIdAsync(
            summary.RecommendationId,
            cancellationToken);

        if (recommendation is null)
        {
            throw new NotFoundException(
                $"Recommendation with id '{summary.RecommendationId}' was not found.");
        }

        try
        {
            return _comparisonService.Compare(summary, recommendation);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    internal static ExecutiveRecommendationSummaryResponse MapToResponse(
        ExecutiveRecommendationSummary item) =>
        new()
        {
            Id = item.Id,
            RecommendationId = item.RecommendationId,
            AIRecommendationId = item.AIRecommendationId,
            ExplainabilityId = item.ExplainabilityId,
            SummaryVersion = item.SummaryVersion,
            ExecutiveSummary = item.ExecutiveSummary,
            KeyDecisionFactors = item.KeyDecisionFactors,
            BusinessImpact = item.BusinessImpact,
            CapacityImpact = item.CapacityImpact,
            WorkloadImpact = item.WorkloadImpact,
            Risks = item.Risks,
            Assumptions = item.Assumptions,
            ConfidenceLevel = item.ConfidenceLevel,
            RecommendedActions = item.RecommendedActions,
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

    private async Task<(AIRecommendation? AiRecommendation, Explainability? Explainability, Decision? Decision)>
        ResolveContextAsync(
            Recommendation recommendation,
            Guid? aiRecommendationId,
            Guid? explainabilityId,
            CancellationToken cancellationToken)
    {
        AIRecommendation? aiRecommendation = null;
        if (aiRecommendationId.HasValue)
        {
            aiRecommendation = await _aiRecommendationRepository.GetByIdAsync(
                aiRecommendationId.Value,
                cancellationToken);
            if (aiRecommendation is null)
            {
                throw new NotFoundException(
                    $"AI Recommendation with id '{aiRecommendationId}' was not found.");
            }

            if (aiRecommendation.RecommendationId != recommendation.Id)
            {
                throw new BusinessRuleException(
                    "AI Recommendation does not belong to the specified Recommendation.");
            }
        }
        else
        {
            var latestAi = await _aiRecommendationRepository.GetByRecommendationIdAsync(
                recommendation.Id,
                cancellationToken);
            aiRecommendation = latestAi.FirstOrDefault(item => !item.Archived) ?? latestAi.FirstOrDefault();
        }

        Explainability? explainability = null;
        if (explainabilityId.HasValue)
        {
            explainability = await _explainabilityRepository.GetByIdAsync(
                explainabilityId.Value,
                cancellationToken);
            if (explainability is null)
            {
                throw new NotFoundException(
                    $"Explainability with id '{explainabilityId}' was not found.");
            }

            if (explainability.RecommendationId != recommendation.Id)
            {
                throw new BusinessRuleException(
                    "Explainability does not belong to the specified Recommendation.");
            }
        }
        else
        {
            var latestExplainability = await _explainabilityRepository.GetByRecommendationIdAsync(
                recommendation.Id,
                cancellationToken);
            explainability = latestExplainability.FirstOrDefault(item => !item.Archived)
                             ?? latestExplainability.FirstOrDefault();
        }

        var decision = await _decisionRepository.GetByRecommendationIdAsync(
            recommendation.Id,
            cancellationToken);

        return (aiRecommendation, explainability, decision);
    }

    private async Task RecordAuditAsync(
        ExecutiveRecommendationSummary item,
        string eventType,
        string action,
        string actor,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.ExecutiveRecommendationSummary,
                EntityId = item.Id,
                EntityVersion = item.SummaryVersion.ToString(),
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
                    item.SummaryVersion,
                    item.ConfidenceLevel,
                    item.Status
                }),
                Metadata = AuditService.SerializeState(new
                {
                    item.ModelVersion,
                    item.PromptVersion,
                    item.AIRecommendationId,
                    item.ExplainabilityId,
                    informationalOnly = true
                })
            },
            cancellationToken);
    }

    private async Task<ExecutiveRecommendationSummary> GetOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Executive Recommendation Summary with id '{id}' was not found.");
        }

        return item;
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

    private async Task<Recommendation> GetActiveRecommendationOrThrowAsync(
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

        if (recommendation.Archived)
        {
            throw new BusinessRuleException(
                "Executive Summaries cannot be generated for archived Recommendations.");
        }

        return recommendation;
    }
}
