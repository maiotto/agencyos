using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class ExecutiveNavigationServiceTests
{
    private readonly ExecutiveNavigationService _service = new();

    [Fact]
    public void GetNavigation_ReturnsCompanyId()
    {
        var companyId = Guid.NewGuid();
        var navigation = _service.GetNavigation(companyId);
        Assert.Equal(companyId, navigation.CompanyId);
    }

    [Fact]
    public void GetNavigation_IncludesEveryRequiredDrillDownTarget()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());
        var paths = navigation.Actions.Select(action => action.DrillDownPath.Split('?')[0]).ToList();

        Assert.Contains("/enterprise-dashboard", paths);
        Assert.Contains("/my-work", paths);
        Assert.Contains("/planning-workspace", paths);
        Assert.Contains("/recommendation-workspace", paths);
        Assert.Contains("/decision-workspace", paths);
        Assert.Contains("/portfolio-analytics", paths);
        Assert.Contains("/cross-portfolio-planning", paths);
        Assert.Contains("/capacity/history", paths);
        Assert.Contains("/workload/history", paths);
        Assert.Contains("/ai-recommendations", paths);
        Assert.Contains("/explainability", paths);
        Assert.Contains("/executive-summaries", paths);
        Assert.Contains("/audit", paths);
    }

    [Fact]
    public void GetNavigation_UsesExpectedCategories()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());
        var categories = navigation.Actions.Select(action => action.Category).Distinct().ToList();

        var allowedCategories = new[]
        {
            "Overview", "Planning", "Recommendations", "Decisions", "Capacity", "Workload", "AI", "Audit", "Portfolios"
        };

        Assert.All(categories, category => Assert.Contains(category, allowedCategories));
    }

    [Fact]
    public void GetNavigation_MarksCrossPortfolioPlanning_AsAdvisory()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());
        var crossPortfolio = navigation.Actions.Single(action => action.Key == "cross-portfolio-planning");

        Assert.True(crossPortfolio.IsAdvisory);
        Assert.Equal("Portfolios", crossPortfolio.Category);
    }

    [Fact]
    public void GetNavigation_AuditAction_IncludesCompanyIdInDrillDownPath()
    {
        var companyId = Guid.NewGuid();
        var navigation = _service.GetNavigation(companyId);
        var auditAction = navigation.Actions.Single(action => action.Key == "audit");

        Assert.Contains(companyId.ToString(), auditAction.DrillDownPath);
    }

    [Fact]
    public void GetNavigation_EveryAction_HasNonEmptyKeyLabelDescription()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());

        Assert.All(navigation.Actions, action =>
        {
            Assert.False(string.IsNullOrWhiteSpace(action.Key));
            Assert.False(string.IsNullOrWhiteSpace(action.Label));
            Assert.False(string.IsNullOrWhiteSpace(action.Description));
            Assert.False(string.IsNullOrWhiteSpace(action.DrillDownPath));
        });
    }

    [Fact]
    public void GetNavigation_ReturnsUniqueKeys()
    {
        var navigation = _service.GetNavigation(Guid.NewGuid());
        var keys = navigation.Actions.Select(action => action.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }
}
