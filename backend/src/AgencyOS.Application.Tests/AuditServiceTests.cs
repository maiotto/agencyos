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

    private AuditService CreateService(IAuditContext? context = null) =>
        new(
            _repository.Object,
            context ?? new NullAuditContext(),
            new NoOpNotificationGenerationService(),
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
}
