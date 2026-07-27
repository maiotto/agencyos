using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class PortfolioTests
{
    [Fact]
    public void Create_RequiresNameAndMission_StartsActive()
    {
        var portfolio = CreateValid();

        Assert.Equal(PortfolioStatus.Active, portfolio.Status);
        Assert.Single(portfolio.Missions);
        Assert.Equal(PortfolioHealth.Unknown, portfolio.PortfolioHealth);
    }

    [Fact]
    public void Create_RejectsEmptyNameAndEmptyMissions()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(name: " "));
        Assert.Throws<InvalidOperationException>(() =>
            Portfolio.Create(
                Guid.NewGuid(),
                "Name",
                null,
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 31),
                null,
                [],
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AssociateMission_RejectsDuplicate_AndRemoveLast()
    {
        var portfolio = CreateValid();
        var second = Guid.NewGuid();
        portfolio.AssociateMission(second, 2, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.AssociateMission(second, 3, DateTimeOffset.UtcNow));

        portfolio.RemoveMission(second, DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() =>
            portfolio.RemoveMission(portfolio.Missions.First().MissionId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Inactive_CannotBeModified_ActiveCannotBeDeleted()
    {
        var portfolio = CreateValid();
        portfolio.Deactivate(DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.Update("X", null, portfolio.PlanningPeriodStart, portfolio.PlanningPeriodEnd, DateTimeOffset.UtcNow));

        portfolio.Activate(DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => portfolio.EnsureCanDelete());

        portfolio.Deactivate(DateTimeOffset.UtcNow);
        portfolio.EnsureCanDelete();
    }

    [Fact]
    public void CalculateHealth_IsDeterministic()
    {
        Assert.Equal(PortfolioHealth.Healthy, PortfolioHealth.Calculate(60m, 55m, 85m));
        Assert.Equal(PortfolioHealth.AtRisk, PortfolioHealth.Calculate(90m, 70m, 85m));
        Assert.Equal(PortfolioHealth.Overloaded, PortfolioHealth.Calculate(110m, 80m, 85m));
        Assert.Equal(PortfolioHealth.Underutilized, PortfolioHealth.Calculate(20m, 15m, 85m));
    }

    private static Portfolio CreateValid(string name = "Q3 Portfolio") =>
        Portfolio.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            name,
            "Description",
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 9, 30),
            null,
            [(Guid.NewGuid(), 1)],
            DateTimeOffset.UtcNow);
}
