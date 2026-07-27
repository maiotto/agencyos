using System.Globalization;
using System.Text.Json;

namespace AgencyOS.Domain.Entities;

/// <summary>
/// Read-only Recommendation Comparison model (US-204 / BR-1301..BR-1306).
/// Built from immutable RecommendationHistory snapshots. Never mutates Recommendations.
/// </summary>
public sealed class RecommendationComparison
{
    private RecommendationComparison(
        RecommendationHistory left,
        RecommendationHistory right,
        IReadOnlyList<RecommendationComparisonDifference> differences)
    {
        Left = left;
        Right = right;
        Differences = differences;
    }

    public RecommendationHistory Left { get; }

    public RecommendationHistory Right { get; }

    public IReadOnlyList<RecommendationComparisonDifference> Differences { get; }

    public bool HasDifferences => Differences.Any(difference => difference.Changed);

    public decimal? ScoreDelta =>
        Left.Score.HasValue || Right.Score.HasValue
            ? (Right.Score ?? 0m) - (Left.Score ?? 0m)
            : null;

    public int? RankDelta =>
        Left.Rank.HasValue || Right.Rank.HasValue
            ? (Right.Rank ?? 0) - (Left.Rank ?? 0)
            : null;

    public int VersionDelta => Right.RecommendationVersion - Left.RecommendationVersion;

    public static RecommendationComparison Compare(
        RecommendationHistory left,
        RecommendationHistory right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (left.Id == right.Id)
        {
            throw new InvalidOperationException("Left and right history snapshots must be different.");
        }

        var differences = new List<RecommendationComparisonDifference>();

        AddScalar(differences, RecommendationComparisonSections.Metadata, "recommendationNumber",
            left.RecommendationNumber, right.RecommendationNumber);
        AddScalar(differences, RecommendationComparisonSections.Metadata, "recommendationVersion",
            left.RecommendationVersion.ToString(CultureInfo.InvariantCulture),
            right.RecommendationVersion.ToString(CultureInfo.InvariantCulture));
        AddScalar(differences, RecommendationComparisonSections.Metadata, "recommendationId",
            left.RecommendationId.ToString(), right.RecommendationId.ToString());
        AddScalar(differences, RecommendationComparisonSections.Metadata, "companyId",
            left.CompanyId.ToString(), right.CompanyId.ToString());
        AddScalar(differences, RecommendationComparisonSections.Metadata, "missionId",
            left.MissionId.ToString(), right.MissionId.ToString());
        AddScalar(differences, RecommendationComparisonSections.Metadata, "contractId",
            left.ContractId.ToString(), right.ContractId.ToString());
        AddScalar(differences, RecommendationComparisonSections.Metadata, "title",
            left.Title, right.Title);
        AddScalar(differences, RecommendationComparisonSections.Metadata, "summary",
            left.Summary, right.Summary);
        AddScalar(differences, RecommendationComparisonSections.Metadata, "recommendationStatus",
            left.RecommendationStatus, right.RecommendationStatus);
        AddScalar(differences, RecommendationComparisonSections.Metadata, "planningTemplateId",
            left.PlanningTemplateId?.ToString(), right.PlanningTemplateId?.ToString());
        AddScalar(differences, RecommendationComparisonSections.Metadata, "decisionEngineVersion",
            left.DecisionEngineVersion, right.DecisionEngineVersion);
        AddScalar(differences, RecommendationComparisonSections.Metadata, "score",
            FormatDecimal(left.Score), FormatDecimal(right.Score));
        AddScalar(differences, RecommendationComparisonSections.Metadata, "rank",
            left.Rank?.ToString(CultureInfo.InvariantCulture),
            right.Rank?.ToString(CultureInfo.InvariantCulture));

        AddScalar(differences, RecommendationComparisonSections.DeliveryStrategy, "deliveryStrategyId",
            left.DeliveryStrategyId.ToString(), right.DeliveryStrategyId.ToString());

        AddScalar(differences, RecommendationComparisonSections.Workflow, "workflowStatus",
            left.WorkflowStatus, right.WorkflowStatus);
        AddScalar(differences, RecommendationComparisonSections.Workflow, "workflowId",
            left.WorkflowId?.ToString(), right.WorkflowId?.ToString());
        AddScalar(differences, RecommendationComparisonSections.Workflow, "approver",
            left.Approver, right.Approver);
        AddScalar(differences, RecommendationComparisonSections.Workflow, "approvalDate",
            left.ApprovalDate?.ToString("O", CultureInfo.InvariantCulture),
            right.ApprovalDate?.ToString("O", CultureInfo.InvariantCulture));
        AddScalar(differences, RecommendationComparisonSections.Workflow, "approvalComment",
            left.ApprovalComment, right.ApprovalComment);

        DiffJson(
            differences,
            RecommendationComparisonSections.Capacity,
            left.CapacitySnapshot,
            right.CapacitySnapshot);
        DiffJson(
            differences,
            RecommendationComparisonSections.Workload,
            left.WorkloadSnapshot,
            right.WorkloadSnapshot);
        DiffJson(
            differences,
            RecommendationComparisonSections.Payload,
            left.RecommendationPayload,
            right.RecommendationPayload);

        return new RecommendationComparison(left, right, differences);
    }

