namespace AgencyOS.Application.DTOs;

public class ClientResponse
{
    public Guid Id { get; set; }

    public Guid? LeadId { get; set; }

    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? TaxIdentifier { get; set; }

    public string? Website { get; set; }

    public string? Industry { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? AccountOwner { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
