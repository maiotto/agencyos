using System.Text.Json;

namespace AgencyOS.Domain.Entities;

/// <summary>
/// Company Decision Profile aggregate (US-401 / BR-1901..BR-1910).
/// Influences ranking and AI Decision Support without changing Decision Engine architecture.
/// </summary>
public class CompanyDecisionProfile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    /// <summary>Stable lineage id across versions of the same profile.</summary>
    public Guid ProfileFamilyId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Status { get; private set; } = string.Empty;

    /// <summary>JSON array of ranking dimensions (weight + preferHigherValues).</summary>
    public string PriorityWeights { get; private set; } = "[]";

    public decimal CapacityWeight { get; private set; }

    public decimal WorkloadWeight { get; private set; }

    public decimal CostWeight { get; private set; }

    public decimal RiskWeight { get; private set; }

    public decimal QualityWeight { get; private set; }

    public string? PreferredStrategy { get; private set; }

    public decimal? PreferredCapacityThreshold { get; private set; }

    public decimal? PreferredWorkloadThreshold { get; private set; }

    public bool DefaultProfile { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public bool Archived => CompanyDecisionProfileStatus.IsArchived(Status);

    public bool IsActive => CompanyDecisionProfileStatus.IsActive(Status);

    public bool IsInactive => CompanyDecisionProfileStatus.IsInactive(Status);

    /// <summary>Ranking dimensions derived from PriorityWeights (ADR-007 compatibility).</summary>
    public IReadOnlyList<DecisionProfileDimensionSetting> Dimensions =>
        DeserializeDimensions(PriorityWeights);

    private CompanyDecisionProfile()
    {
    }

    public static CompanyDecisionProfile Create(
        Guid companyId,
        string code,
        string name,
        string? description,
        string priorityWeightsJson,
        decimal capacityWeight,
        decimal workloadWeight,
        decimal costWeight,
        decimal riskWeight,
        decimal qualityWeight,
        string? preferredStrategy,
        decimal? preferredCapacityThreshold,
        decimal? preferredWorkloadThreshold,
        bool defaultProfile,
        DateTimeOffset createdAt)
    {
        ValidateInputs(
            companyId,
            code,
            name,
            priorityWeightsJson,
            capacityWeight,
            workloadWeight,
            costWeight,
            riskWeight,
            qualityWeight,
            preferredCapacityThreshold,
            preferredWorkloadThreshold,
            version: 1);

        return new CompanyDecisionProfile
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ProfileFamilyId = Guid.NewGuid(),
            Code = code.Trim(),
            Name = name.Trim(),
            Description = NormalizeOptional(description),
            Status = CompanyDecisionProfileStatus.Active,
            PriorityWeights = NormalizeJson(priorityWeightsJson),
            CapacityWeight = Round(capacityWeight),
            WorkloadWeight = Round(workloadWeight),
            CostWeight = Round(costWeight),
            RiskWeight = Round(riskWeight),
            QualityWeight = Round(qualityWeight),
            PreferredStrategy = NormalizeOptional(preferredStrategy),
            PreferredCapacityThreshold = preferredCapacityThreshold,
            PreferredWorkloadThreshold = preferredWorkloadThreshold,
            DefaultProfile = defaultProfile,
            Version = 1,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    /// <summary>
    /// Lightweight ranking snapshot for deterministic scoring tests and in-memory ranking views.
    /// </summary>
    public static CompanyDecisionProfile CreateRankingView(
        Guid id,
        string code,
        string name,
        IReadOnlyList<DecisionProfileDimensionSetting> dimensions)
    {
        ArgumentNullException.ThrowIfNull(dimensions);

        return new CompanyDecisionProfile
        {
            Id = id,
            CompanyId = Guid.Empty,
            ProfileFamilyId = id,
            Code = code,
            Name = name,
            Status = CompanyDecisionProfileStatus.Active,
            PriorityWeights = SerializeDimensions(dimensions),
            Version = 1,
            CreatedAt = DateTimeOffset.UnixEpoch,
            UpdatedAt = DateTimeOffset.UnixEpoch
        };
    }

    /// <summary>
    /// Creates a new immutable version with updated configuration (BR-1905). Does not mutate this instance.
    /// </summary>
    public CompanyDecisionProfile CreateNewVersion(
        string? name,
        string? description,
        string priorityWeightsJson,
        decimal capacityWeight,
        decimal workloadWeight,
        decimal costWeight,
        decimal riskWeight,
        decimal qualityWeight,
        string? preferredStrategy,
        decimal? preferredCapacityThreshold,
        decimal? preferredWorkloadThreshold,
        DateTimeOffset updatedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Archived profiles cannot create new versions.");
        }

        ValidateInputs(
            CompanyId,
            Code,
            string.IsNullOrWhiteSpace(name) ? Name : name,
            priorityWeightsJson,
            capacityWeight,
            workloadWeight,
            costWeight,
            riskWeight,
            qualityWeight,
            preferredCapacityThreshold,
            preferredWorkloadThreshold,
            Version + 1);

        return new CompanyDecisionProfile
        {
            Id = Guid.NewGuid(),
            CompanyId = CompanyId,
            ProfileFamilyId = ProfileFamilyId,
            Code = Code,
            Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim(),
            Description = description is null ? Description : NormalizeOptional(description),
            Status = CompanyDecisionProfileStatus.Active,
            PriorityWeights = NormalizeJson(priorityWeightsJson),
            CapacityWeight = Round(capacityWeight),
            WorkloadWeight = Round(workloadWeight),
            CostWeight = Round(costWeight),
            RiskWeight = Round(riskWeight),
            QualityWeight = Round(qualityWeight),
            PreferredStrategy = preferredStrategy is null
                ? PreferredStrategy
                : NormalizeOptional(preferredStrategy),
            PreferredCapacityThreshold = preferredCapacityThreshold,
            PreferredWorkloadThreshold = preferredWorkloadThreshold,
            DefaultProfile = DefaultProfile,
            Version = Version + 1,
            CreatedAt = CreatedAt,
            UpdatedAt = updatedAt
        };
    }

    public CompanyDecisionProfile Clone(string clonedName, string clonedCode, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(clonedName))
        {
            throw new InvalidOperationException("Cloned profile Name is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(clonedCode))
        {
            throw new InvalidOperationException("Cloned profile Code is mandatory.");
        }

        return new CompanyDecisionProfile
        {
            Id = Guid.NewGuid(),
            CompanyId = CompanyId,
            ProfileFamilyId = Guid.NewGuid(),
            Code = clonedCode.Trim(),
            Name = clonedName.Trim(),
            Description = Description,
            Status = CompanyDecisionProfileStatus.Active,
            PriorityWeights = PriorityWeights,
            CapacityWeight = CapacityWeight,
            WorkloadWeight = WorkloadWeight,
            CostWeight = CostWeight,
            RiskWeight = RiskWeight,
            QualityWeight = QualityWeight,
            PreferredStrategy = PreferredStrategy,
            PreferredCapacityThreshold = PreferredCapacityThreshold,
            PreferredWorkloadThreshold = PreferredWorkloadThreshold,
            DefaultProfile = false,
            Version = 1,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        Status = CompanyDecisionProfileStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        if (DefaultProfile)
        {
            throw new InvalidOperationException("Default profile cannot be deactivated. Set another default first.");
        }

        Status = CompanyDecisionProfileStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (Archived)
        {
            throw new InvalidOperationException("Profile is already archived.");
        }

        if (DefaultProfile)
        {
            throw new InvalidOperationException("Default profile cannot be archived. Set another default first.");
        }

        Status = CompanyDecisionProfileStatus.Archived;
        ArchivedAt = archivedAt;
        UpdatedAt = archivedAt;
    }

    public void SetDefault(bool isDefault, DateTimeOffset updatedAt)
    {
        EnsureNotArchived();
        if (isDefault && !IsActive)
        {
            throw new InvalidOperationException("Only Active profiles can be set as default (BR-1901 / BR-1904).");
        }

        DefaultProfile = isDefault;
        UpdatedAt = updatedAt;
    }

    public void ClearDefault(DateTimeOffset updatedAt)
    {
        DefaultProfile = false;
        UpdatedAt = updatedAt;
    }

    public bool Validate()
    {
        try
        {
            ValidateInputs(
                CompanyId == Guid.Empty && Dimensions.Count > 0 ? Guid.NewGuid() : CompanyId,
                string.IsNullOrWhiteSpace(Code) ? "CODE" : Code,
                Name,
                PriorityWeights,
                CapacityWeight,
                WorkloadWeight,
                CostWeight,
                RiskWeight,
                QualityWeight,
                PreferredCapacityThreshold,
                PreferredWorkloadThreshold,
                Version <= 0 ? 1 : Version);
            return CompanyDecisionProfileStatus.IsKnown(Status) || CompanyId == Guid.Empty;
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
            throw new InvalidOperationException("Archived profiles cannot be modified.");
        }
    }

    private static void ValidateInputs(
        Guid companyId,
        string code,
        string name,
        string priorityWeightsJson,
        decimal capacityWeight,
        decimal workloadWeight,
        decimal costWeight,
        decimal riskWeight,
        decimal qualityWeight,
        decimal? preferredCapacityThreshold,
        decimal? preferredWorkloadThreshold,
        int version)
    {
        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Code is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Name is mandatory (BR-1902).");
        }

        if (version < 1)
        {
            throw new InvalidOperationException("Version must be >= 1 (BR-1905).");
        }

        var dimensions = DeserializeDimensions(priorityWeightsJson);
        if (dimensions.Count == 0 || dimensions.All(dimension => dimension.Weight <= 0))
        {
            throw new InvalidOperationException(
                "PriorityWeights must include at least one dimension with a positive weight.");
        }

        ValidateWeight(capacityWeight, nameof(CapacityWeight));
        ValidateWeight(workloadWeight, nameof(WorkloadWeight));
        ValidateWeight(costWeight, nameof(CostWeight));
        ValidateWeight(riskWeight, nameof(RiskWeight));
        ValidateWeight(qualityWeight, nameof(QualityWeight));

        if (preferredCapacityThreshold is < 0 or > 100)
        {
            throw new InvalidOperationException("PreferredCapacityThreshold must be between 0 and 100.");
        }

        if (preferredWorkloadThreshold is < 0 or > 100)
        {
            throw new InvalidOperationException("PreferredWorkloadThreshold must be between 0 and 100.");
        }
    }

    private static void ValidateWeight(decimal weight, string name)
    {
        if (weight is < 0 or > 1)
        {
            throw new InvalidOperationException($"{name} must be between 0 and 1.");
        }
    }

    private static decimal Round(decimal value) =>
        decimal.Round(value, 4, MidpointRounding.AwayFromZero);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeJson(string json)
    {
        var dimensions = DeserializeDimensions(json);
        return SerializeDimensions(dimensions);
    }

    public static string SerializeDimensions(IReadOnlyList<DecisionProfileDimensionSetting> dimensions) =>
        JsonSerializer.Serialize(dimensions, JsonOptions);

    public static IReadOnlyList<DecisionProfileDimensionSetting> DeserializeDimensions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<DecisionProfileDimensionSetting>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<DecisionProfileDimensionSetting>>(json, JsonOptions);
            return items ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("PriorityWeights must be valid JSON.", ex);
        }
    }
}

public class DecisionProfileDimensionSetting
{
    public string Dimension { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public bool PreferHigherValues { get; set; }
}

public static class CompanyDecisionProfileStatus
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
