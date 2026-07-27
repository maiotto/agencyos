namespace AgencyOS.Application.DTOs;

public class RecommendationComparisonQueryParameters
{
    public Guid LeftId { get; set; }

    public Guid RightId { get; set; }
}

public class RecommendationVersionComparisonQueryParameters
{
    public int? LeftVersion { get; set; }

    public int? RightVersion { get; set; }
}

public class RecommendationComparisonFieldDiffResponse
{
    public string Section { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? LeftValue { get; set; }

    public string? RightValue { get; set; }

    public bool Changed { get; set; }
}

public class RecommendationComparisonSectionResponse
{
    public string Section { get; set; } = string.Empty;

    public bool HasDifferences { get; set; }

    public IReadOnlyList<RecommendationComparisonFieldDiffResponse> Fields { get; set; } = [];
}

public class RecommendationComparisonResponse
{
    public RecommendationHistoryResponse Left { get; set; } = null!;

    public RecommendationHistoryResponse Right { get; set; } = null!;

    public bool HasDifferences { get; set; }

    public decimal? ScoreDelta { get; set; }

    public int? RankDelta { get; set; }

    public int VersionDelta { get; set; }

    public IReadOnlyList<RecommendationComparisonFieldDiffResponse> Differences { get; set; } = [];

    public IReadOnlyList<RecommendationComparisonSectionResponse> Sections { get; set; } = [];
}
