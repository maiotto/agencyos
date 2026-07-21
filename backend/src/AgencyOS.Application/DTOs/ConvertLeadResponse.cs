namespace AgencyOS.Application.DTOs;

public class ConvertLeadResponse
{
    public LeadResponse Lead { get; set; } = null!;

    public Guid ClientId { get; set; }
}
