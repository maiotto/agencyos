using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class DecisionServiceTests
{
    private readonly Mock<IDecisionRepository> _decisionRepository = new();
    private readonly Mock<IRecommendationRepository> _recommendationRepository = new();
    private readonly Mock<IRecommendationWorkflowRepository> _workflowRepository = new();

    private DecisionService CreateService() =>
        new(
            _decisionRepository.Object,
            _recommendationRepository.Object,
            _workflowRepository.Object,
            new NoOpAuditService(),
            NullLogger<DecisionService>.Instance);

    [Fact]
    public async Task CreateAsync_RequiresApprovedWorkflow()
    {
        var recommendation = CreateRecommendation();
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        _workflowRepository.Setup(repository => repository.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecommendationWorkflow>());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().CreateAsync(new CreateDecisionRequest
            {
                RecommendationId = recommendation.Id,
                CreatedBy = "planner"
            }));
    }

    [Fact]
    public async Task CreateAsync_PersistsDecision_WhenApproved()
    {
        var recommendation = CreateRecommendation();
        Decision? persisted = null;
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        var workflow = RecommendationWorkflow.Create(
            recommendation.Id,
            recommendation.DeliveryStrategyId,
            recommendation.ContractId,
            recommendation.MissionId,
            null,
            recommendation.Title,
            recommendation.Summary,
            "approver",
            recommendation.Rank,
            recommendation.Score,
            DateTimeOffset.UtcNow);
        workflow.Submit("approver", null, DateTimeOffset.UtcNow);
        workflow.Approve("approver", DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow);
        _workflowRepository.Setup(repository => repository.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { workflow });
        _decisionRepository.Setup(repository => repository.ExistsByRecommendationIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _decisionRepository.Setup(repository => repository.AddAsync(
                It.IsAny<Decision>(),
                It.IsAny<CancellationToken>()))
            .Callback<Decision, CancellationToken>((decision, _) => persisted = decision)
            .ReturnsAsync((Decision decision, CancellationToken _) => decision);

        var result = await CreateService().CreateAsync(new CreateDecisionRequest
        {
            RecommendationId = recommendation.Id,
            CreatedBy = "planner"
        });

        Assert.NotNull(persisted);
        Assert.Equal(recommendation.Id, result.RecommendationId);
        Assert.Equal(DecisionStatus.Created, result.DecisionStatus);
        Assert.Equal(DecisionImplementationStatus.NotStarted, result.ImplementationStatus);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateRecommendation()
    {
        var recommendation = CreateRecommendation();
        _recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        var workflow = RecommendationWorkflow.Create(
            recommendation.Id,
            recommendation.DeliveryStrategyId,
            recommendation.ContractId,
            recommendation.MissionId,
            null,
            recommendation.Title,
            null,
            "approver",
            1,
            80m,
            DateTimeOffset.UtcNow);
        workflow.Submit("approver", null, DateTimeOffset.UtcNow);
        workflow.Approve("approver", DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow);
        _workflowRepository.Setup(repository => repository.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { workflow });
        _decisionRepository.Setup(repository => repository.ExistsByRecommendationIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().CreateAsync(new CreateDecisionRequest
            {
                RecommendationId = recommendation.Id,
                CreatedBy = "planner"
            }));
    }

    [Fact]
    public async Task StartCompleteOutcome_UpdatesLifecycle()
    {
        var decision = Decision.Create(
            Guid.NewGuid(),
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "planner",
            DateTimeOffset.UtcNow);

        _decisionRepository.Setup(repository => repository.GetByIdAsync(decision.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(decision);
        _decisionRepository.Setup(repository => repository.UpdateAsync(It.IsAny<Decision>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Decision item, CancellationToken _) => item);

        var service = CreateService();
        var started = await service.StartImplementationAsync(
            decision.Id,
            new DecisionActionRequest { Actor = "planner" });
        Assert.Equal(DecisionStatus.InProgress, started.DecisionStatus);

        var completed = await service.CompleteAsync(
            decision.Id,
            new DecisionActionRequest { Actor = "planner" });
        Assert.Equal(DecisionStatus.Completed, completed.DecisionStatus);

        var withOutcome = await service.RecordOutcomeAsync(
            decision.Id,
            new RecordDecisionOutcomeRequest
            {
                Outcome = "Success",
                Actor = "planner",
                BusinessValue = "High"
            });
        Assert.Equal("Success", withOutcome.Outcome);
    }

    [Fact]
    public void Validators_RejectInvalidFiltersAndEmptyOutcome()
    {
        var queryValidator = new DecisionQueryParametersValidator();
        Assert.False(queryValidator.Validate(new DecisionQueryParameters
        {
            DecisionStatus = "Nope"
        }).IsValid);

        var outcomeValidator = new RecordDecisionOutcomeRequestValidator();
        Assert.False(outcomeValidator.Validate(new RecordDecisionOutcomeRequest
        {
            Outcome = "",
            Actor = "planner"
        }).IsValid);
    }

    private static Recommendation CreateRecommendation() =>
        Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-DEC-1",
            "Human + AI",
            "Summary",
            "Reason",
            80m,
            1,
            1,
            RecommendationVersions.CurrentDecisionEngineVersion,
            null,
            "{}",
            "{}",
            "{\"ok\":true}",
            "decision-engine",
            DateTimeOffset.UtcNow);
}
