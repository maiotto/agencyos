using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class NotificationTests
{
    [Fact]
    public void Create_CreatesUnreadNotification()
    {
        var item = CreateNotification();

        Assert.Equal(NotificationStatus.Unread, item.Status);
        Assert.False(item.Archived);
        Assert.Null(item.ReadAt);
        Assert.True(item.Validate());
        Assert.True(item.IsUnread);
    }

    [Fact]
    public void MarkRead_And_MarkUnread_ToggleStatus()
    {
        var item = CreateNotification();
        var readAt = DateTimeOffset.UtcNow;

        item.MarkRead(readAt);
        Assert.True(item.IsRead);
        Assert.Equal(readAt, item.ReadAt);

        item.MarkUnread();
        Assert.True(item.IsUnread);
        Assert.Null(item.ReadAt);
    }

    [Fact]
    public void Archive_MarksArchived_AndBlocksReadChanges()
    {
        var item = CreateNotification();
        item.Archive();

        Assert.True(item.Archived);
        Assert.Throws<InvalidOperationException>(() => item.Archive());
        Assert.Throws<InvalidOperationException>(() => item.MarkRead(DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => item.MarkUnread());
    }

    [Fact]
    public void Create_RejectsInvalidInputs()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Notification.Create(
                Guid.Empty,
                "user",
                "title",
                "message",
                NotificationCategory.Decision,
                NotificationPriority.High,
                NotificationSourceEntities.Decision,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));

        Assert.Throws<InvalidOperationException>(() =>
            Notification.Create(
                AgencyOSCompanies.DefaultCompanyId,
                " ",
                "title",
                "message",
                NotificationCategory.Decision,
                NotificationPriority.High,
                NotificationSourceEntities.Decision,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));

        Assert.Throws<InvalidOperationException>(() =>
            Notification.Create(
                AgencyOSCompanies.DefaultCompanyId,
                "user",
                "title",
                "message",
                "Unknown",
                NotificationPriority.High,
                NotificationSourceEntities.Decision,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    private static Notification CreateNotification() =>
        Notification.Create(
            AgencyOSCompanies.DefaultCompanyId,
            "planner",
            "Decision awaiting action",
            "A decision was created from an approved recommendation.",
            NotificationCategory.Decision,
            NotificationPriority.High,
            NotificationSourceEntities.Decision,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
