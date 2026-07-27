using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class AuditHookTests
{
    [Fact]
    public async Task DecisionCreate_RecordsAuditEvent()
    {
        var recommendation = Recommendation.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-AUDIT-1",
            "Human + AI",
            null,
            null,
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

        var decisionRepository = new Mock<IDecisionRepository>();
        var recommendationRepository = new Mock<IRecommendationRepository>();
        var workflowRepository = new Mock<IRecommendationWorkflowRepository>();
        var auditService = new Mock<IAuditService>();
        AuditEventWriteRequest? recorded = null;

        recommendationRepository.Setup(repository => repository.GetByIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recommendation);
        workflowRepository.Setup(repository => repository.GetAllAsync(
                It.IsAny<RecommendationWorkflowQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { workflow });
        decisionRepository.Setup(repository => repository.ExistsByRecommendationIdAsync(
                recommendation.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        decisionRepository.Setup(repository => repository.AddAsync(
                It.IsAny<Decision>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Decision decision, CancellationToken _) => decision);
        auditService.Setup(service => service.RecordSafeAsync(
                It.IsAny<AuditEventWriteRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<AuditEventWriteRequest, CancellationToken>((request, _) => recorded = request)
            .Returns(Task.CompletedTask);

        var service = new DecisionService(
            decisionRepository.Object,
            recommendationRepository.Object,
            workflowRepository.Object,
            auditService.Object,
            NullLogger<DecisionService>.Instance);

        await service.CreateAsync(new CreateDecisionRequest
        {
            RecommendationId = recommendation.Id,
            CreatedBy = "planner"
        });

        Assert.NotNull(recorded);
        Assert.Equal(AuditEntityTypes.Decision, recorded!.EntityType);
        Assert.Equal(AuditEventTypes.Created, recorded.EventType);
        Assert.Equal("Decision.Create", recorded.Action);
    }

    [Fact]
    public async Task ConcurrentRecordSafe_DoesNotThrow()
    {
        var repository = new Mock<IAuditEventRepository>();
        repository.Setup(item => item.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent item, CancellationToken _) => item);

        var service = new AuditService(
            repository.Object,
            new AgencyOS.Application.Audit.NullAuditContext(),
            new AgencyOS.Application.Audit.NoOpNotificationGenerationService(),
            NullLogger<AuditService>.Instance);

        var tasks = Enumerable.Range(0, 20).Select(index =>
            service.RecordSafeAsync(new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Decision,
                EntityId = Guid.NewGuid(),
                EventType = AuditEventTypes.Created,
                Action = $"Decision.Create.{index}",
                UserId = "planner",
                UserName = "planner"
            }));

        var exception = await Record.ExceptionAsync(() => Task.WhenAll(tasks));
        Assert.Null(exception);
        repository.Verify(
            item => item.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(20));
    }
}
