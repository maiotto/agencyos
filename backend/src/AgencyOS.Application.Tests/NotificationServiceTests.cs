using AgencyOS.Application.Audit;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using AgencyOS.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgencyOS.Application.Tests;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task AddQueryAndUnreadCount_Work()
    {
        await using var context = CreateContext();
        await SeedCompanyAsync(context);
        var repository = new NotificationRepository(context);

        var notification = Notification.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "planner",
            "Title",
            "Message body",
            NotificationCategory.Decision,
            NotificationPriority.High,
            NotificationSourceEntities.Decision,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        await repository.AddAsync(notification);

        var results = await repository.QueryAsync(new NotificationQueryParameters
        {
            CompanyId = AgencyOSCompanies.DefaultCompanyId,
            UserId = "planner",
            Status = NotificationStatus.Unread
        });

        var unread = await repository.CountUnreadAsync(AgencyOSCompanies.DefaultCompanyId, "planner");

        Assert.Single(results);
        Assert.Equal(1, unread);
        Assert.NotNull(await repository.GetByIdAsync(notification.Id));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedCompanyAsync(ApplicationDbContext context)
    {
        // In-memory tests do not enforce FK; company seed keeps company id consistent with fixtures.
        await context.SaveChangesAsync();
    }
}

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly CompanyContext _companyContext = new();
    private readonly NullAuditContext _auditContext = new();

    public NotificationServiceTests()
    {
        _companyContext.CompanyId = AgencyOSCompanies.DefaultCompanyId;
        _companyContext.IsSelected = true;
    }

    private NotificationService CreateService() =>
        new(
            _repository.Object,
            _companyContext,
            _auditContext,
            new NoOpAuditService(),
            NullLogger<NotificationService>.Instance);

    private NotificationQueryService CreateQueryService() =>
        new(_repository.Object, _companyContext, _auditContext);

    [Fact]
    public async Task MarkRead_UpdatesStatus()
    {
        var notification = CreateNotification();
        _repository
            .Setup(repository => repository.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _repository
            .Setup(repository => repository.UpdateAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await CreateService().MarkReadAsync(notification.Id);

        Assert.Equal(NotificationStatus.Read, result.Status);
        Assert.NotNull(result.ReadAt);
    }

    [Fact]
    public async Task MarkUnread_ClearsReadAt()
    {
        var notification = CreateNotification();
        notification.MarkRead(DateTimeOffset.UtcNow);
        _repository
            .Setup(repository => repository.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _repository
            .Setup(repository => repository.UpdateAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await CreateService().MarkUnreadAsync(notification.Id);

        Assert.Equal(NotificationStatus.Unread, result.Status);
        Assert.Null(result.ReadAt);
    }

    [Fact]
    public async Task Archive_MarksArchived()
    {
        var notification = CreateNotification();
        _repository
            .Setup(repository => repository.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        _repository
            .Setup(repository => repository.UpdateAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var result = await CreateService().ArchiveAsync(notification.Id);

        Assert.True(result.Archived);
    }

    [Fact]
    public async Task GetById_ThrowsWhenOutsideUserScope()
    {
        var notification = Notification.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "other-user",
            "Title",
            "Message",
            NotificationCategory.Decision,
            NotificationPriority.High,
            NotificationSourceEntities.Decision,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateQueryService().GetByIdAsync(notification.Id));
    }

    private static Notification CreateNotification() =>
        Notification.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "system",
            "Title",
            "Message",
            NotificationCategory.Decision,
            NotificationPriority.High,
            NotificationSourceEntities.Decision,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}

public class NotificationGenerationServiceTests
{
    [Fact]
    public async Task GenerateSafeAsync_PersistsNotification()
    {
        var repository = new Mock<INotificationRepository>();
        Notification? created = null;
        repository
            .Setup(item => item.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((notification, _) => created = notification)
            .ReturnsAsync((Notification notification, CancellationToken _) => notification);

        var service = new NotificationGenerationService(
            repository.Object,
            new CompanyContext { CompanyId = AgencyOSCompanies.DefaultCompanyId, IsSelected = true },
            new NullAuditContext(),
            NullLogger<NotificationGenerationService>.Instance);

        await service.GenerateSafeAsync(new NotificationGenerationRequest
        {
            Title = "Generated",
            Message = "From test",
            Category = NotificationCategory.Audit,
            Priority = NotificationPriority.Low,
            SourceEntity = NotificationSourceEntities.AuditEvent,
            SourceEntityId = Guid.NewGuid()
        });

        Assert.NotNull(created);
        Assert.Equal("Generated", created!.Title);
    }

    [Fact]
    public async Task GenerateSafeAsync_DoesNotThrow_WhenRepositoryFails()
    {
        var repository = new Mock<INotificationRepository>();
        repository
            .Setup(item => item.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db down"));

        var service = new NotificationGenerationService(
            repository.Object,
            new CompanyContext(),
            new NullAuditContext(),
            NullLogger<NotificationGenerationService>.Instance);

        await service.GenerateSafeAsync(new NotificationGenerationRequest
        {
            Title = "Generated",
            Message = "From test",
            Category = NotificationCategory.System,
            Priority = NotificationPriority.Low,
            SourceEntity = NotificationSourceEntities.System
        });
    }
}
