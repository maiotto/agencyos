namespace AgencyOS.Application.DTOs;

public class DeliveryStrategyResponse
{
    public Guid StrategyId { get; set; }

    public Guid ContractId { get; set; }

    public Guid MissionId { get; set; }

    public string StrategyName { get; set; } = string.Empty;

    public IReadOnlyList<DeliveryStrategyTaskAssignmentResponse> AssignedResources { get; set; } =
        Array.Empty<DeliveryStrategyTaskAssignmentResponse>();

    public DeliveryStrategyResourceMixResponse ResourceMix { get; set; } = new();

    public decimal EstimatedHours { get; set; }

    public decimal EstimatedCost { get; set; }

    public DeliveryStrategyPlanningMetadataResponse PlanningMetadata { get; set; } = new();
}
