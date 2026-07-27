using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using Moq;

namespace AgencyOS.Application.Tests;

public class PersonalTimelineServiceTests
{
    private readonly Mock<IAuditQueryService> _auditQueryService = new();

    private PersonalTimelineService CreateService() => new(_auditQueryService.Object);

    private static AuditEventResponse CreateEvent(
        Guid? companyId,
        DateTimeOffset occurredAt,
        string entityType = "Decision",
        string eventType = "StatusChanged",
        string action = "Update") => new()
    {
        Id = Guid.NewGuid(),
        EntityType = entityType,
        EntityId = Guid.NewGuid(),
        EventType = eventType,
        Action = action,
        CompanyId = companyId,
        UserId = "alice",
        UserName = "Alice",
        OccurredAt = occurredAt,
        Source = "Api"
    };

    [Fact]
    public async Task GetTimelineAsync_DelegatesToGetByUserId()
    {
        _auditQueryService
            .Setup(service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse>());

        var service = CreateService();
        await service.GetTimelineAsync("alice", Guid.NewGuid(), null, null);

        _auditQueryService.Verify(
            service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTimelineAsync_FiltersOutOtherCompanies()
    {
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        _auditQueryService
            .Setup(service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse>
            {
                CreateEvent(companyId, now),
                CreateEvent(otherCompanyId, now),
                CreateEvent(null, now)
            });

        var service = CreateService();
        var items = await service.GetTimelineAsync("alice", companyId, null, null);

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetTimelineAsync_AppliesFromAndToBounds()
    {
        var companyId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        _auditQueryService
            .Setup(service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse>
            {
                CreateEvent(companyId, now.AddDays(-10)),
                CreateEvent(companyId, now.AddDays(-2)),
                CreateEvent(companyId, now)
            });

        var service = CreateService();
        var items = await service.GetTimelineAsync("alice", companyId, now.AddDays(-5), now.AddDays(-1));

        Assert.Single(items);
    }

    [Fact]
    public async Task GetTimelineAsync_OrdersByOccurredAtDescending()
    {
        var companyId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var older = CreateEvent(companyId, now.AddDays(-1));
        var newer = CreateEvent(companyId, now);

        _auditQueryService
            .Setup(service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse> { older, newer });

        var service = CreateService();
        var items = await service.GetTimelineAsync("alice", companyId, null, null);

        Assert.Equal(newer.Id, items[0].Id);
        Assert.Equal(older.Id, items[1].Id);
    }

    [Fact]
    public async Task GetTimelineAsync_MapsDrillDownPathToAuditEvent()
    {
        var companyId = Guid.NewGuid();
        var auditEvent = CreateEvent(companyId, DateTimeOffset.UtcNow);

        _auditQueryService
            .Setup(service => service.GetByUserIdAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditEventResponse> { auditEvent });

        var service = CreateService();
        var items = await service.GetTimelineAsync("alice", companyId, null, null);

        Assert.Equal($"/audit/{auditEvent.Id}", items[0].DrillDownPath);
    }
}
