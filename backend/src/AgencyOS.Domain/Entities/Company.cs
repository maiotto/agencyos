namespace AgencyOS.Domain.Entities;

/// <summary>
/// Company aggregate (US-402 / BR-2001..BR-2010).
/// First-class multi-company tenant configuration. Historical data is never rewritten.
/// </summary>
public class Company
{
    public Guid Id { get; private set; }

    public string CompanyCode { get; private set; } = string.Empty;

    public string CompanyName { get; private set; } = string.Empty;

    public string? LegalName { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public string Timezone { get; private set; } = string.Empty;

    public string? Country { get; private set; }

    public string? Language { get; private set; }

    public string? Currency { get; private set; }

    /// <summary>JSON planning preferences blob (BR-2006 — config only; never rewrites history).</summary>
    public string PlanningConfiguration { get; private set; } = "{}";

    public Guid? DecisionProfileId { get; private set; }

    public Guid? DefaultCalendarId { get; private set; }

    public Guid? DefaultPlanningTemplateId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public bool Archived => CompanyStatus.IsArchived(Status);

    public bool IsActive => CompanyStatus.IsActive(Status);

    public bool IsInactive => CompanyStatus.IsInactive(Status);

    private Company()
    {
    }

    public static Company Create(
        Guid id,
        string companyCode,
        string companyName,
        string? legalName,
        string timezone,
        string? country,
        string? language,
        string? currency,
        string? planningConfiguration,
        Guid? decisionProfileId,
        Guid? defaultCalendarId,
        Guid? defaultPlanningTemplateId,
        DateTimeOffset createdAt)
    {
        ValidateIdentity(companyCode, companyName, timezone);

        return new Company
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            CompanyCode = companyCode.Trim(),
            CompanyName = companyName.Trim(),
            LegalName = NormalizeOptional(legalName),
            Status = CompanyStatus.Active,
            Timezone = timezone.Trim(),
            Country = NormalizeOptional(country),
            Language = NormalizeOptional(language),
            Currency = NormalizeOptional(currency),
            PlanningConfiguration = NormalizeJson(planningConfiguration),
            DecisionProfileId = decisionProfileId,
            DefaultCalendarId = defaultCalendarId,
            DefaultPlanningTemplateId = defaultPlanningTemplateId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void Update(
        string companyName,
        string? legalName,
        string timezone,
        string? country,
        string? language,
        string? currency,
        string? planningConfiguration,
        Guid? decisionProfileId,
        Guid? defaultCalendarId,
        Guid? defaultPlanningTemplateId,
        DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new InvalidOperationException("CompanyName is mandatory (BR-2001).");
        }

        if (string.IsNullOrWhiteSpace(timezone))
        {
            throw new InvalidOperationException("Timezone is mandatory.");
        }

        CompanyName = companyName.Trim();
        LegalName = NormalizeOptional(legalName);
        Timezone = timezone.Trim();
        Country = NormalizeOptional(country);
        Language = NormalizeOptional(language);
        Currency = NormalizeOptional(currency);
        PlanningConfiguration = NormalizeJson(planningConfiguration);
        DecisionProfileId = decisionProfileId;
        DefaultCalendarId = defaultCalendarId;
        DefaultPlanningTemplateId = defaultPlanningTemplateId;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        Status = CompanyStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        Status = CompanyStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Company is already archived.");
        }

        Status = CompanyStatus.Archived;
        ArchivedAt = archivedAt;
        UpdatedAt = archivedAt;
    }

    /// <summary>
    /// Validates that this company may be selected as the active company context (BR-2003).
    /// </summary>
    public void Select()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Only Active Companies may be selected (BR-2003).");
        }
    }

    public void AssignDecisionProfile(Guid decisionProfileId, DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        if (decisionProfileId == Guid.Empty)
        {
            throw new InvalidOperationException("DecisionProfileId is mandatory when assigning (BR-2007).");
        }

        DecisionProfileId = decisionProfileId;
        UpdatedAt = updatedAt;
    }

    public void AssignDefaultPlanningTemplate(Guid? planningTemplateId, DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        DefaultPlanningTemplateId = planningTemplateId;
        UpdatedAt = updatedAt;
    }

    public bool Validate()
    {
        try
        {
            ValidateIdentity(CompanyCode, CompanyName, Timezone);
            return CompanyStatus.IsKnown(Status);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private void EnsureNotArchived()
    {
        if (Archived)
        {
            throw new InvalidOperationException("Archived companies cannot be modified.");
        }
    }

    private static void ValidateIdentity(string companyCode, string companyName, string timezone)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
        {
            throw new InvalidOperationException("CompanyCode is mandatory (BR-2002).");
        }

        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new InvalidOperationException("CompanyName is mandatory (BR-2001).");
        }

        if (string.IsNullOrWhiteSpace(timezone))
        {
            throw new InvalidOperationException("Timezone is mandatory.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeJson(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
}

public static class CompanyStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Archived = "Archived";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Active, Inactive, Archived };

    public static bool IsKnown(string status) => All.Contains(status);

    public static bool IsActive(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);

    public static bool IsInactive(string status) =>
        string.Equals(status, Inactive, StringComparison.OrdinalIgnoreCase);

    public static bool IsArchived(string status) =>
        string.Equals(status, Archived, StringComparison.OrdinalIgnoreCase);
}

public static class AgencyOSCompanies
{
    /// <summary>Default seeded company used across Release 1.1 fixtures.</summary>
    public static readonly Guid DefaultCompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
}
