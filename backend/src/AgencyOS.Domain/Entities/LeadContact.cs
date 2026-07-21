namespace AgencyOS.Domain.Entities;

public class LeadContact
{
    public Guid LeadId { get; set; }

    public Guid ContactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Lead Lead { get; set; } = null!;

    public Contact Contact { get; set; } = null!;
}
