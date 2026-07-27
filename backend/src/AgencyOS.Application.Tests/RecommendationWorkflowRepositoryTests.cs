using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class RecommendationWorkflowRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsWithTransitions()
    {
        await using var context = CreateContext();
        var repository = new RecommendationWorkflowRepository(context);
        var workflow = CreateWorkflow();
        workflow.Submit("planner", "submit", DateTimeOffset.UtcNow);

        await repository.AddAsync(workflow);
        var loaded = await repository.GetByIdAsync(workflow.Id);

        Assert.NotNull(loaded);
        Assert.Equal(RecommendationWorkflowStatus.PendingApproval, loaded!.Status);
        Assert.Equal(2, loaded.Transitions.Count);
    }

    [Fact]
    public async Task Query_FiltersByStatusAndSearch()
    {
        await using var context = CreateContext();
        var repository = new RecommendationWorkflowRepository(context);

        var pending = CreateWorkflow("Alpha Strategy");
        pending.Submit("planner", null, DateTimeOffset.UtcNow);
        await repository.AddAsync(pending);

        await repository.AddAsync(CreateWorkflow("Other"));

        var results = await repository.GetAllAsync(new RecommendationWorkflowQueryParameters
        {
            Status = RecommendationWorkflowStatus.PendingApproval,
            Search = "Alpha"
        });

        Assert.Single(results);
        Assert.Equal(pending.Id, results[0].Id);
    }

    [Fact]
    public async Task Update_PersistsApprovalTransition()
    {
        await using var context = CreateContext();
        var repository = new RecommendationWorkflowRepository(context);
        var workflow = CreateWorkflow();
        await repository.AddAsync(workflow);

        workflow.Submit("planner", null, DateTimeOffset.UtcNow);
        workflow.Approve("approver", DateTimeOffset.UtcNow, "ok", DateTimeOffset.UtcNow);
        await repository.UpdateAsync(workflow);

        context.ChangeTracker.Clear();
        var loaded = await repository.GetByIdAsync(workflow.Id);
        Assert.Equal(RecommendationWorkflowStatus.Approved, loaded!.Status);
        Assert.Equal(3, loaded.Transitions.Count);
        Assert.Equal("approver", loaded.Approver);
    }


    private static RecommendationWorkflow CreateWorkflow(string title = "Balanced delivery mix") =>
        RecommendationWorkflow.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            title,
            "Summary",
            "planner@agencyos.local",
            1,
            0.9m,
            DateTimeOffset.UtcNow);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
