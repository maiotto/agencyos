namespace AgencyOS.Application.DTOs;

public class DecisionQueryParameters
{
    public Guid? CompanyId { get; set; }

    public Guid? MissionId { get; set; }

    public Guid? ContractId { get; set; }

    public Guid? RecommendationId { get; set; }

    public string? DecisionStatus { get; set; }

    public string? ImplementationStatus { get; set; }

    public string? Search { get; set; }

    public DateTimeOffset? DecisionFrom { get; set; }

    public DateTimeOffset? DecisionTo { get; set; }

    public string? OrderBy { get; set; }

    public string? OrderDirection { get; set; }
}

public class CreateDecisionRequest
{
    public Guid RecommendationId { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? DecisionDate { get; set; }
}

public class DecisionActionRequest
{
    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }
}

public class RecordDecisionOutcomeRequest
{
    public string Outcome { get; set; } = string.Empty;

    public string? BusinessValue { get; set; }

    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }
}

public class DecisionTimelineEntryResponse
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string FromDecisionStatus { get; set; } = string.Empty;

    public string ToDecisionStatus { get; set; } = string.Empty;

    public string FromImplementationStatus { get; set; } = string.Empty;

    public string ToImplementationStatus { get; set; } = string.Empty;

    public string Actor { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}

public class DecisionResponse
{
    public Guid Id { get; set; }

    public Guid RecommendationId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid MissionId { get; set; }

    public Guid ContractId { get; set; }

    public string DecisionStatus { get; set; } = string.Empty;

    public string ImplementationStatus { get; set; } = string.Empty;

    public DateTimeOffset DecisionDate { get; set; }

    public DateTimeOffset? ImplementationDate { get; set; }

    public DateTimeOffset? CompletedDate { get; set; }

    public string? Outcome { get; set; }

    public string? BusinessValue { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IReadOnlyList<DecisionTimelineEntryResponse> Timeline { get; set; } = [];
}
