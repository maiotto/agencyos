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

public class AuditServiceTests
{
    private readonly Mock<IAuditEventRepository> _repository = new();

    private AuditService CreateService(
        IAuditContext? context = null,
        INotificationGenerationService? notificationGenerationService = null) =>
        new(
            _repository.Object,
            context ?? new NullAuditContext(),
            notificationGenerationService ?? new NoOpNotificationGenerationService(),
            NullLogger<AuditService>.Instance);

    [Fact]
    public async Task RecordSafeAsync_PersistsAuditEvent()
    {
        AuditEvent? persisted = null;
        _repository.Setup(repository => repository.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEvent, CancellationToken>((item, _) => persisted = item)
            .ReturnsAsync((AuditEvent item, CancellationToken _) => item);

        await CreateService().RecordSafeAsync(new AuditEventWriteRequest
        {
            EntityType = AuditEntityTypes.Recommendation,
            EntityId = Guid.NewGuid(),
            EventType = AuditEventTypes.Created,
            Action = "Recommendation.Create",
            UserId = "decision-engine",
            UserName = "decision-engine",
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            CurrentState = "{\"status\":\"Active\"}"
        });

        Assert.NotNull(persisted);
        Assert.Equal(AuditEntityTypes.Recommendation, persisted!.EntityType);
    }

    [Fact]
    public async Task RecordSafeAsync_DoesNotThrow_WhenRepositoryFails()
    {
        _repository.Setup(repository => repository.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var exception = await Record.ExceptionAsync(() =>
            CreateService().RecordSafeAsync(new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Decision,
                EntityId = Guid.NewGuid(),
                EventType = AuditEventTypes.Created,
                Action = "Decision.Create",
                UserId = "planner",
                UserName = "planner"
            }));

        Assert.Null(exception);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFound()
    {
        _repository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task QueryHelpers_DelegateToRepositoryFilters()
    {
        _repository.Setup(repository => repository.QueryAsync(
                It.IsAny<AuditEventQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AuditEvent>());

        var service = CreateService();
        Assert.Empty(await service.GetByEntityIdAsync(Guid.NewGuid()));
        Assert.Empty(await service.GetByCorrelationIdAsync(Guid.NewGuid()));
        Assert.Empty(await service.GetByUserIdAsync("planner"));
        Assert.Empty(await service.GetByCompanyIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public void QueryValidator_RejectsInvalidDateRange()
    {
        var validator = new AuditEventQueryParametersValidator();
        var result = validator.Validate(new AuditEventQueryParameters
        {
            OccurredFrom = DateTimeOffset.UtcNow,
            OccurredTo = DateTimeOffset.UtcNow.AddDays(-1)
        });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(AuditEntityTypes.Decision, NotificationCategory.Decision, NotificationSourceEntities.Decision)]
    [InlineData(AuditEntityTypes.RecommendationWorkflow, NotificationCategory.Recommendation, NotificationSourceEntities.RecommendationWorkflow)]
    [InlineData(AuditEntityTypes.CapacityHistory, NotificationCategory.Capacity, NotificationSourceEntities.CapacityHistory)]
    [InlineData(AuditEntityTypes.Portfolio, NotificationCategory.Portfolio, NotificationSourceEntities.Portfolio)]
    [InlineData(AuditEntityTypes.PlanningTemplate, NotificationCategory.Planning, NotificationSourceEntities.PlanningWorkspace)]
    [InlineData(AuditEntityTypes.ExecutiveRecommendationSummary, NotificationCategory.Executive, NotificationSourceEntities.ExecutiveWorkspace)]
    [InlineData(AuditEntityTypes.Company, NotificationCategory.Company, NotificationSourceEntities.Company)]
    [InlineData(AuditEntityTypes.Recommendation, NotificationCategory.Recommendation, NotificationSourceEntities.RecommendationWorkspace)]
    public async Task RecordSafeAsync_GeneratesNotification_ForMappedEntityTypes(
        string entityType,
        string expectedCategory,
        string expectedSourceEntity)
    {
        SetupSuccessfulAuditPersist();
        var notifications = new Mock<INotificationGenerationService>();
        NotificationGenerationRequest? generated = null;
        notifications
            .Setup(service => service.GenerateSafeAsync(
                It.IsAny<NotificationGenerationRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<NotificationGenerationRequest, CancellationToken>((request, _) => generated = request)
            .Returns(Task.CompletedTask);

        var entityId = Guid.NewGuid();
        await CreateService(notificationGenerationService: notifications.Object).RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = entityType,
                EntityId = entityId,
                EventType = AuditEventTypes.Created,
                Action = $"{entityType}.Create",
                UserId = "planner",
                UserName = "planner",
                CompanyId = AgencyOSCompanies.DefaultCompanyId
            });

        Assert.NotNull(generated);
        Assert.Equal(expectedCategory, generated!.Category);
        Assert.Equal(expectedSourceEntity, generated.SourceEntity);
        Assert.Equal(entityId, generated.SourceEntityId);
        notifications.Verify(
            service => service.GenerateSafeAsync(
                It.IsAny<NotificationGenerationRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(AuditEntityTypes.Notification)]
    [InlineData(AuditEntityTypes.PersonalProductivityDashboard)]
    [InlineData(AuditEntityTypes.CrossPortfolioPlan)]
    [InlineData(AuditEntityTypes.WorkloadHistory)]
    [InlineData(AuditEntityTypes.AIRecommendation)]
    [InlineData(AuditEntityTypes.Explainability)]
    [InlineData(AuditEntityTypes.CompanyDecisionProfile)]
    public async Task RecordSafeAsync_DoesNotGenerateNotification_ForUnmappedOrNotificationEntityTypes(
        string entityType)
    {
        SetupSuccessfulAuditPersist();
        var notifications = new Mock<INotificationGenerationService>();

        await CreateService(notificationGenerationService: notifications.Object).RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = entityType,
                EntityId = Guid.NewGuid(),
                EventType = AuditEventTypes.Executed,
                Action = $"{entityType}.Action",
                UserId = "planner",
                UserName = "planner",
                CompanyId = AgencyOSCompanies.DefaultCompanyId
            });

        notifications.Verify(
            service => service.GenerateSafeAsync(
                It.IsAny<NotificationGenerationRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RecordSafeAsync_GeneratesNotification_ExactlyOnce_PerAuditEvent()
    {
        SetupSuccessfulAuditPersist();
        var notifications = new Mock<INotificationGenerationService>();

        await CreateService(notificationGenerationService: notifications.Object).RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Decision,
                EntityId = Guid.NewGuid(),
                EventType = AuditEventTypes.StatusChanged,
                Action = "Decision.Complete",
                UserId = "planner",
                UserName = "planner",
                CompanyId = AgencyOSCompanies.DefaultCompanyId
            });

        notifications.Verify(
            service => service.GenerateSafeAsync(
                It.IsAny<NotificationGenerationRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupSuccessfulAuditPersist()
    {
        _repository.Setup(repository => repository.AddAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditEvent item, CancellationToken _) => item);
    }
}
