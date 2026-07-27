using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IExecutiveRecommendationSummaryRepository
{
    Task<IReadOnlyList<ExecutiveRecommendationSummary>> QueryAsync(
        ExecutiveRecommendationSummaryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummary?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExecutiveRecommendationSummary>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSummaryVersionAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummary> AddAsync(
        ExecutiveRecommendationSummary summary,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummary> UpdateAsync(
        ExecutiveRecommendationSummary summary,
        CancellationToken cancellationToken = default);
}

public interface IExecutiveRecommendationSummaryGenerationService
{
    ExecutiveRecommendationSummary Generate(
        Recommendation recommendation,
        AIRecommendation? aiRecommendation,
        Explainability? explainability,
        Decision? decision,
        int summaryVersion,
        string generatedBy,
        DateTimeOffset generatedAt);
}

public interface IExecutiveRecommendationSummaryComparisonService
{
    ExecutiveRecommendationSummaryComparisonResponse Compare(
        ExecutiveRecommendationSummary summary,
        Recommendation recommendation);
}

public interface IExecutiveRecommendationSummaryService
{
    Task<IReadOnlyList<ExecutiveRecommendationSummaryResponse>> GetAllAsync(
        ExecutiveRecommendationSummaryQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummaryResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExecutiveRecommendationSummaryResponse>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummaryResponse> GenerateAsync(
        GenerateExecutiveRecommendationSummaryRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummaryResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummaryResponse> CreateNewVersionAsync(
        Guid id,
        CreateExecutiveRecommendationSummaryVersionRequest request,
        CancellationToken cancellationToken = default);

    Task<ExecutiveRecommendationSummaryComparisonResponse> CompareAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
