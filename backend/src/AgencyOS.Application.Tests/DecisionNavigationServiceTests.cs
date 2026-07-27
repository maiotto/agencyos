using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class DecisionNavigationServiceTests
{
    private readonly DecisionNavigationService _service = new();

    [Fact]
    public void GetNavigation_ReturnsOrderedActions_WithExpectedKeys()
    {
        var companyId = Guid.NewGuid();

        var navigation = _service.GetNavigation(companyId);

        Assert.Equal(companyId, navigation.CompanyId);
        Assert.Equal(10, navigation.Actions.Count);
        Assert.Contains(navigation.Actions, action => action.Key == "list-decisions");
        Assert.Contains(navigation.Actions, action => action.Key == "create-decision");
        Assert.Contains(navigation.Actions, action => action.Key == "decision-detail");
        Assert.Contains(navigation.Actions, action => action.Key == "start-implementation");
        Assert.Contains(navigation.Actions, action => action.Key == "complete");
        Assert.Contains(navigation.Actions, action => action.Key == "cancel");
        Assert.Contains(navigation.Actions, action => action.Key == "record-outcome");
        Assert.Contains(navigation.Actions, action => action.Key == "recommendation-workspace");
        Assert.Contains(navigation.Actions, action => action.Key == "enterprise-dashboard");
        Assert.Contains(navigation.Actions, action => action.Key == "audit");
        Assert.All(navigation.Actions, action => Assert.False(string.IsNullOrWhiteSpace(action.DrillDownPath)));
    }

    [Fact]
    public void GetNavigation_MarksLifecycleAndOutcomeActions_AsRequiringHumanApproval()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "create-decision" && action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "start-implementation" && action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "complete" && action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "cancel" && action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "record-outcome" && action.RequiresHumanApproval);
    }

    [Fact]
    public void GetNavigation_DoesNotRequireHumanApproval_ForPureNavigationActions()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "list-decisions" && !action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "decision-detail" && !action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "recommendation-workspace" && !action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "enterprise-dashboard" && !action.RequiresHumanApproval);
        Assert.Contains(navigation.Actions, action => action.Key == "audit" && !action.RequiresHumanApproval);
    }

    [Fact]
    public void GetNavigation_IncludesCompanyId_InScopedPaths()
    {
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        var navigation = _service.GetNavigation(companyId);

        Assert.Contains(navigation.Actions, action =>
            action.Key == "list-decisions"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(navigation.Actions, action =>
            action.Key == "audit"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetNavigation_PointsCreateAction_ToDecisionsNewRoute()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.Contains(navigation.Actions, action => action.Key == "create-decision" && action.DrillDownPath == "/decisions/new");
    }
}
