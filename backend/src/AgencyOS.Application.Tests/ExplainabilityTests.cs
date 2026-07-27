using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class ExplainabilityTests
{
    [Fact]
    public void Generate_CreatesActiveRecommendationExplanation()
    {
        var item = CreateRecommendationExplanation();

        Assert.Equal(ExplainabilityStatus.Active, item.Status);
        Assert.Equal(ExplainabilityTypes.Recommendation, item.ExplanationType);
        Assert.Null(item.AIRecommendationId);
        Assert.Equal(1, item.GenerationVersion);
        Assert.True(item.Validate());
    }

    [Fact]
    public void Generate_CreatesActiveAIRecommendationExplanation()
    {
        var aiId = Guid.NewGuid();
        var item = CreateAIExplanation(aiId);

        Assert.Equal(ExplainabilityTypes.AIRecommendation, item.ExplanationType);
        Assert.Equal(aiId, item.AIRecommendationId);
        Assert.True(item.Validate());
    }

    [Fact]
    public void Archive_MarksArchived()
    {
        var item = CreateRecommendationExplanation();
        item.Archive(DateTimeOffset.UtcNow);

        Assert.True(item.Archived);
        Assert.NotNull(item.ArchivedAt);
        Assert.Throws<InvalidOperationException>(() => item.Archive(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Generate_RejectsMissingMandatoryFields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Explainability.Generate(
                Guid.NewGuid(),
                null,
                ExplainabilityTypes.Recommendation,
                1,
                DateTimeOffset.UtcNow,
                "user",
                " ",
                "detailed",
                "[]",
                "[]",
                "[]",
                "confidence",
                "capacity",
                "workload",
                ExplainabilityVersions.CurrentModelVersion,
                ExplainabilityVersions.CurrentPromptVersion));

        Assert.Throws<InvalidOperationException>(() =>
            Explainability.Generate(
                Guid.NewGuid(),
                null,
                ExplainabilityTypes.Recommendation,
                1,
                DateTimeOffset.UtcNow,
                "user",
                "summary",
                "detailed",
                "[]",
                "[]",
                "[]",
                " ",
                "capacity",
                "workload",
                ExplainabilityVersions.CurrentModelVersion,
                ExplainabilityVersions.CurrentPromptVersion));

        Assert.Throws<InvalidOperationException>(() =>
            Explainability.Generate(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ExplainabilityTypes.Recommendation,
                1,
                DateTimeOffset.UtcNow,
                "user",
                "summary",
                "detailed",
                "[]",
                "[]",
                "[]",
                "confidence",
                "capacity",
                "workload",
                ExplainabilityVersions.CurrentModelVersion,
                ExplainabilityVersions.CurrentPromptVersion));
    }

    private static Explainability CreateRecommendationExplanation() =>
        Explainability.Generate(
            Guid.NewGuid(),
            null,
            ExplainabilityTypes.Recommendation,
            1,
            DateTimeOffset.UtcNow,
            "explainer",
            "Executive summary",
            "Detailed explanation",
            "[\"factor\"]",
            "[\"assumption\"]",
            "[\"risk\"]",
            "Confidence explanation",
            "Capacity explanation",
            "Workload explanation",
            ExplainabilityVersions.CurrentModelVersion,
            ExplainabilityVersions.CurrentPromptVersion);

    private static Explainability CreateAIExplanation(Guid aiId) =>
        Explainability.Generate(
            Guid.NewGuid(),
            aiId,
            ExplainabilityTypes.AIRecommendation,
            1,
            DateTimeOffset.UtcNow,
            "explainer",
            "AI executive summary",
            "AI detailed explanation",
            "[\"factor\"]",
            "[\"assumption\"]",
            "[\"risk\"]",
            "AI confidence explanation",
            "AI capacity explanation",
            "AI workload explanation",
            ExplainabilityVersions.CurrentModelVersion,
            ExplainabilityVersions.CurrentPromptVersion);
}
