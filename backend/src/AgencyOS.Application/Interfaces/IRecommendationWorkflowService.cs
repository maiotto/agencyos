using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IRecommendationWorkflowService
{
    Task<IReadOnlyList<RecommendationWorkflowResponse>> GetAllAsync(
        RecommendationWorkflowQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendationWorkflowTransitionResponse>> GetTimelineAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> CreateAsync(
        CreateRecommendationWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> SubmitAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> ApproveAsync(
        Guid id,
        ApproveRecommendationWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> RejectAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> CancelAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default);

    Task<RecommendationWorkflowResponse> ReopenAsync(
        Guid id,
        RecommendationWorkflowActionRequest request,
        CancellationToken cancellationToken = default);
}
