using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IDecisionService
{
    Task<IReadOnlyList<DecisionResponse>> GetAllAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DecisionResponse>> FilterAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DecisionTimelineEntryResponse>> GetTimelineAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> CreateAsync(
        CreateDecisionRequest request,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> StartImplementationAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> CompleteAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> CancelAsync(
        Guid id,
        DecisionActionRequest request,
        CancellationToken cancellationToken = default);

    Task<DecisionResponse> RecordOutcomeAsync(
        Guid id,
        RecordDecisionOutcomeRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Timeline queries for Decision Tracking (US-205).
/// </summary>
public interface IDecisionTimelineService
{
    Task<IReadOnlyList<DecisionTimelineEntryResponse>> GetTimelineAsync(
        Guid decisionId,
        CancellationToken cancellationToken = default);
}
