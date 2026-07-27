using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class PortfolioRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsWithMissions()
    {
        await using var context = CreateContext();
        var repository = new PortfolioRepository(context);
        var portfolio = CreatePortfolio("Alpha");

        await repository.AddAsync(portfolio);
        var loaded = await repository.GetByIdAsync(portfolio.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Alpha", loaded!.Name);
        Assert.Single(loaded.Missions);
    }

    [Fact]
    public async Task Query_FiltersBySearch()
    {
        await using var context = CreateContext();
        var repository = new PortfolioRepository(context);
        await repository.AddAsync(CreatePortfolio("Alpha Delivery"));
        await repository.AddAsync(CreatePortfolio("Beta Delivery"));

        var results = await repository.GetAllAsync(new PortfolioQueryParameters { Search = "Alpha" });

        Assert.Single(results);
        Assert.Equal("Alpha Delivery", results[0].Name);
    }

    [Fact]
    public async Task Update_PersistsMissionAssociation()
    {
        await using var context = CreateContext();
        var repository = new PortfolioRepository(context);
        var portfolio = CreatePortfolio();
        await repository.AddAsync(portfolio);

        var secondMission = Guid.NewGuid();
        portfolio.AssociateMission(secondMission, 2, DateTimeOffset.UtcNow);
        await repository.UpdateAsync(portfolio);

        context.ChangeTracker.Clear();
        var loaded = await repository.GetByIdAsync(portfolio.Id);
        Assert.Equal(2, loaded!.Missions.Count);
    }

    [Fact]
    public async Task ExistsByCompanyAndName_DetectsDuplicates()
    {
        await using var context = CreateContext();
        var repository = new PortfolioRepository(context);
        var portfolio = CreatePortfolio("Unique Name");
        await repository.AddAsync(portfolio);

        Assert.True(await repository.ExistsByCompanyAndNameAsync(portfolio.CompanyId, "unique name"));
        Assert.False(await repository.ExistsByCompanyAndNameAsync(
            portfolio.CompanyId,
            "unique name",
            portfolio.Id));
    }

    private static Portfolio CreatePortfolio(string name = "Q3 Portfolio") =>
        Portfolio.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            name,
            "Description",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 9, 30),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
