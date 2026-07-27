using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IAIRecommendationRepository
{
    Task<IReadOnlyList<AIRecommendation>> QueryAsync(
        AIRecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AIRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIRecommendation>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextGenerationVersionAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<AIRecommendation> AddAsync(
        AIRecommendation aiRecommendation,
        CancellationToken cancellationToken = default);

    Task<AIRecommendation> UpdateAsync(
        AIRecommendation aiRecommendation,
        CancellationToken cancellationToken = default);
}

public interface IAIRecommendationGenerationService
{
    AIRecommendation Generate(
        Recommendation recommendation,
        int generationVersion,
        string generatedBy,
        DateTimeOffset generatedAt);
}

public interface IAIRecommendationComparisonService
{
    AIRecommendationComparisonResponse Compare(
        AIRecommendation aiRecommendation,
        Recommendation recommendation);
}

public interface IAIRecommendationService
{
    Task<IReadOnlyList<AIRecommendationResponse>> GetAllAsync(
        AIRecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIRecommendationResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationResponse> GenerateAsync(
        GenerateAIRecommendationRequest request,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AIRecommendationComparisonResponse> CompareAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
