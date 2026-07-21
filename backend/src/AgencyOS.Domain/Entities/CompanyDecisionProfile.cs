namespace AgencyOS.Domain.Entities;

public class CompanyDecisionProfile
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<DecisionProfileDimensionSetting> Dimensions { get; set; } =
        Array.Empty<DecisionProfileDimensionSetting>();
}

public class DecisionProfileDimensionSetting
{
    public string Dimension { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public bool PreferHigherValues { get; set; }
}
