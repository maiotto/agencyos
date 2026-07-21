namespace AgencyOS.Application.DTOs;

public class AllocationConflictResponse
{
    public Guid ConflictId { get; set; }

    public string ConflictType { get; set; } = string.Empty;

    public Guid ExecutionResourceId { get; set; }

    public string ExecutionResourceCode { get; set; } = string.Empty;

    public string ExecutionResourceName { get; set; } = string.Empty;

    public IReadOnlyList<AllocationConflictAssignmentReference> RelatedAssignments { get; set; } =
        Array.Empty<AllocationConflictAssignmentReference>();

    public string Severity { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SuggestedResolution { get; set; } = string.Empty;
}
