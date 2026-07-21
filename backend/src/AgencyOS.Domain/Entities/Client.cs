namespace AgencyOS.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }

    public Guid? LeadId { get; set; }

    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? TaxId { get; set; }

    public string? Website { get; set; }

    public string? Segment { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? AccountOwner { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Lead? Lead { get; set; }

    public ICollection<ClientContact> ClientContacts { get; set; } = [];
}