    private static void AddScalar(
        ICollection<RecommendationComparisonDifference> differences,
        string section,
        string path,
        string? leftValue,
        string? rightValue)
    {
        differences.Add(new RecommendationComparisonDifference(
            section,
            path,
            leftValue,
            rightValue,
            !string.Equals(leftValue, rightValue, StringComparison.Ordinal)));
    }

    private static string? FormatDecimal(decimal? value) =>
        value?.ToString(CultureInfo.InvariantCulture);

    private static void DiffJson(
        ICollection<RecommendationComparisonDifference> differences,
        string section,
        string leftJson,
        string rightJson)
    {
        try
        {
            using var leftDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(leftJson) ? "null" : leftJson);
            using var rightDocument = JsonDocument.Parse(string.IsNullOrWhiteSpace(rightJson) ? "null" : rightJson);
            DiffElements(differences, section, "$", leftDocument.RootElement, rightDocument.RootElement);
        }
        catch (JsonException)
        {
            AddScalar(differences, section, "$", leftJson, rightJson);
        }
    }

    private static void DiffElements(
        ICollection<RecommendationComparisonDifference> differences,
        string section,
        string path,
        JsonElement left,
        JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            AddScalar(differences, section, path, SerializeElement(left), SerializeElement(right));
            return;
        }

        switch (left.ValueKind)
        {
            case JsonValueKind.Object:
                var leftProps = left.EnumerateObject().ToDictionary(property => property.Name, property => property.Value);
                var rightProps = right.EnumerateObject().ToDictionary(property => property.Name, property => property.Value);
                foreach (var key in leftProps.Keys.Union(rightProps.Keys).OrderBy(name => name, StringComparer.Ordinal))
                {
                    var childPath = path == "$" ? key : $"{path}.{key}";
                    leftProps.TryGetValue(key, out var leftChild);
                    rightProps.TryGetValue(key, out var rightChild);

                    if (!leftProps.ContainsKey(key))
                    {
                        AddScalar(differences, section, childPath, null, SerializeElement(rightChild));
                    }
                    else if (!rightProps.ContainsKey(key))
                    {
                        AddScalar(differences, section, childPath, SerializeElement(leftChild), null);
                    }
                    else
                    {
                        DiffElements(differences, section, childPath, leftChild, rightChild);
                    }
                }

                break;

            case JsonValueKind.Array:
                var leftItems = left.EnumerateArray().ToList();
                var rightItems = right.EnumerateArray().ToList();
                var max = Math.Max(leftItems.Count, rightItems.Count);
                for (var index = 0; index < max; index++)
                {
                    var childPath = $"{path}[{index}]";
                    if (index >= leftItems.Count)
                    {
                        AddScalar(differences, section, childPath, null, SerializeElement(rightItems[index]));
                    }
                    else if (index >= rightItems.Count)
                    {
                        AddScalar(differences, section, childPath, SerializeElement(leftItems[index]), null);
                    }
                    else
                    {
                        DiffElements(differences, section, childPath, leftItems[index], rightItems[index]);
                    }
                }

                break;

            default:
                var leftValue = SerializeElement(left);
                var rightValue = SerializeElement(right);
                AddScalar(differences, section, path, leftValue, rightValue);
                break;
        }
    }

    private static string SerializeElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => "null",
            JsonValueKind.Undefined => "undefined",
            _ => element.GetRawText()
        };
}

public static class RecommendationComparisonSections
{
    public const string Metadata = "Metadata";
    public const string Capacity = "Capacity";
    public const string Workload = "Workload";
    public const string Payload = "Payload";
    public const string DeliveryStrategy = "DeliveryStrategy";
    public const string Workflow = "Workflow";
}

public sealed class RecommendationComparisonDifference
{
    public RecommendationComparisonDifference(
        string section,
        string path,
        string? leftValue,
        string? rightValue,
        bool changed)
    {
        Section = section;
        Path = path;
        LeftValue = leftValue;
        RightValue = rightValue;
        Changed = changed;
    }

    public string Section { get; }

    public string Path { get; }

    public string? LeftValue { get; }

    public string? RightValue { get; }

    public bool Changed { get; }
}
