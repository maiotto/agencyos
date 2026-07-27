using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationSummaryServiceTests
{
    private readonly Mock<IRecommendationService> _recommendationService = new();
    private readonly Mock<IRecommendationWorkflowService> _recommendationWorkflowService = new();
    private readonly Mock<IRecommendationHistoryService> _recommendationHistoryService = new();
    private readonly Mock<IAIRecommendationService> _aiRecommendationService = new();
    private readonly Mock<IExplainabilityService> _explainabilityService = new();
    private readonly Mock<IExecutiveRecommendationSummaryService> _executiveRecommendationSummaryService = new();
    private readonly Mock<IRecommendationComparisonService> _recommendationComparisonService = new();

    private RecommendationSummaryService CreateService() =>
        new(
            _recommendationService.Object,
            _recommendationWorkflowService.Object,
            _recommendationHistoryService.Object,
            _aiRecommendationService.Object,
            _explainabilityService.Object,
            _executiveRecommendationSummaryService.Object,
            _recommendationComparisonService.Object);

    [Fact]
    public async Task GetRecommendationsSectionAsync_ReturnsCards_OrderedByGeneratedAtDescending()
    {
        var companyId = Guid.NewGuid();
        var older = new RecommendationResponse
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Title = "Older",
            GeneratedAt = DateTimeOffset.UtcNow.AddDays(-2)
        };
        var newer = new RecommendationResponse
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Title = "Newer",
            GeneratedAt = DateTimeOffset.UtcNow
        };

        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([older, newer]);

        var section = await CreateService().GetRecommendationsSectionAsync(companyId, CancellationToken.None);

        Assert.Equal(2, section.Recommendations.Count);
        Assert.Equal(newer.Id, section.Recommendations[0].Id);
        Assert.Equal($"/recommendations/{newer.Id}", section.Recommendations[0].DrillDownPath);
        Assert.Equal("list-recommendations", section.ListAction.Key);
    }

    [Fact]
    public async Task GetRecommendationsSectionAsync_QueriesActiveRecommendationsOnly()
    {
        var companyId = Guid.NewGuid();
        RecommendationQueryParameters? captured = null;
        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, RecommendationQueryParameters?, CancellationToken>((_, parameters, _) => captured = parameters)
            .ReturnsAsync(new List<RecommendationResponse>());

        await CreateService().GetRecommendationsSectionAsync(companyId, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(false, captured!.Archived);
    }

    [Fact]
    public async Task GetRecommendationsSectionAsync_NeverCallsRecommendationWriteMethods()
    {
        var companyId = Guid.NewGuid();
        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationResponse>());

        await CreateService().GetRecommendationsSectionAsync(companyId, CancellationToken.None);

        _recommendationService.Verify(
            service => service.GetByCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _recommendationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetApprovalSectionAsync_ReturnsCards_WithApproveAndRejectPaths_FilteredByCompany()
    {
        var companyId = Guid.NewGuid();
        var matchingWorkflowId = Guid.NewGuid();

        _recommendationService
            .Setup(service => service.GetByCompanyIdAsync(
                companyId,
                It.IsAny<RecommendationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecommendationResponse>());

        _recommendationWorkflowService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new RecommendationWorkflowResponse
                {
                    Id = matchingWorkflowId,
                    CompanyId = companyId,
                    RecommendationId = Guid.NewGuid(),
                    Title = "Needs approval",
                    Status = RecommendationWorkflowStatus.PendingApproval,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new RecommendationWorkflowResponse
                {
                    Id = Guid.NewGuid(),
                    CompanyId = Guid.NewGuid(),
                    RecommendationId = Guid.NewGuid(),
                    Title = "Other company",
                    Status = RecommendationWorkflowStatus.PendingApproval,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]);

        var section = await CreateService().GetApprovalSectionAsync(companyId, CancellationToken.None);

        Assert.Single(section.PendingApprovals);
        var card = section.PendingApprovals[0];
        Assert.Equal(matchingWorkflowId, card.WorkflowId);
        Assert.Equal($"/recommendations/workflow/{matchingWorkflowId}", card.DrillDownPath);
        Assert.Equal($"/recommendations/workflow/{matchingWorkflowId}/approve", card.ApprovePath);
        Assert.Equal($"/recommendations/workflow/{matchingWorkflowId}/reject", card.RejectPath);
        Assert.True(card.RequiresHumanApproval);
        Assert.True(section.RequiresHumanApproval);
    }

    [Fact]
    public async Task GetHistorySectionAsync_ReturnsCards_WithDrillDownPaths_AndAppliesFromTo()
    {
        var companyId = Guid.NewGuid();
        var from = DateTimeOffset.UtcNow.AddDays(-10);
        var to = DateTimeOffset.UtcNow;
        var historyId = Guid.NewGuid();
        RecommendationHistoryQueryParameters? captured = null;

        _recommendationHistoryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<RecommendationHistoryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .Callback<RecommendationHistoryQueryParameters, CancellationToken>((parameters, _) => captured = parameters)
            .ReturnsAsync(
            [
                new RecommendationHistoryResponse
                {
                    Id = historyId,
                    CompanyId = companyId,
                    RecommendationId = Guid.NewGuid(),
                    RecommendationNumber = "REC-0001",
                    EventType = RecommendationHistoryEventType.VersionCreated,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]);

        var section = await CreateService().GetHistorySectionAsync(companyId, from, to, CancellationToken.None);

        Assert.Single(section.Items);
        Assert.Equal($"/recommendations/history/{historyId}", section.Items[0].DrillDownPath);
        Assert.NotNull(captured);
        Assert.Equal(companyId, captured!.CompanyId);
        Assert.Equal(from, captured.CreatedFrom);
        Assert.Equal(to, captured.CreatedTo);
    }

    [Fact]
    public async Task GetCompareSectionAsync_ReturnsEmptyComparison_WhenIdsMissing()
    {
        var section = await CreateService().GetCompareSectionAsync(Guid.NewGuid(), null, null, CancellationToken.None);

        Assert.False(section.HasComparison);
        Assert.Null(section.Comparison);
        Assert.Equal("compare", section.Action.Key);
        _recommendationComparisonService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCompareSectionAsync_ReturnsEmptyComparison_WhenIdsEqual()
    {
        var id = Guid.NewGuid();

        var section = await CreateService().GetCompareSectionAsync(Guid.NewGuid(), id, id, CancellationToken.None);

        Assert.False(section.HasComparison);
        Assert.Null(section.Comparison);
        _recommendationComparisonService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCompareSectionAsync_CallsComparisonService_WhenBothIdsSetAndDistinct()
    {
        var leftId = Guid.NewGuid();
        var rightId = Guid.NewGuid();
        var comparison = new RecommendationComparisonResponse
        {
            Left = new RecommendationHistoryResponse(),
            Right = new RecommendationHistoryResponse()
        };

        _recommendationComparisonService
            .Setup(service => service.CompareByIdsAsync(leftId, rightId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comparison);

        var section = await CreateService().GetCompareSectionAsync(Guid.NewGuid(), leftId, rightId, CancellationToken.None);

        Assert.True(section.HasComparison);
        Assert.Same(comparison, section.Comparison);
        _recommendationComparisonService.Verify(
            service => service.CompareByIdsAsync(leftId, rightId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAiSectionAsync_ReturnsCards_MarkedAdvisoryAndInformational()
    {
        var companyId = Guid.NewGuid();
        var aiId = Guid.NewGuid();
        var explainabilityId = Guid.NewGuid();

        _aiRecommendationService
            .Setup(service => service.GetAllAsync(It.IsAny<AIRecommendationQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new AIRecommendationResponse { Id = aiId, RecommendationId = Guid.NewGuid() }]);

        _explainabilityService
            .Setup(service => service.GetAllAsync(It.IsAny<ExplainabilityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ExplainabilityResponse { Id = explainabilityId, RecommendationId = Guid.NewGuid() }]);

        var section = await CreateService().GetAiSectionAsync(companyId, null, null, CancellationToken.None);

        Assert.Single(section.AiRecommendations);
        Assert.Single(section.Explainability);
        Assert.True(section.AiRecommendations[0].IsAdvisory);
        Assert.True(section.Explainability[0].IsInformational);
        Assert.Equal($"/ai-recommendations/{aiId}", section.AiRecommendations[0].DrillDownPath);
        Assert.Equal($"/explainability/{explainabilityId}", section.Explainability[0].DrillDownPath);
        Assert.True(section.IsAdvisory);
        Assert.True(section.IsInformational);
    }

    [Fact]
    public async Task GetExecutiveSummarySectionAsync_ReturnsCards_WithDrillDownPaths()
    {
        var companyId = Guid.NewGuid();
        var summaryId = Guid.NewGuid();

        _executiveRecommendationSummaryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ExecutiveRecommendationSummaryQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ExecutiveRecommendationSummaryResponse { Id = summaryId, RecommendationId = Guid.NewGuid() }]);

        var section = await CreateService().GetExecutiveSummarySectionAsync(companyId, null, null, CancellationToken.None);

        Assert.Single(section.Summaries);
        Assert.Equal($"/executive-summaries/{summaryId}", section.Summaries[0].DrillDownPath);
        Assert.Equal("generate-executive-summary", section.GenerateAction.Key);
    }
}
