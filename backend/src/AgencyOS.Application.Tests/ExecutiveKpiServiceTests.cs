using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class ExecutiveKpiServiceTests
{
    private readonly ExecutiveKpiService _service = new();

    private static EnterpriseDashboardSummaryResponse CreateSummary() => new()
    {
        PortfolioCount = 12,
        ActivePortfolioCount = 9,
        RecommendationCount = 34,
        DecisionCount = 20,
        PendingDecisionCount = 5,
        CompletedDecisionCount = 12,
        OverallHealth = new HealthIndicator { Status = "Healthy", Label = "Healthy" }
    };

    [Fact]
    public void BuildKpiSummary_MapsCountsDirectlyFromSummary()
    {
        var kpis = _service.BuildKpiSummary(CreateSummary());

        Assert.Equal(12, kpis.PortfolioCount);
        Assert.Equal(9, kpis.ActivePortfolioCount);
        Assert.Equal(34, kpis.RecommendationCount);
        Assert.Equal(20, kpis.DecisionCount);
        Assert.Equal(5, kpis.PendingDecisionCount);
        Assert.Equal(12, kpis.CompletedDecisionCount);
    }

    [Fact]
    public void BuildKpiSummary_UsesSummaryOverallHealth_WhenNoOverrideSupplied()
    {
        var kpis = _service.BuildKpiSummary(CreateSummary());
        Assert.Equal("Healthy", kpis.OverallHealth.Status);
    }

    [Fact]
    public void BuildKpiSummary_UsesOverrideHealth_WhenSupplied()
    {
        var overrideHealth = new HealthIndicator { Status = "AtRisk", Label = "At Risk" };
        var kpis = _service.BuildKpiSummary(CreateSummary(), overallHealth: overrideHealth);
        Assert.Equal("AtRisk", kpis.OverallHealth.Status);
    }

    [Fact]
    public void BuildKpiSummary_DefaultsCapacityWorkloadAudit_ToZero_WhenNotSupplied()
    {
        var kpis = _service.BuildKpiSummary(CreateSummary());

        Assert.Equal(0m, kpis.CapacityUtilizationPercentage);
        Assert.Equal(0m, kpis.WorkloadPercentage);
        Assert.Equal(0, kpis.AuditEventCount);
    }

    [Fact]
    public void BuildKpiSummary_MapsCapacityWorkloadAudit_WhenSupplied()
    {
        var capacity = new EnterpriseDashboardCapacityResponse { AverageUtilizationPercentage = 72.5m };
        var workload = new EnterpriseDashboardWorkloadResponse { AverageWorkloadPercentage = 61.2m };
        var audit = new EnterpriseDashboardAuditResponse { EventCount = 44 };

        var kpis = _service.BuildKpiSummary(CreateSummary(), capacity, workload, audit);

        Assert.Equal(72.5m, kpis.CapacityUtilizationPercentage);
        Assert.Equal(61.2m, kpis.WorkloadPercentage);
        Assert.Equal(44, kpis.AuditEventCount);
    }
}
