using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CapacityHistoryTests
{
    [Fact]
    public void Create_PersistsImmutableSnapshot()
    {
        var calculationDate = DateTimeOffset.Parse("2026-07-26T12:00:00Z");
        var history = CapacityHistory.Create(
            executionResourceId: Guid.Parse("77777777-7777-4777-8777-777777777777"),
            companyId: Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            periodStart: new DateOnly(2026, 7, 1),
            periodEnd: new DateOnly(2026, 7, 7),
            workingDays: 5,
            holidayDays: 0,
            availableDays: 5,
            configuredHours: 40,
            availableHours: 30,
            capacityHours: 40,
            allocatedHours: 10,
            utilizationPercentage: 25,
            calculationVersion: CapacityHistoryVersions.Current,
            operationalInputsJson: """{"operationalDays":[]}""",
            calculationDate: calculationDate);

        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.Equal(CapacityHistoryVersions.Current, history.CalculationVersion);
        Assert.Equal(calculationDate, history.CreatedAt);
        Assert.Equal(40, history.CapacityHours);
    }

    [Fact]
    public void Create_RejectsEmptyExecutionResource()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(executionResourceId: Guid.Empty));
    }

    [Fact]
    public void Create_RejectsEmptyCompany()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(companyId: Guid.Empty));
    }

    [Fact]
    public void Create_RejectsInvertedPeriod()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CapacityHistory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2026, 7, 10),
                new DateOnly(2026, 7, 1),
                1,
                0,
                1,
                8,
                8,
                8,
                0,
                0,
                CapacityHistoryVersions.Current,
                """{"operationalDays":[]}""",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_RejectsBlankCalculationVersion()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CreateValid(calculationVersion: " "));
    }

    private static CapacityHistory CreateValid(
        Guid? executionResourceId = null,
        Guid? companyId = null,
        string calculationVersion = CapacityHistoryVersions.Current)
    {
        return CapacityHistory.Create(
            executionResourceId ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            5,
            0,
            5,
            40,
            30,
            40,
            10,
            25,
            calculationVersion,
            """{"operationalDays":[]}""",
            DateTimeOffset.UtcNow);
    }
}
