using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class PlanningNavigationServiceTests
{
    private readonly PlanningNavigationService _service = new();

    [Fact]
    public void GetNavigation_ReturnsOrderedActions_WithExpectedKeys()
    {
        var companyId = Guid.NewGuid();

        var navigation = _service.GetNavigation(companyId);

        Assert.Equal(companyId, navigation.CompanyId);
        Assert.Equal(10, navigation.Actions.Count);
        Assert.Contains(navigation.Actions, action => action.Key == "manage-templates");
        Assert.Contains(navigation.Actions, action => action.Key == "capacity-planning");
        Assert.Contains(navigation.Actions, action => action.Key == "cross-portfolio-planning" && action.IsAdvisory);
        Assert.All(navigation.Actions, action => Assert.False(string.IsNullOrWhiteSpace(action.DrillDownPath)));
    }

    [Fact]
    public void GetNavigation_IncludesCompanyId_InTemplateAndPortfolioPaths()
    {
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        var navigation = _service.GetNavigation(companyId);

        Assert.Contains(navigation.Actions, action =>
            action.Key == "manage-templates"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(navigation.Actions, action =>
            action.Key == "portfolios"
            && action.DrillDownPath.Contains(companyId.ToString(), StringComparison.OrdinalIgnoreCase));
    }
}

public class PlanningHistoryServiceTests
{
    private readonly Mock<IAuditQueryService> _auditQueryService = new();
    private readonly PlanningHistoryService _service;

    public PlanningHistoryServiceTests()
    {
        _service = new PlanningHistoryService(_auditQueryService.Object);
    }

    [Fact]
    public async Task GetHistoryAsync_FiltersToPlanningEntityTypes_Only()
    {
        var companyId = Guid.NewGuid();
        var keptId = Guid.NewGuid();
        _auditQueryService
            .Setup(service => service.GetAllAsync(It.IsAny<AuditEventQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new AuditEventResponse
                {
                    Id = keptId,
                    EntityType = AuditEntityTypes.PlanningTemplate,
                    EntityId = Guid.NewGuid(),
                    EventType = AuditEventTypes.Created,
                    Action = "PlanningTemplate.Create",
                    OccurredAt = DateTimeOffset.UtcNow,
                    UserId = "planner",
                    CompanyId = companyId
                },
                new AuditEventResponse
                {
                    Id = Guid.NewGuid(),
                    EntityType = AuditEntityTypes.Decision,
                    EntityId = Guid.NewGuid(),
                    EventType = AuditEventTypes.Created,
                    Action = "Decision.Create",
                    OccurredAt = DateTimeOffset.UtcNow,
                    UserId = "planner",
                    CompanyId = companyId
                }
            ]);

        var history = await _service.GetHistoryAsync(companyId, null, null);

        Assert.Single(history.Items);
        Assert.Equal(keptId, history.Items[0].Id);
        Assert.Equal($"/audit/{keptId}", history.Items[0].DrillDownPath);
        Assert.Equal("audit-trail", history.Action.Key);
    }
}
