namespace AgencyOS.Application.DTOs;

public class ConvertLeadRequest
{
    public string LegalName { get; set; } = string.Empty;

    public string TaxIdentifier { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? Website { get; set; }

    public Guid? AccountOwner { get; set; }
}
