using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class AuditEventTests
{
    [Fact]
    public void Create_PersistsImmutableAuditEvent()
    {
        var audit = AuditEvent.Create(
            AuditEntityTypes.Decision,
            Guid.NewGuid(),
            "1",
            AuditEventTypes.Created,
            "Decision.Create",
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "planner",
            "Planner",
            DateTimeOffset.UtcNow,
            AuditSources.Api,
            Guid.NewGuid(),
            "session-1",
            "request-1",
            "{\"status\":\"Created\"}",
            "{\"status\":\"InProgress\"}",
            "{\"note\":\"ok\"}");

        Assert.Equal(AuditEntityTypes.Decision, audit.EntityType);
        Assert.Equal(AuditEventTypes.Created, audit.EventType);
        Assert.NotEqual(Guid.Empty, audit.Id);
    }

    [Fact]
    public void Create_RejectsEmptyMandatoryFields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AuditEvent.Create(
                " ",
                Guid.NewGuid(),
                null,
                AuditEventTypes.Created,
                "Action",
                null,
                "user",
                "user",
                DateTimeOffset.UtcNow,
                AuditSources.Api,
                null,
                null,
                null,
                null,
                null,
                null));
    }
}
