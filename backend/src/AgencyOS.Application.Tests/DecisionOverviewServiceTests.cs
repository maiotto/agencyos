using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class DecisionOverviewServiceTests
{
    private readonly Mock<IDecisionService> _decisionService = new();

    private DecisionOverviewService CreateService() => new(_decisionService.Object);

    private void SetupDecisions(IReadOnlyList<DecisionResponse> decisions)
    {
        _decisionService
            .Setup(service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(decisions);
    }

    private static DecisionResponse CreateDecision(
        Guid companyId,
        string decisionStatus,
        string implementationStatus = AgencyOS.Domain.Entities.DecisionImplementationStatus.NotStarted,
        string? outcome = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            RecommendationId = Guid.NewGuid(),
            CompanyId = companyId,
            MissionId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            DecisionStatus = decisionStatus,
            ImplementationStatus = implementationStatus,
            DecisionDate = DateTimeOffset.UtcNow,
            Outcome = outcome,
            CreatedBy = "tester"
        };

    [Fact]
    public async Task GetOverviewAsync_ComputesStatusCounts()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(
        [
            CreateDecision(companyId, DecisionStatus.Created),
            CreateDecision(companyId, DecisionStatus.Created),
            CreateDecision(companyId, DecisionStatus.InProgress),
            CreateDecision(companyId, DecisionStatus.Completed),
            CreateDecision(companyId, DecisionStatus.Cancelled)
        ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new DecisionWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(5, overview.Kpis.TotalCount);
        Assert.Equal(2, overview.Kpis.PendingCount);
        Assert.Equal(1, overview.Kpis.InProgressCount);
        Assert.Equal(1, overview.Kpis.CompletedCount);
        Assert.Equal(1, overview.Kpis.CancelledCount);
        Assert.True(overview.RequiresHumanApproval);
        Assert.False(string.IsNullOrWhiteSpace(overview.HumanApprovalDisclaimer));
    }

    [Fact]
    public async Task GetOverviewAsync_ComputesWithOutcomeCount()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(
        [
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, "Positive impact"),
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, null),
            CreateDecision(companyId, DecisionStatus.Completed, DecisionImplementationStatus.Completed, "   ")
        ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new DecisionWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(1, overview.Kpis.WithOutcomeCount);
    }

    [Fact]
    public async Task GetOverviewAsync_ComputesImplementationNotStartedCount()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(
        [
            CreateDecision(companyId, DecisionStatus.Created, DecisionImplementationStatus.NotStarted),
            CreateDecision(companyId, DecisionStatus.InProgress, DecisionImplementationStatus.InProgress),
            CreateDecision(companyId, DecisionStatus.Created, DecisionImplementationStatus.NotStarted)
        ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new DecisionWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(2, overview.Kpis.ImplementationNotStartedCount);
    }

    [Fact]
    public async Task GetOverviewAsync_PassesCompanyIdAndWindow_ToDecisionQuery()
    {
        var companyId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow;
        DecisionQueryParameters? captured = null;
        _decisionService
            .Setup(service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()))
            .Callback<DecisionQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new List<DecisionResponse>());

        await CreateService().GetOverviewAsync(
            companyId,
            new DecisionWorkspaceQueryParameters { From = from, To = to },
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(from, captured.DecisionFrom);
        Assert.Equal(to, captured.DecisionTo);
    }

    [Fact]
    public async Task GetOverviewAsync_NeverCallsDecisionWriteMethods()
    {
        var companyId = Guid.NewGuid();
        SetupDecisions(new List<DecisionResponse>());

        await CreateService().GetOverviewAsync(companyId, new DecisionWorkspaceQueryParameters(), CancellationToken.None);

        _decisionService.Verify(
            service => service.GetAllAsync(It.IsAny<DecisionQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _decisionService.VerifyNoOtherCalls();
    }
}
