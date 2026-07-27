using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationWorkspaceServiceTests
{
    private readonly Mock<IRecommendationOverviewService> _overviewService = new();
    private readonly Mock<IRecommendationNavigationService> _navigationService = new();
    private readonly Mock<IRecommendationSummaryService> _summaryService = new();
    private readonly Mock<ICompanyRepository> _companyRepository = new();
    private readonly Mock<ICompanyContext> _companyContext = new();

    public RecommendationWorkspaceServiceTests()
    {
        _companyContext.SetupProperty(context => context.CompanyId);
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => CreateCompany(id));

        _overviewService
            .Setup(service => service.GetOverviewAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationWorkspaceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, RecommendationWorkspaceQueryParameters _, CancellationToken __) =>
                new RecommendationOverviewResponse { CompanyId = companyId, Kpis = new RecommendationKpiSummaryResponse() });

        _navigationService
            .Setup(service => service.GetNavigation(It.IsAny<Guid>()))
            .Returns((Guid companyId) => new RecommendationNavigationResponse { CompanyId = companyId, Actions = [] });

        _summaryService
            .Setup(service => service.GetRecommendationsSectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, CancellationToken _) =>
                new RecommendationsSectionResponse { CompanyId = companyId });

        _summaryService
            .Setup(service => service.GetApprovalSectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, CancellationToken _) =>
                new ApprovalSectionResponse { CompanyId = companyId });

        _summaryService
            .Setup(service => service.GetHistorySectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken _) =>
                new HistorySectionResponse { CompanyId = companyId, From = from, To = to });

        _summaryService
            .Setup(service => service.GetCompareSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, Guid? left, Guid? right, CancellationToken _) =>
                new CompareSectionResponse { CompanyId = companyId, LeftRecommendationId = left, RightRecommendationId = right });

        _summaryService
            .Setup(service => service.GetAiSectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? _, DateTimeOffset? __, CancellationToken ___) =>
                new AiSectionResponse { CompanyId = companyId });

        _summaryService
            .Setup(service => service.GetExecutiveSummarySectionAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid companyId, DateTimeOffset? _, DateTimeOffset? __, CancellationToken ___) =>
                new ExecutiveSummarySectionResponse { CompanyId = companyId });
    }

    private RecommendationWorkspaceService CreateService() =>
        new(
            _overviewService.Object,
            _navigationService.Object,
            _summaryService.Object,
            _companyRepository.Object,
            _companyContext.Object);

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromParametersFirst()
    {
        var explicitCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new RecommendationWorkspaceQueryParameters { CompanyId = explicitCompanyId });

        Assert.Equal(explicitCompanyId, workspace.CompanyId);
        Assert.True(workspace.Overview.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ResolvesCompanyId_FromCompanyContext_WhenParametersUnset()
    {
        var contextCompanyId = Guid.NewGuid();
        _companyContext.Object.CompanyId = contextCompanyId;

        var workspace = await CreateService().GetWorkspaceAsync(new RecommendationWorkspaceQueryParameters());

        Assert.Equal(contextCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_FallsBackToDefaultCompany_WhenUnset()
    {
        _companyContext.Object.CompanyId = null;

        var workspace = await CreateService().GetWorkspaceAsync(new RecommendationWorkspaceQueryParameters());

        Assert.Equal(AgencyOSCompanies.DefaultCompanyId, workspace.CompanyId);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ThrowsNotFound_WhenCompanyMissing()
    {
        _companyRepository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetWorkspaceAsync(new RecommendationWorkspaceQueryParameters { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task GetWorkspaceAsync_DefaultsToTrailingThirtyDayWindow()
    {
        var workspace = await CreateService().GetWorkspaceAsync(new RecommendationWorkspaceQueryParameters());

        Assert.NotNull(workspace.From);
        Assert.NotNull(workspace.To);
        Assert.True((workspace.To!.Value - workspace.From!.Value).TotalDays is > 29 and < 31);
    }

    [Fact]
    public async Task GetWorkspaceAsync_PassesThroughCompareIds_Unmodified()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();

        var workspace = await CreateService().GetWorkspaceAsync(
            new RecommendationWorkspaceQueryParameters
            {
                LeftRecommendationId = leftId,
                RightRecommendationId = rightId
            });

        Assert.Equal(leftId, workspace.Compare.LeftRecommendationId);
        Assert.Equal(rightId, workspace.Compare.RightRecommendationId);
    }

    [Fact]
    public async Task GetRecommendationsSectionAsync_NeverCallsSummaryWriteMethods()
    {
        await CreateService().GetRecommendationsSectionAsync(new RecommendationWorkspaceQueryParameters());

        _summaryService.Verify(
            service => service.GetRecommendationsSectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _summaryService.VerifyNoOtherCalls();
    }

    private static Company CreateCompany(Guid? id = null) =>
        Company.Create(
            id ?? AgencyOSCompanies.DefaultCompanyId,
            "AOS",
            "AgencyOS Default",
            null,
            "UTC",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
}

/// <summary>
/// End-to-end verification that the Recommendation Workspace never calls a write method on
/// <see cref="IRecommendationService"/> — wires the real <see cref="RecommendationOverviewService"/>
/// and <see cref="RecommendationSummaryService"/> against a mocked <see cref="IRecommendationService"/>
/// (DEC-503-001).
/// </summary>
public class RecommendationWorkspaceServiceReadOnlyTests
{
    [Fact]
    public async Task GetWorkspaceAsync_NeverCallsRecommendationWriteMethods_EndToEnd()
    {
        var companyId = Guid.NewGuid();
        var recommendationService = new Mock<IRecommendationService>();
        recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationResponse>());

        var workflowService = new Mock<IRecommendationWorkflowService>();
        workflowService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationWorkflowResponse>());

        var historyService = new Mock<IRecommendationHistoryService>();
        historyService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationHistoryResponse>());

        var aiService = new Mock<IAIRecommendationService>();
        aiService
            .Setup(service => service.GetAllAsync(It.IsAny<AIRecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AIRecommendationResponse>());

        var explainabilityService = new Mock<IExplainabilityService>();
        explainabilityService
            .Setup(service => service.GetAllAsync(It.IsAny<ExplainabilityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExplainabilityResponse>());

        var executiveSummaryService = new Mock<IExecutiveRecommendationSummaryService>();
        executiveSummaryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutiveRecommendationSummaryResponse>());

        var comparisonService = new Mock<IRecommendationComparisonService>();

        var overviewService = new RecommendationOverviewService(
            recommendationService.Object,
            workflowService.Object,
            historyService.Object,
            aiService.Object,
            explainabilityService.Object,
            executiveSummaryService.Object);

        var summaryService = new RecommendationSummaryService(
            recommendationService.Object,
            workflowService.Object,
            historyService.Object,
            aiService.Object,
            explainabilityService.Object,
            executiveSummaryService.Object,
            comparisonService.Object);

        var navigationService = new RecommendationNavigationService();

        var companyRepository = new Mock<ICompanyRepository>();
        companyRepository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Company.Create(
                companyId,
                "AOS",
                "AgencyOS Default",
                null,
                "UTC",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow));

        var companyContext = new Mock<ICompanyContext>();

        var workspaceService = new RecommendationWorkspaceService(
            overviewService,
            navigationService,
            summaryService,
            companyRepository.Object,
            companyContext.Object);

        await workspaceService.GetWorkspaceAsync(new RecommendationWorkspaceQueryParameters { CompanyId = companyId });

        recommendationService.Verify(
            service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        recommendationService.VerifyNoOtherCalls();
        workflowService.Verify(
            service => service.GetAllAsync(It.IsAny<RecommendationWorkflowQueryParameters>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        workflowService.VerifyNoOtherCalls();
    }
}
