namespace AgencyOS.Infrastructure.Configuration;

public class CompanyDecisionProfilesOptions
{
    public const string SectionName = "CompanyDecisionProfiles";

    public List<CompanyDecisionProfileOptions> Profiles { get; set; } = [];
}

public class CompanyDecisionProfileOptions
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<DecisionProfileDimensionOptions> Dimensions { get; set; } = [];
}

public class DecisionProfileDimensionOptions
{
    public string Dimension { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public bool PreferHigherValues { get; set; }
}
