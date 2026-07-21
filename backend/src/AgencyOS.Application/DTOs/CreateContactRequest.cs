namespace AgencyOS.Application.DTOs;

public class CreateContactRequest
{
    public Guid ClientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string? JobTitle { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public bool IsPrimaryContact { get; set; }

    public string Status { get; set; } = string.Empty;
}
