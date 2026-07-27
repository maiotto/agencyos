using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class RecommendationWorkflowServiceTests
{
    private readonly Mock<IRecommendationWorkflowRepository> _repository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IRecommendationHistoryService> _historyService = new();
    private readonly Mock<ILogger<RecommendationWorkflowService>> _logger = new();

    private RecommendationWorkflowService CreateService() =>
        new(_repository.Object, _recommendationRepository.Object, _historyService.Object, new NoOpAuditService(), _logger.Object);

    [Fact]
    public async Task CreateAsync_PersistsDraftWorkflow()
    {
        var recommendation = CreateRecommendation();
        RecommendationWorkflow? persisted = null;

        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(recommendation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<RecommendationWorkflow>(), It.IsAny<CancellationToken>()))
            .Callback<RecommendationWorkflow, CancellationToken>((workflow, _) => persisted = workflow)
            .ReturnsAsync((RecommendationWorkflow workflow, CancellationToken _) => workflow);
        _historyService
            .Setup(service => service.PersistWorkflowTransitionAsync(
                It.IsAny<Recommendation>(),
                It.IsAny<RecommendationWorkflow>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateService().CreateAsync(new CreateRecommendationWorkflowRequest
        {
            RecommendationId = recommendation.Id,
            CreatedBy = "planner@agencyos.local"
        });

        Assert.NotNull(persisted);
        Assert.Equal(RecommendationWorkflowStatus.Draft, persisted!.Status);
        Assert.Equal(recommendation.Id, persisted.RecommendationId);
        Assert.Equal(result.Id, persisted.Id);
        Assert.Single(result.Transitions);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenRecommendationMissing()
    {
        var recommendationId = Guid.NewGuid();
        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(recommendationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recommendation?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreateAsync(new CreateRecommendationWorkflowRequest
            {
                RecommendationId = recommendationId,
                CreatedBy = "planner@agencyos.local"
            }));
    }

    [Fact]
    public async Task SubmitAsync_ThrowsWhenNotDraft()
    {
        var workflow = CreateWorkflow();
        workflow.Submit("planner", null, DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().SubmitAsync(workflow.Id, new RecommendationWorkflowActionRequest
            {
                Actor = "planner"
            }));
    }

    [Fact]
    public async Task ApproveAsync_PersistsApproverMetadata()
    {
        var recommendation = CreateRecommendation();
        var workflow = CreateWorkflow(recommendation.Id);
        workflow.Submit("planner", null, DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);
        _repository
            .Setup(repository => repository.UpdateAsync(It.IsAny<RecommendationWorkflow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecommendationWorkflow item, CancellationToken _) => item);
        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(recommendation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);

        var result = await CreateService().ApproveAsync(workflow.Id, new ApproveRecommendationWorkflowRequest
        {
            Approver = "approver@agencyos.local",
            Comment = "Approved"
        });

        Assert.Equal(RecommendationWorkflowStatus.Approved, result.Status);
        Assert.Equal("approver@agencyos.local", result.Approver);
        Assert.NotNull(result.ApprovalDate);
    }

    [Fact]
    public async Task ReopenAsync_RejectsCancelledWorkflow()
    {
        var workflow = CreateWorkflow();
        workflow.Cancel("planner", null, DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().ReopenAsync(workflow.Id, new RecommendationWorkflowActionRequest
            {
                Actor = "planner"
            }));
    }

    [Fact]
    public async Task CancelAsync_FromDraft_Succeeds()
    {
        var recommendation = CreateRecommendation();
        var workflow = CreateWorkflow(recommendation.Id);
        _repository
            .Setup(repository => repository.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);
        _repository
            .Setup(repository => repository.UpdateAsync(It.IsAny<RecommendationWorkflow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecommendationWorkflow item, CancellationToken _) => item);
        _recommendationRepository
            .Setup(repository => repository.GetByIdAsync(recommendation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);

        var result = await CreateService().CancelAsync(workflow.Id, new RecommendationWorkflowActionRequest
        {
            Actor = "planner",
            Comment = "Withdrawn"
        });

        Assert.Equal(RecommendationWorkflowStatus.Cancelled, result.Status);
    }

    [Fact]
    public void Validators_RequireActorAndApprover()
    {
        var actionValidator = new RecommendationWorkflowActionRequestValidator();
        Assert.False(actionValidator.Validate(new RecommendationWorkflowActionRequest()).IsValid);

        var approveValidator = new ApproveRecommendationWorkflowRequestValidator();
        Assert.False(approveValidator.Validate(new ApproveRecommendationWorkflowRequest()).IsValid);
    }

    private static RecommendationWorkflow CreateWorkflow(Guid? recommendationId = null) =>
        RecommendationWorkflow.Create(
            recommendationId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Balanced delivery mix",
            "Top ranked",
            "planner@agencyos.local",
            1,
            0.91m,
            DateTimeOffset.UtcNow);

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-TEST-001",
            "Balanced delivery mix",
            "Top ranked",
            "Reason",
            0.91m,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"strategy\":\"balanced\"}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}
