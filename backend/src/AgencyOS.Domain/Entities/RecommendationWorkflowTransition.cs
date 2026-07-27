namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable workflow transition record (US-201 / BR-1009 / BR-1010).
/// </summary>
public class RecommendationWorkflowTransition
{
    public Guid Id { get; private set; }

    public Guid RecommendationWorkflowId { get; private set; }

    public string FromStatus { get; private set; } = string.Empty;

    public string ToStatus { get; private set; } = string.Empty;

    public string Actor { get; private set; } = string.Empty;

    public string? Comment { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    private RecommendationWorkflowTransition()
    {
    }

    internal static RecommendationWorkflowTransition Create(
        Guid recommendationWorkflowId,
        string fromStatus,
        string toStatus,
        string actor,
        string? comment,
        DateTimeOffset occurredAt)
    {
        if (recommendationWorkflowId == Guid.Empty)
        {
            throw new InvalidOperationException("RecommendationWorkflowId is mandatory for transitions.");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException("Actor is mandatory for workflow transitions.");
        }

        return new RecommendationWorkflowTransition
        {
            Id = Guid.NewGuid(),
            RecommendationWorkflowId = recommendationWorkflowId,
            FromStatus = RecommendationWorkflowStatus.Canonicalize(fromStatus),
            ToStatus = RecommendationWorkflowStatus.Canonicalize(toStatus),
            Actor = actor.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            OccurredAt = occurredAt
        };
    }
}
