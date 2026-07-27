namespace AgencyOS.Application.DTOs;

public class CompanyQueryParameters
{
    public string? Status { get; set; }

    public string? Search { get; set; }

    public string OrderBy { get; set; } = "companyName";

    public string OrderDirection { get; set; } = "asc";

    /// <summary>When false (default), Archived companies are excluded (BR-2009 allows opt-in inclusion).</summary>
    public bool IncludeArchived { get; set; }
}

public class CreateCompanyRequest
{
    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? LegalName { get; set; }

    public string Timezone { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? Language { get; set; }

    public string? Currency { get; set; }

    public string? PlanningConfiguration { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public Guid? DefaultCalendarId { get; set; }

    public Guid? DefaultPlanningTemplateId { get; set; }
}

public class UpdateCompanyRequest
{
    public string CompanyName { get; set; } = string.Empty;

    public string? LegalName { get; set; }

    public string Timezone { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? Language { get; set; }

    public string? Currency { get; set; }

    public string? PlanningConfiguration { get; set; }

    public Guid? DecisionProfileId { get; set; }

    public Guid? DefaultCalendarId { get; set; }

    public Guid? DefaultPlanningTemplateId { get; set; }
}

public class SelectCompanyRequest
{
    public Guid CompanyId { get; set; }
}

public class CompanyResponse
{
    public Guid Id { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? LegalName { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Timezone { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? Language { get; set; }

    public string? Currency { get; set; }

    public string PlanningConfiguration { get; set; } = "{}";

    public Guid? DecisionProfileId { get; set; }

    public Guid? DefaultCalendarId { get; set; }

    public Guid? DefaultPlanningTemplateId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }
}
