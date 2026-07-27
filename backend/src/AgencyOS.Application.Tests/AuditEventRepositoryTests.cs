using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class AuditEventRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTrips()
    {
        await using var context = CreateContext();
        var repository = new AuditEventRepository(context);
        var audit = AuditEvent.Create(
            AuditEntityTypes.Decision,
            Guid.NewGuid(),
            null,
            AuditEventTypes.Created,
            "Decision.Create",
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "planner",
            "planner",
            DateTimeOffset.UtcNow,
            AuditSources.Api,
            Guid.NewGuid(),
            null,
            null,
            null,
            "{\"ok\":true}",
            null);

        await repository.AddAsync(audit);
        var loaded = await repository.GetByIdAsync(audit.Id);

        Assert.NotNull(loaded);
        Assert.Equal(audit.EntityId, loaded!.EntityId);
    }

    [Fact]
    public async Task Query_FiltersByCompanyUserAndCorrelation()
    {
        await using var context = CreateContext();
        var repository = new AuditEventRepository(context);
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
        var correlationId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        await repository.AddAsync(AuditEvent.Create(
            AuditEntityTypes.Decision,
            entityId,
            null,
            AuditEventTypes.Created,
            "Decision.Create",
            companyId,
            "planner",
            "planner",
            DateTimeOffset.UtcNow,
            AuditSources.Api,
            correlationId,
            null,
            null,
            null,
            null,
            null));
        await repository.AddAsync(AuditEvent.Create(
            AuditEntityTypes.Recommendation,
            Guid.NewGuid(),
            "1",
            AuditEventTypes.Created,
            "Recommendation.Create",
            Guid.NewGuid(),
            "other",
            "other",
            DateTimeOffset.UtcNow,
            AuditSources.DecisionEngine,
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null));

        var byCompany = await repository.QueryAsync(new AuditEventQueryParameters { CompanyId = companyId });
        var byUser = await repository.QueryAsync(new AuditEventQueryParameters { UserId = "planner" });
        var byCorrelation = await repository.QueryAsync(new AuditEventQueryParameters { CorrelationId = correlationId });
        var byEntity = await repository.QueryAsync(new AuditEventQueryParameters { EntityId = entityId });

        Assert.Single(byCompany);
        Assert.Single(byUser);
        Assert.Single(byCorrelation);
        Assert.Single(byEntity);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
