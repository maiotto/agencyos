using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class RecommendationNavigationServiceTests
{
    private readonly RecommendationNavigationService _service = new();

    [Fact]
    public void GetNavigation_ReturnsOrderedActions_WithExpectedKeys()
    {
        var companyId = Guid.NewGuid();

        var navigation = _service.GetNavigation(companyId);

        Assert.Equal(companyId, navigation.CompanyId);
        Assert.Equal(17, navigation.Actions.Count);
        Assert.Contains(navigation.Actions, action => action.Key == "list-recommendations");
        Assert.Contains(navigation.Actions, action => action.Key == "generate-recommendations");
        Assert.Contains(navigation.Actions, action => action.Key == "approval-queue");
        Assert.Contains(navigation.Actions, action => action.Key == "start-workflow");
        Assert.Contains(navigation.Actions, action => action.Key == "history");
        Assert.Contains(navigation.Actions, action => action.Key == "compare");
        Assert.Contains(navigation.Actions, action => action.Key == "archive-restore");
        Assert.Contains(navigation.Actions, action => action.Key == "ai-recommendations");
        Assert.Contains(navigation.Actions, action => action.Key == "explainability");
        Assert.Contains(navigation.Actions, action => action.Key == "executive-summaries");
        Assert.Contains(navigation.Actions, action => action.Key == "decisions");
        Assert.Contains(navigation.Actions, action => action.Key == "audit");
        Assert.All(navigation.Actions, action => Assert.False(string.IsNullOrWhiteSpace(action.DrillDownPath)));
    }

    [Fact]
    public void GetNavigation_MarksApproveAndReject_AsRequiringHumanApproval()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "approve" && action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "reject" && action.RequiresHumanApproval);
    }

    [Fact]
    public void GetNavigation_MarksAiActions_AsAdvisory()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "ai-recommendations" && action.IsAdvisory);
        Assert.Contains(navigation.Actions, action => action.Key == "generate-ai" && action.IsAdvisory);
    }

    [Fact]
    public void GetNavigation_MarksExplainabilityActions_AsInformational()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "explainability" && action.IsInformational);
        Assert.Contains(
            navigation.Actions,
            action => action.Key == "generate-explainability" && action.IsInformational);
    }

    [Fact]
    public void GetNavigation_IncludesCompanyId_InScopedPaths()
    {
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        var navigation = _service.GetNavigation(companyId);

        Assert.Contains(navigation.Actions, action =>
            action.Key == "list-recommendations"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(navigation.Actions, action =>
            action.Key == "ai-recommendations"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
    }
}
