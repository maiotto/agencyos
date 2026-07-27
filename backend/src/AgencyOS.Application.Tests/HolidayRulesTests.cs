using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class HolidayRulesTests
{
    [Fact]
    public void Create_NationalHoliday_ClearsStateAndCity()
    {
        var holiday = Holiday.Create(
            Guid.NewGuid(),
            "Independence Day",
            "National",
            HolidayTypes.National,
            new DateOnly(2026, 9, 7),
            "SP",
            "Sao Paulo",
            true,
            DateTimeOffset.UtcNow);

        Assert.Null(holiday.StateCode);
        Assert.Null(holiday.City);
        Assert.Equal(HolidayStatus.Inactive, holiday.Status);
        Assert.True(holiday.Recurring);
    }

    [Fact]
    public void Create_CompanyHoliday_RequiresCompanyId()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Holiday.Create(
                null,
                "Foundation Day",
                null,
                HolidayTypes.Company,
                new DateOnly(2026, 11, 15),
                null,
                null,
                false,
                DateTimeOffset.UtcNow));

        Assert.Contains("CompanyId", exception.Message);
    }

    [Fact]
    public void Create_MunicipalHoliday_RequiresStateAndCity()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Holiday.Create(
                Guid.NewGuid(),
                "City Day",
                null,
                HolidayTypes.Municipal,
                new DateOnly(2026, 1, 25),
                "SP",
                null,
                true,
                DateTimeOffset.UtcNow));

        Assert.Contains("City", exception.Message);
    }

    [Fact]
    public void OccursOn_RecurringHoliday_MatchesMonthAndDay()
    {
        var holiday = Holiday.Create(
            null,
            "New Year",
            null,
            HolidayTypes.National,
            new DateOnly(2020, 1, 1),
            null,
            null,
            true,
            DateTimeOffset.UtcNow);

        Assert.True(holiday.OccursOn(new DateOnly(2026, 1, 1)));
        Assert.False(holiday.OccursOn(new DateOnly(2026, 1, 2)));
    }

    [Fact]
    public void EnsureCanDelete_ThrowsWhenActive()
    {
        var holiday = Holiday.Create(
            Guid.NewGuid(),
            "Future Holiday",
            null,
            HolidayTypes.Company,
            new DateOnly(2099, 5, 1),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
        holiday.Activate(DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            holiday.EnsureCanDelete(new DateOnly(2026, 7, 26)));

        Assert.Contains("Deactivate first", exception.Message);
    }

    [Fact]
    public void WorkingCalendar_IsWorkingDay_ExcludesActiveHolidays()
    {
        var companyId = Guid.NewGuid();
        var calendar = WorkingCalendar.Create(
            companyId,
            "Weekdays",
            new DateOnly(2026, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);

        var holiday = Holiday.Create(
            companyId,
            "Company Day",
            null,
            HolidayTypes.Company,
            new DateOnly(2026, 8, 3),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
        holiday.Activate(DateTimeOffset.UtcNow);

        Assert.True(calendar.IsWorkingDay(new DateOnly(2026, 8, 3)));
        Assert.False(calendar.IsWorkingDay(new DateOnly(2026, 8, 3), [holiday]));
    }

    [Fact]
    public void SameScope_DetectsDuplicateCompanyHoliday()
    {
        var companyId = Guid.NewGuid();
        var first = Holiday.Create(
            companyId,
            "A",
            null,
            HolidayTypes.Company,
            new DateOnly(2026, 12, 25),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);

        Assert.True(first.SameScopeAs(
            HolidayTypes.Company,
            companyId,
            null,
            null,
            new DateOnly(2026, 12, 25),
            false));
    }
}
