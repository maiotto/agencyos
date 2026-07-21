namespace AgencyOS.Application.DTOs;

public class CreateClientRequest
{
    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? TaxIdentifier { get; set; }

    public string? Website { get; set; }

    public string? Industry { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? AccountOwner { get; set; }
}
