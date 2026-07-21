namespace AgencyOS.Domain.Entities;

public class ClientContact
{
    public Guid ClientId { get; set; }

    public Guid ContactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Client Client { get; set; } = null!;

    public Contact Contact { get; set; } = null!;
}
