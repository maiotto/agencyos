using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Read-only Decision Workspace section builder (US-504 / BR-2701..BR-2710). Projects existing
/// Decision, Decision Timeline, and Decision Audit data into thin, drill-down ready section DTOs.
/// Never calls CreateAsync/StartImplementationAsync/CompleteAsync/CancelAsync/RecordOutcomeAsync
/// on <see cref="IDecisionService"/> (DEC-504-001).
/// </summary>
public class DecisionSummaryService : IDecisionSummaryService
{
    private const int DecisionCardLimit = 50;
    private const int RecentDecisionsForTimelineLimit = 20;
    private const int TimelineItemLimit = 50;
    private const int OutcomeCardLimit = 50;

    private readonly IDecisionService _decisionService;
    private readonly IAuditQueryService _auditQueryService;

    public DecisionSummaryService(IDecisionService decisionService, IAuditQueryService auditQueryService)
    {
        _decisionService = decisionService;
        _auditQueryService = auditQueryService;
    }

    public async Task<DecisionsSectionResponse> GetDecisionsSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _decisionService.GetAllAsync(
            new DecisionQueryParameters { CompanyId = companyId, DecisionFrom = from, DecisionTo = to },
            cancellationToken);

        var cards = decisions
            .OrderByDescending(decision => decision.DecisionDate)
            .Select(ToCard)
            .ToList();

