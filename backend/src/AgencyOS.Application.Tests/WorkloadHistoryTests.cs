using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class WorkloadHistoryTests
{
    [Fact]
    public void Create_PersistsImmutableSnapshot()
    {
        var calculationDate = DateTimeOffset.Parse("2026-07-26T12:00:00Z");
        var history = WorkloadHistory.Create(
            executionResourceId: Guid.Parse("77777777-7777-4777-8777-777777777777"),
            companyId: Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            periodStart: new DateOnly(2026, 7, 1),
            periodEnd: new DateOnly(2026, 7, 7),
            allocatedHours: 24,
            capacityHours: 40,
            workloadPercentage: 60,
            workingDays: 5,
            holidayDays: 0,
            availableDays: 5,
            calculationVersion: WorkloadHistoryVersions.Current,
            operationalInputsJson: """{"assignmentDistribution":[]}""",
            calculationDate: calculationDate);

        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.Equal(WorkloadHistoryVersions.Current, history.CalculationVersion);
        Assert.Equal(24, history.AllocatedHours);
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
            WorkloadHistory.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2026, 7, 10),
                new DateOnly(2026, 7, 1),
                8,
                40,
                20,
                1,
                0,
                1,
                WorkloadHistoryVersions.Current,
                """{"assignmentDistribution":[]}""",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_RejectsBlankCalculationVersion()
    {
        Assert.Throws<InvalidOperationException>(() => CreateValid(calculationVersion: " "));
    }

    private static WorkloadHistory CreateValid(
        Guid? executionResourceId = null,
        Guid? companyId = null,
        string calculationVersion = WorkloadHistoryVersions.Current)
    {
        return WorkloadHistory.Create(
            executionResourceId ?? Guid.NewGuid(),
            companyId ?? Guid.NewGuid(),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 7),
            24,
            40,
            60,
            5,
            0,
            5,
            calculationVersion,
            """{"assignmentDistribution":[]}""",
            DateTimeOffset.UtcNow);
    }
}
