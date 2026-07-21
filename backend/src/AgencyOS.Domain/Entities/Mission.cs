namespace AgencyOS.Domain.Entities;

public class Mission
{
    public Guid Id { get; set; }

    public Guid ClientContractId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid MissionTypeId { get; set; }

    public Guid MissionStatusId { get; set; }

    public string Priority { get; set; } = "NORMAL";

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
