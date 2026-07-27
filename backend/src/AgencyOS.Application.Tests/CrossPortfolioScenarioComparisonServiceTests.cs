using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioScenarioComparisonServiceTests
{
    private readonly CrossPortfolioScenarioComparisonService _service = new();

    [Fact]
    public void Compare_ComputesDeltas_BetweenTwoScenarios()
    {
        var left = CreateRecord(averageUtilization: 60m, averageWorkload: 50m, portfolioConflicts: 0, resourceConflicts: 1, participationCount: 2);
        var right = CreateRecord(averageUtilization: 80m, averageWorkload: 70m, portfolioConflicts: 1, resourceConflicts: 3, participationCount: 3);

        var result = _service.Compare(left, right);

        Assert.Same(left.Scenario, result.Left);
        Assert.Same(right.Scenario, result.Right);

        var utilizationDiff = result.FieldDiffs.Single(diff => diff.Field == "Average Utilization %");
        Assert.Equal(20m, utilizationDiff.Delta);

        var workloadDiff = result.FieldDiffs.Single(diff => diff.Field == "Average Workload %");
        Assert.Equal(20m, workloadDiff.Delta);

        var portfolioConflictDiff = result.FieldDiffs.Single(diff => diff.Field == "Portfolio Conflict Count");
        Assert.Equal(1m, portfolioConflictDiff.Delta);

        var resourceConflictDiff = result.FieldDiffs.Single(diff => diff.Field == "Resource Conflict Count");
        Assert.Equal(2m, resourceConflictDiff.Delta);

        var participationDiff = result.FieldDiffs.Single(diff => diff.Field == "Participation Count");
        Assert.Equal(1m, participationDiff.Delta);

        Assert.True(result.RequiresHumanApproval);
        Assert.False(string.IsNullOrWhiteSpace(result.AdvisoryDisclaimer));
    }

    [Fact]
    public void Compare_HandlesNullPercentages_Gracefully()
    {
        var left = CreateRecord(averageUtilization: null, averageWorkload: null, portfolioConflicts: 0, resourceConflicts: 0, participationCount: 1);
        var right = CreateRecord(averageUtilization: 50m, averageWorkload: 40m, portfolioConflicts: 0, resourceConflicts: 0, participationCount: 1);

        var result = _service.Compare(left, right);

        var utilizationDiff = result.FieldDiffs.Single(diff => diff.Field == "Average Utilization %");
        Assert.Null(utilizationDiff.Delta);
        Assert.Null(utilizationDiff.LeftValue);
        Assert.Equal("50", utilizationDiff.RightValue);
    }

    private static CrossPortfolioScenarioRecord CreateRecord(
        decimal? averageUtilization,
        decimal? averageWorkload,
        int portfolioConflicts,
        int resourceConflicts,
        int participationCount)
    {
        var scenarioId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var scenario = new CrossPortfolioScenarioResponse
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            CreatedAt = DateTimeOffset.UtcNow,
            Capacity = new EnterpriseCapacityResponse { AverageUtilizationPercentage = averageUtilization },
            Workload = new EnterpriseWorkloadResponse { AverageWorkloadPercentage = averageWorkload },
            Conflicts = new ConflictSummaryResponse
            {
                PortfolioConflictCount = portfolioConflicts,
                ResourceConflictCount = resourceConflicts
            },
            Portfolios = Enumerable.Range(0, participationCount)
                .Select(_ => new PortfolioParticipationResponse { PortfolioId = Guid.NewGuid() })
                .ToList()
        };

        return new CrossPortfolioScenarioRecord
        {
            ScenarioId = scenarioId,
            CompanyId = companyId,
            CreatedAt = scenario.CreatedAt,
            Scenario = scenario
        };
    }
}
