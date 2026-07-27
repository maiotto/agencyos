namespace AgencyOS.Application.DTOs;

public class CompanyDecisionProfileQueryParameters
{
    public Guid? CompanyId { get; set; }

    public string? Status { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public bool? DefaultProfile { get; set; }

    public string OrderBy { get; set; } = "name";

    public string OrderDirection { get; set; } = "asc";
}

public class DecisionProfileDimensionRequest
{
    public string Dimension { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public bool PreferHigherValues { get; set; }
}

public class CreateCompanyDecisionProfileRequest
{
    public Guid CompanyId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<DecisionProfileDimensionRequest> PriorityWeights { get; set; } = [];

    public decimal CapacityWeight { get; set; }

    public decimal WorkloadWeight { get; set; }

    public decimal CostWeight { get; set; }

    public decimal RiskWeight { get; set; }

    public decimal QualityWeight { get; set; }

    public string? PreferredStrategy { get; set; }

    public decimal? PreferredCapacityThreshold { get; set; }

    public decimal? PreferredWorkloadThreshold { get; set; }

    public bool DefaultProfile { get; set; }
}

public class UpdateCompanyDecisionProfileRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public List<DecisionProfileDimensionRequest> PriorityWeights { get; set; } = [];

    public decimal CapacityWeight { get; set; }

    public decimal WorkloadWeight { get; set; }

    public decimal CostWeight { get; set; }

    public decimal RiskWeight { get; set; }

    public decimal QualityWeight { get; set; }

    public string? PreferredStrategy { get; set; }

    public decimal? PreferredCapacityThreshold { get; set; }

    public decimal? PreferredWorkloadThreshold { get; set; }
}

public class CloneCompanyDecisionProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

public class CompanyDecisionProfileResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid ProfileFamilyId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public IReadOnlyList<DecisionProfileDimensionRequest> Dimensions { get; set; } =
        Array.Empty<DecisionProfileDimensionRequest>();

    public decimal CapacityWeight { get; set; }

    public decimal WorkloadWeight { get; set; }

    public decimal CostWeight { get; set; }

    public decimal RiskWeight { get; set; }

    public decimal QualityWeight { get; set; }

    public string? PreferredStrategy { get; set; }

    public decimal? PreferredCapacityThreshold { get; set; }

    public decimal? PreferredWorkloadThreshold { get; set; }

    public bool DefaultProfile { get; set; }

    public int Version { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }
}
