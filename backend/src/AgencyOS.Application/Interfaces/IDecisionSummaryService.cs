using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Read-only Decision Workspace section builder (US-504 / BR-2701..BR-2710). Projects existing
/// Decision, Decision Timeline, and Decision Audit data into thin, drill-down ready section DTOs.
/// Never calls CreateAsync/StartImplementationAsync/CompleteAsync/CancelAsync/RecordOutcomeAsync
/// on <see cref="IDecisionService"/> (DEC-504-001).
/// </summary>
public interface IDecisionSummaryService
{
    Task<DecisionsSectionResponse> GetDecisionsSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<DecisionTimelineSectionResponse> GetTimelineSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? decisionId,
        CancellationToken cancellationToken = default);

    Task<DecisionOutcomesSectionResponse> GetOutcomesSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<DecisionAuditSectionResponse> GetAuditSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);
}
