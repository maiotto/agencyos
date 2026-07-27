using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IExplainabilityRepository
{
    Task<IReadOnlyList<Explainability>> QueryAsync(
        ExplainabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Explainability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Explainability>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextGenerationVersionAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<Explainability> AddAsync(
        Explainability explainability,
        CancellationToken cancellationToken = default);

    Task<Explainability> UpdateAsync(
        Explainability explainability,
        CancellationToken cancellationToken = default);
}

public interface IExplainabilityGenerationService
{
    Explainability GenerateForRecommendation(
        Recommendation recommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt);

    Explainability GenerateForAIRecommendation(
        Recommendation recommendation,
        AIRecommendation aiRecommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt);
}

public interface IExplainabilityService
{
    Task<IReadOnlyList<ExplainabilityResponse>> GetAllAsync(
        ExplainabilityQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExplainabilityResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExplainabilityResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<ExplainabilityResponse> GenerateAsync(
        GenerateExplainabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<ExplainabilityResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
