namespace AgencyOS.Domain.Entities;

public class Lead
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string? Website { get; set; }

    public string? Segment { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal? EstimatedRevenue { get; set; }

    public Guid? OwnerId { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<LeadContact> LeadContacts { get; set; } = [];
}
