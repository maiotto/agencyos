using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationOverviewServiceTests
{
    private readonly Mock<IRecommendationService> _recommendationService = new();
    private readonly Mock<IRecommendationWorkflowService> _recommendationWorkflowService = new();
    private readonly Mock<IRecommendationHistoryService> _recommendationHistoryService = new();
    private readonly Mock<IAIRecommendationService> _aiRecommendationService = new();
    private readonly Mock<IExplainabilityService> _explainabilityService = new();
    private readonly Mock<IExecutiveRecommendationSummaryService> _executiveRecommendationSummaryService = new();

    private RecommendationOverviewService CreateService() =>
        new(
            _recommendationService.Object,
            _recommendationWorkflowService.Object,
            _recommendationHistoryService.Object,
            _aiRecommendationService.Object,
            _explainabilityService.Object,
            _executiveRecommendationSummaryService.Object);

    private void SetupDefaults(
        IReadOnlyList<RecommendationResponse>? recommendations = null,
        IReadOnlyList<RecommendationWorkflowResponse>? workflows = null)
    {
        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendations ?? new List<RecommendationResponse>());

        _recommendationWorkflowService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflows ?? new List<RecommendationWorkflowResponse>());

        _recommendationHistoryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationHistoryResponse>());

        _aiRecommendationService
            .Setup(service => service.GetAllAsync(It.IsAny<AIRecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AIRecommendationResponse>());

        _explainabilityService
            .Setup(service => service.GetAllAsync(It.IsAny<ExplainabilityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExplainabilityResponse>());

        _executiveRecommendationSummaryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutiveRecommendationSummaryResponse>());
    }

    [Fact]
    public async Task GetOverviewAsync_ComputesActiveAndArchivedCounts()
    {
        var companyId = Guid.NewGuid();
        SetupDefaults(recommendations:
        [
            new RecommendationResponse { Id = Guid.NewGuid(), CompanyId = companyId, Archived = false },
            new RecommendationResponse { Id = Guid.NewGuid(), CompanyId = companyId, Archived = false },
            new RecommendationResponse { Id = Guid.NewGuid(), CompanyId = companyId, Archived = true }
        ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new RecommendationWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(2, overview.Kpis.ActiveRecommendationCount);
        Assert.Equal(1, overview.Kpis.ArchivedRecommendationCount);
        Assert.True(overview.RequiresHumanApproval);
        Assert.False(string.IsNullOrWhiteSpace(overview.HumanApprovalDisclaimer));
    }

    [Fact]
    public async Task GetOverviewAsync_CountsPendingApproval_WhenWorkflowCompanyIdMatches()
    {
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        SetupDefaults(workflows:
        [
            new RecommendationWorkflowResponse { Id = Guid.NewGuid(), CompanyId = companyId, RecommendationId = Guid.NewGuid() },
            new RecommendationWorkflowResponse { Id = Guid.NewGuid(), CompanyId = otherCompanyId, RecommendationId = Guid.NewGuid() }
        ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new RecommendationWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(1, overview.Kpis.PendingApprovalCount);
    }

    [Fact]
    public async Task GetOverviewAsync_CountsPendingApproval_ByRecommendationIdIntersection_WhenWorkflowCompanyIdMissing()
    {
        var companyId = Guid.NewGuid();
        var companyRecommendationId = Guid.NewGuid();
        var otherRecommendationId = Guid.NewGuid();

        SetupDefaults(
            recommendations: [new RecommendationResponse { Id = companyRecommendationId, CompanyId = companyId }],
            workflows:
            [
                new RecommendationWorkflowResponse { Id = Guid.NewGuid(), CompanyId = null, RecommendationId = companyRecommendationId },
                new RecommendationWorkflowResponse { Id = Guid.NewGuid(), CompanyId = null, RecommendationId = otherRecommendationId }
            ]);

        var overview = await CreateService().GetOverviewAsync(
            companyId,
            new RecommendationWorkspaceQueryParameters(),
            CancellationToken.None);

        Assert.Equal(1, overview.Kpis.PendingApprovalCount);
    }

    [Fact]
    public async Task GetOverviewAsync_QueriesPendingApprovalWorkflowsOnly()
    {
        var companyId = Guid.NewGuid();
        RecommendationWorkflowQueryParameters? captured = null;
        SetupDefaults();
        _recommendationWorkflowService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<RecommendationWorkflowQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(new List<RecommendationWorkflowResponse>());

        await CreateService().GetOverviewAsync(companyId, new RecommendationWorkspaceQueryParameters(), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(RecommendationWorkflowStatus.PendingApproval, captured!.Status);
    }

    [Fact]
    public async Task GetOverviewAsync_NeverCallsRecommendationWriteMethods()
    {
        var companyId = Guid.NewGuid();
        SetupDefaults();

        await CreateService().GetOverviewAsync(companyId, new RecommendationWorkspaceQueryParameters(), CancellationToken.None);

        _recommendationService.Verify(
            service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _recommendationService.VerifyNoOtherCalls();
    }
}
