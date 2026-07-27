using System.Text.Json;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Mappings;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _repository;
    private readonly IRecommendationHistoryService _recommendationHistoryService;
    private readonly IAuditService _auditService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        IRecommendationRepository repository,
        IRecommendationHistoryService recommendationHistoryService,
        IAuditService auditService,
        ILogger<RecommendationService> logger)
    {
        _repository = repository;
        _recommendationHistoryService = recommendationHistoryService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetAllAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _repository.QueryAsync(parameters, cancellationToken);
        return recommendations.Select(RecommendationMappings.ToResponse).ToList();
    }

    public Task<IReadOnlyList<RecommendationResponse>> FilterAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<RecommendationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await GetOrThrowAsync(id, cancellationToken);
        return RecommendationMappings.ToResponse(recommendation);
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetByCompanyIdAsync(
        Guid companyId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _repository.GetByCompanyIdAsync(companyId, parameters, cancellationToken);
        return recommendations.Select(RecommendationMappings.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetByMissionIdAsync(
        Guid missionId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _repository.GetByMissionIdAsync(missionId, parameters, cancellationToken);
        return recommendations.Select(RecommendationMappings.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetByContractIdAsync(
        Guid contractId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _repository.GetByContractIdAsync(contractId, parameters, cancellationToken);
        return recommendations.Select(RecommendationMappings.ToResponse).ToList();
    }

    public async Task<IReadOnlyList<RecommendationResponse>> GetVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await GetOrThrowAsync(id, cancellationToken);
        var versions = await _repository.GetVersionsByNumberAsync(
            recommendation.RecommendationNumber,
            cancellationToken);
        return versions.Select(RecommendationMappings.ToResponse).ToList();
    }

    public async Task<RecommendationResponse> CreateAsync(
        CreateRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        var generatedAt = request.GeneratedAt ?? DateTimeOffset.UtcNow;
        var number = string.IsNullOrWhiteSpace(request.RecommendationNumber)
            ? RecommendationMappings.BuildRecommendationNumber(
                request.ContractId,
                request.DeliveryStrategyId,
                generatedAt)
            : request.RecommendationNumber.Trim();

        Recommendation recommendation;
        try
        {
            recommendation = Recommendation.Create(
                request.CompanyId,
                request.MissionId,
                request.ContractId,
                request.DeliveryStrategyId,
                number,
                request.Title,
                request.Summary,
                request.Reason,
                request.Score,
                request.Rank,
                request.Version ?? 1,
                string.IsNullOrWhiteSpace(request.DecisionEngineVersion)
                    ? RecommendationVersions.CurrentDecisionEngineVersion
                    : request.DecisionEngineVersion,
                request.PlanningTemplateId,
                NormalizeJson(request.CapacitySnapshot),
                NormalizeJson(request.WorkloadSnapshot),
                request.RecommendationPayload,
                request.GeneratedBy,
                generatedAt);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        try
        {
            var created = await _repository.AddAsync(recommendation, cancellationToken);
            await _recommendationHistoryService.PersistFromRecommendationAsync(
                created,
                RecommendationHistoryEventType.VersionCreated,
                created.GeneratedBy,
                cancellationToken);
            await RecordRecommendationAuditAsync(
                created,
                AuditEventTypes.Created,
                "Recommendation.Create",
                previousState: null,
                cancellationToken);
            _logger.LogInformation(
                "Recommendation persisted {RecommendationId} Number={Number} Version={Version}",
                created.Id,
                created.RecommendationNumber,
                created.Version);
            return RecommendationMappings.ToResponse(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recommendation persistence failed for DeliveryStrategy {DeliveryStrategyId}",
                request.DeliveryStrategyId);
            throw new BusinessRuleException(
                "Recommendation persistence failed. Publication aborted.");
        }
    }

    public async Task<RecommendationResponse> CreateNewVersionAsync(
        Guid id,
        CreateRecommendationVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await GetOrThrowAsync(id, cancellationToken);

        Recommendation versioned;
        try
        {
            versioned = source.CreateNewVersion(
                request.Title,
                request.Summary,
                request.Reason,
                request.Score,
                request.Rank,
                string.IsNullOrWhiteSpace(request.DecisionEngineVersion)
                    ? RecommendationVersions.CurrentDecisionEngineVersion
                    : request.DecisionEngineVersion,
                request.PlanningTemplateId,
                NormalizeJson(request.CapacitySnapshot),
                NormalizeJson(request.WorkloadSnapshot),
                request.RecommendationPayload,
                request.GeneratedBy,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        try
        {
            var created = await _repository.AddAsync(versioned, cancellationToken);
            await _recommendationHistoryService.PersistFromRecommendationAsync(
                created,
                RecommendationHistoryEventType.VersionCreated,
                created.GeneratedBy,
                cancellationToken);
            await RecordRecommendationAuditAsync(
                created,
                AuditEventTypes.VersionCreated,
                "Recommendation.CreateNewVersion",
                previousState: AuditService.SerializeState(new
                {
                    source.Id,
                    source.Version,
                    source.Status
                }),
                cancellationToken);
            _logger.LogInformation(
                "Recommendation version created {RecommendationId} Number={Number} Version={Version}",
                created.Id,
                created.RecommendationNumber,
                created.Version);
            return RecommendationMappings.ToResponse(created);
        }
        catch (Exception)
        {
            throw new BusinessRuleException(
                "Recommendation version persistence failed. Publication aborted.");
        }
    }

    public async Task<RecommendationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            recommendation.Archive(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(recommendation, cancellationToken);
        await _recommendationHistoryService.PersistFromRecommendationAsync(
            updated,
            RecommendationHistoryEventType.Archived,
            updated.GeneratedBy,
            cancellationToken);
        await RecordRecommendationAuditAsync(
            updated,
            AuditEventTypes.Archived,
            "Recommendation.Archive",
            previousState: AuditService.SerializeState(new { status = RecommendationStatus.Active }),
            cancellationToken);
        _logger.LogInformation("Recommendation archived {RecommendationId}", updated.Id);
        return RecommendationMappings.ToResponse(updated);
    }

    public async Task<RecommendationResponse> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            recommendation.Restore();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(recommendation, cancellationToken);
        await _recommendationHistoryService.PersistFromRecommendationAsync(
            updated,
            RecommendationHistoryEventType.Restored,
            updated.GeneratedBy,
            cancellationToken);
        await RecordRecommendationAuditAsync(
            updated,
            AuditEventTypes.Restored,
            "Recommendation.Restore",
            previousState: AuditService.SerializeState(new { status = RecommendationStatus.Archived }),
            cancellationToken);
        _logger.LogInformation("Recommendation restored {RecommendationId}", updated.Id);
        return RecommendationMappings.ToResponse(updated);
    }

    public async Task<IReadOnlyList<Recommendation>> PersistRankedStrategiesAsync(
        RankDeliveryStrategyRequest request,
        RankDeliveryStrategyResponse ranking,
        CancellationToken cancellationToken = default)
    {
        if (ranking.RankedStrategies.Count == 0)
        {
            return [];
        }

        var generatedAt = DateTimeOffset.UtcNow;
        var generatedBy = string.IsNullOrWhiteSpace(request.GeneratedBy)
            ? "decision-engine"
            : request.GeneratedBy.Trim();
        var companyId = request.CompanyId;
        var capacitySnapshot = BuildCapacitySnapshot(request);
        var workloadSnapshot = BuildWorkloadSnapshot(request);
        var persisted = new List<Recommendation>();

        try
        {
            foreach (var ranked in ranking.RankedStrategies)
            {
                var strategyId = ranked.EvaluatedStrategy.Strategy.StrategyId;
                var payload = RecommendationMappings.SerializePayload(new
                {
                    ranking.ContractId,
                    ranking.MissionId,
                    ranking.CompanyDecisionProfileId,
                    ranking.CompanyDecisionProfileName,
                    RankedStrategy = ranked,
                    PeriodStartDate = request.PeriodStartDate,
                    PeriodEndDate = request.PeriodEndDate,
                    request.PlanningTemplateId
                });

                var latest = await _repository.GetLatestByDeliveryStrategyAsync(
                    strategyId,
                    request.ContractId,
                    request.MissionId,
                    cancellationToken);

                Recommendation recommendation;
                if (latest is null)
                {
                    recommendation = Recommendation.Create(
                        companyId,
                        request.MissionId,
                        request.ContractId,
                        strategyId,
                        RecommendationMappings.BuildRecommendationNumber(
                            request.ContractId,
                            strategyId,
                            generatedAt),
                        ranked.EvaluatedStrategy.Strategy.StrategyName,
                        $"Ranked delivery strategy for mission {request.MissionId:N}.",
                        RecommendationMappings.BuildReason(ranked),
                        ranked.FinalScore,
                        ranked.RankPosition,
                        1,
                        RecommendationVersions.CurrentDecisionEngineVersion,
                        request.PlanningTemplateId,
                        capacitySnapshot,
                        workloadSnapshot,
                        payload,
                        generatedBy,
                        generatedAt,
                        ranking.CompanyDecisionProfileId,
                        ranking.CompanyDecisionProfileVersion);
                }
                else
                {
                    recommendation = latest.CreateNewVersion(
                        ranked.EvaluatedStrategy.Strategy.StrategyName,
                        $"Ranked delivery strategy for mission {request.MissionId:N}.",
                        RecommendationMappings.BuildReason(ranked),
                        ranked.FinalScore,
                        ranked.RankPosition,
                        RecommendationVersions.CurrentDecisionEngineVersion,
                        request.PlanningTemplateId,
                        capacitySnapshot,
                        workloadSnapshot,
                        payload,
                        generatedBy,
                        generatedAt,
                        ranking.CompanyDecisionProfileId,
                        ranking.CompanyDecisionProfileVersion);
                }

                persisted.Add(recommendation);
            }

            await _repository.AddRangeAsync(persisted, cancellationToken);
            await _recommendationHistoryService.PersistRangeFromRecommendationsAsync(
                persisted,
                RecommendationHistoryEventType.VersionCreated,
                generatedBy,
                cancellationToken);

            foreach (var recommendation in persisted)
            {
                await RecordRecommendationAuditAsync(
                    recommendation,
                    recommendation.Version <= 1 ? AuditEventTypes.Created : AuditEventTypes.VersionCreated,
                    "Recommendation.PersistRanked",
                    previousState: null,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Persisted {Count} recommendations for Contract {ContractId} Mission {MissionId}",
                persisted.Count,
                request.ContractId,
                request.MissionId);

            return persisted;
        }
        catch (Exception ex) when (ex is not BusinessRuleException)
        {
            _logger.LogError(
                ex,
                "Recommendation persistence failed for Contract {ContractId}. Publication aborted.",
                request.ContractId);
            throw new BusinessRuleException(
                "Recommendation persistence failed. Publication aborted.");
        }
    }

    private async Task RecordRecommendationAuditAsync(
        Recommendation recommendation,
        string eventType,
        string action,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Recommendation,
                EntityId = recommendation.Id,
                EntityVersion = recommendation.Version.ToString(),
                EventType = eventType,
                Action = action,
                CompanyId = recommendation.CompanyId,
                UserId = recommendation.GeneratedBy,
                UserName = recommendation.GeneratedBy,
                Source = AuditSources.DecisionEngine,
                PreviousState = previousState,
                CurrentState = AuditService.SerializeState(new
                {
                    recommendation.Id,
                    recommendation.RecommendationNumber,
                    recommendation.Version,
                    recommendation.Status,
                    recommendation.Score,
                    recommendation.Rank,
                    recommendation.Title
                })
            },
            cancellationToken);
    }

    private async Task<Recommendation> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(id, cancellationToken);
        if (recommendation is null)
        {
            throw new NotFoundException($"Recommendation with id '{id}' was not found.");
        }

        return recommendation;
    }

    private static string NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        using var document = JsonDocument.Parse(json);
        return json.Trim();
    }

    private static string BuildCapacitySnapshot(RankDeliveryStrategyRequest request) =>
        RecommendationMappings.SerializePayload(new
        {
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            Source = "decision-engine-ranking",
            request.PlanningTemplateId
        });

    private static string BuildWorkloadSnapshot(RankDeliveryStrategyRequest request) =>
        RecommendationMappings.SerializePayload(new
        {
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            Source = "decision-engine-ranking",
            request.ContractId,
            request.MissionId
        });
}