        return new DecisionsSectionResponse
        {
            CompanyId = companyId,
            TotalCount = decisions.Count,
            Pending = cards
                .Where(card => DecisionStatus.IsCreated(card.DecisionStatus))
                .Take(DecisionCardLimit)
                .ToList(),
            InProgress = cards
                .Where(card => DecisionStatus.IsInProgress(card.DecisionStatus))
                .Take(DecisionCardLimit)
                .ToList(),
            Completed = cards
                .Where(card => DecisionStatus.IsCompleted(card.DecisionStatus))
                .Take(DecisionCardLimit)
                .ToList(),
            Cancelled = cards
                .Where(card => DecisionStatus.IsCancelled(card.DecisionStatus))
                .Take(DecisionCardLimit)
                .ToList(),
            ListAction = new DecisionActionResponse
            {
                Key = "list-decisions",
                Label = "List Decisions",
                Category = "Decisions",
                DrillDownPath = $"/decisions?companyId={companyId}",
                Description = "Browse Decisions for this Company."
            },
            CreateAction = new DecisionActionResponse
            {
                Key = "create-decision",
                Label = "Create Decision",
                Category = "Decisions",
                DrillDownPath = "/decisions/new",
                Description = "Create a Decision from an Approved Recommendation. Always requires explicit human action (DEC-504-001).",
                RequiresHumanApproval = true
            }
        };
    }

    public async Task<DecisionTimelineSectionResponse> GetTimelineSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? decisionId,
        CancellationToken cancellationToken = default)
    {
        var action = new DecisionActionResponse
        {
            Key = "timeline",
            Label = "Timeline",
            Category = "Decisions",
            DrillDownPath = decisionId.HasValue ? $"/decisions/{decisionId.Value}" : $"/decisions?companyId={companyId}",
            Description = "Browse the immutable Decision Timeline (BR-2703)."
        };

        if (decisionId.HasValue)
        {
            var decision = await _decisionService.GetByIdAsync(decisionId.Value, cancellationToken);
            if (decision.CompanyId != companyId)
            {
                throw new NotFoundException(
                    $"Decision with id '{decisionId.Value}' was not found for company '{companyId}'.");
            }

            var focusedItems = decision.Timeline
                .OrderByDescending(entry => entry.OccurredAt)
                .Select(entry => ToTimelineItem(decision, entry))
                .ToList();

            return new DecisionTimelineSectionResponse
            {
                CompanyId = companyId,
                DecisionId = decisionId,
                Items = focusedItems,
                Action = action
            };
        }

        var decisions = await _decisionService.GetAllAsync(
            new DecisionQueryParameters
            {
                CompanyId = companyId,
                DecisionFrom = from,
                DecisionTo = to,
                OrderBy = "UpdatedAt",
                OrderDirection = "desc"
            },
            cancellationToken);

        var aggregatedItems = decisions
            .Take(RecentDecisionsForTimelineLimit)
            .SelectMany(decision => decision.Timeline.Select(entry => ToTimelineItem(decision, entry)))
            .OrderByDescending(item => item.OccurredAt)
            .Take(TimelineItemLimit)
            .ToList();

        return new DecisionTimelineSectionResponse
        {
            CompanyId = companyId,
            DecisionId = null,
            Items = aggregatedItems,
            Action = action
        };
    }

    public async Task<DecisionOutcomesSectionResponse> GetOutcomesSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var decisions = await _decisionService.GetAllAsync(
            new DecisionQueryParameters { CompanyId = companyId, DecisionFrom = from, DecisionTo = to },
            cancellationToken);

        var outcomes = decisions
            .Where(decision => !string.IsNullOrWhiteSpace(decision.Outcome))
            .OrderByDescending(decision => decision.CompletedDate ?? decision.DecisionDate)
            .Take(OutcomeCardLimit)
            .Select(decision => new DecisionOutcomeCardResponse
            {
                Id = decision.Id,
                RecommendationId = decision.RecommendationId,
                DecisionStatus = decision.DecisionStatus,
                ImplementationStatus = decision.ImplementationStatus,
                Outcome = decision.Outcome!,
                BusinessValue = decision.BusinessValue,
                CompletedDate = decision.CompletedDate,
                DrillDownPath = $"/decisions/{decision.Id}",
                RecommendationDrillDownPath = $"/recommendations/{decision.RecommendationId}"
            })
            .ToList();

        return new DecisionOutcomesSectionResponse
        {
            CompanyId = companyId,
            Outcomes = outcomes,
            RecordOutcomeAction = new DecisionActionResponse
            {
                Key = "record-outcome",
                Label = "Record Outcome",
                Category = "Outcomes",
                DrillDownPath = "/decisions",
                Description = "Recording an outcome always requires explicit human action on the Decision detail page (DEC-504-001).",
                RequiresHumanApproval = true
            }
        };
    }

    public async Task<DecisionAuditSectionResponse> GetAuditSectionAsync(
        Guid companyId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditQueryService.GetAllAsync(
            new AuditEventQueryParameters
            {
                EntityType = AuditEntityTypes.Decision,
                CompanyId = companyId,
                OccurredFrom = from,
                OccurredTo = to
            },
            cancellationToken);

        var items = events
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .Select(auditEvent => new DecisionAuditItemResponse
            {
                Id = auditEvent.Id,
                EntityId = auditEvent.EntityId,
                EventType = auditEvent.EventType,
                Action = auditEvent.Action,
                OccurredAt = auditEvent.OccurredAt,
                UserId = auditEvent.UserId,
                DrillDownPath = $"/audit/{auditEvent.Id}"
            })
            .ToList();

        return new DecisionAuditSectionResponse
        {
            CompanyId = companyId,
            From = from,
            To = to,
            Items = items,
            Action = new DecisionActionResponse
            {
                Key = "audit",
                Label = "Audit Trail",
                Category = "Audit",
                DrillDownPath = $"/audit?companyId={companyId}",
                Description = "Full, immutable Audit Trail for Decisions (BR-2704)."
            }
        };
    }

    private static DecisionCardResponse ToCard(DecisionResponse decision) =>
        new()
        {
            Id = decision.Id,
            RecommendationId = decision.RecommendationId,
            CompanyId = decision.CompanyId,
            MissionId = decision.MissionId,
            ContractId = decision.ContractId,
            DecisionStatus = decision.DecisionStatus,
            ImplementationStatus = decision.ImplementationStatus,
            DecisionDate = decision.DecisionDate,
            ImplementationDate = decision.ImplementationDate,
            CompletedDate = decision.CompletedDate,
            Outcome = decision.Outcome,
            BusinessValue = decision.BusinessValue,
            CreatedBy = decision.CreatedBy,
            DrillDownPath = $"/decisions/{decision.Id}",
            RecommendationDrillDownPath = $"/recommendations/{decision.RecommendationId}"
        };

    private static DecisionTimelineItemResponse ToTimelineItem(
        DecisionResponse decision,
        DecisionTimelineEntryResponse entry) =>
        new()
        {
            DecisionId = decision.Id,
            RecommendationId = decision.RecommendationId,
            EventType = entry.EventType,
            FromDecisionStatus = entry.FromDecisionStatus,
            ToDecisionStatus = entry.ToDecisionStatus,
            FromImplementationStatus = entry.FromImplementationStatus,
            ToImplementationStatus = entry.ToImplementationStatus,
            Actor = entry.Actor,
            Comment = entry.Comment,
            OccurredAt = entry.OccurredAt,
            DrillDownPath = $"/decisions/{decision.Id}"
        };
}
