namespace AgencyOS.Domain.Entities;

public class ClientContract
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public string ContractNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string BillingModel { get; set; } = string.Empty;

    public decimal? Value { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly? RenewalDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
}
