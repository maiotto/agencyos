namespace AgencyOS.Domain.Entities;

public class Contact
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? JobTitle { get; set; }

    public bool IsPrimary { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<LeadContact> LeadContacts { get; set; } = [];
}
