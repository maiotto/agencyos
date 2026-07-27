using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class HolidayServiceTests
{
    private readonly Mock<IHolidayRepository> _holidayRepository = new();
    private readonly Mock<ILogger<HolidayService>> _logger = new();

    private HolidayService CreateService() =>
        new(_holidayRepository.Object, _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenMissing()
    {
        var holidayId = Guid.NewGuid();
        _holidayRepository
            .Setup(repository => repository.GetByIdAsync(holidayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Holiday?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(holidayId));
    }

    [Fact]
    public async Task CreateAsync_PersistsInactiveHoliday()
    {
        Holiday? persisted = null;

        _holidayRepository
            .Setup(repository => repository.ExistsWithSameScopeAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateOnly>(),
                It.IsAny<bool>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _holidayRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Holiday>(), It.IsAny<CancellationToken>()))
            .Callback<Holiday, CancellationToken>((holiday, _) => persisted = holiday)
            .ReturnsAsync((Holiday holiday, CancellationToken _) => holiday);

        var result = await CreateService().CreateAsync(CreateValidCompanyRequest());

        Assert.NotNull(persisted);
        Assert.Equal(HolidayStatus.Inactive, persisted!.Status);
        Assert.Equal("Foundation Day", result.Name);
        Assert.Equal(HolidayTypes.Company, result.HolidayType);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictOnDuplicateScope()
    {
        _holidayRepository
            .Setup(repository => repository.ExistsWithSameScopeAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateOnly>(),
                It.IsAny<bool>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(CreateValidCompanyRequest()));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenHistorical()
    {
        var holiday = Holiday.Create(
            Guid.NewGuid(),
            "Past",
            null,
            HolidayTypes.Company,
            new DateOnly(2020, 1, 1),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);

        _holidayRepository
            .Setup(repository => repository.GetByIdAsync(holiday.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holiday);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().UpdateAsync(holiday.Id, new UpdateHolidayRequest
            {
                CompanyId = holiday.CompanyId,
                Name = "Changed",
                HolidayType = HolidayTypes.Company,
                HolidayDate = new DateOnly(2020, 2, 1)
            }));
    }

    [Fact]
    public async Task ActivateAsync_ActivatesWhenUnique()
    {
        var holiday = CreateFutureHoliday();

        _holidayRepository
            .Setup(repository => repository.GetByIdAsync(holiday.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holiday);
        _holidayRepository
            .Setup(repository => repository.ExistsWithSameScopeAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateOnly>(),
                It.IsAny<bool>(),
                holiday.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _holidayRepository
            .Setup(repository => repository.UpdateAsync(holiday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holiday);

        await CreateService().ActivateAsync(holiday.Id);

        Assert.Equal(HolidayStatus.Active, holiday.Status);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenActive()
    {
        var holiday = CreateFutureHoliday();
        holiday.Activate(DateTimeOffset.UtcNow);

        _holidayRepository
            .Setup(repository => repository.GetByIdAsync(holiday.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holiday);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().DeleteAsync(holiday.Id));
    }

    [Fact]
    public async Task DeleteAsync_RemovesInactiveFutureHoliday()
    {
        var holiday = CreateFutureHoliday();

        _holidayRepository
            .Setup(repository => repository.GetByIdAsync(holiday.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(holiday);
        _holidayRepository
            .Setup(repository => repository.DeleteAsync(holiday, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateService().DeleteAsync(holiday.Id);

        _holidayRepository.Verify(
            repository => repository.DeleteAsync(holiday, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOperationalWorkingDay_ViaWorkingCalendarService_MarksHoliday()
    {
        var companyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
        var date = new DateOnly(2099, 8, 3);

        var calendar = WorkingCalendar.Create(
            companyId,
            "Weekdays",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);

        var holiday = Holiday.Create(
            companyId,
            "Company Day",
            null,
            HolidayTypes.Company,
            date,
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
        holiday.Activate(DateTimeOffset.UtcNow);

        var calendarRepository = new Mock<IWorkingCalendarRepository>();
        var holidayRepository = new Mock<IHolidayRepository>();
        var logger = new Mock<ILogger<WorkingCalendarService>>();

        calendarRepository
            .Setup(repository => repository.GetActiveCoveringDateAsync(
                companyId,
                date,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        holidayRepository
            .Setup(repository => repository.GetActiveForCompanyOnDateAsync(
                companyId,
                date,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { holiday });

        var service = new WorkingCalendarService(
            calendarRepository.Object,
            holidayRepository.Object,
            new Mock<IWorkingHoursRepository>().Object,
            logger.Object);
        var result = await service.GetOperationalWorkingDayAsync(companyId, date);

        Assert.True(result.IsCalendarWorkingWeekday);
        Assert.True(result.IsHoliday);
        Assert.False(result.IsWorkingDay);
        Assert.Single(result.Holidays);
    }

    private static CreateHolidayRequest CreateValidCompanyRequest() =>
        new()
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Foundation Day",
            HolidayType = HolidayTypes.Company,
            HolidayDate = new DateOnly(2099, 11, 15),
            Recurring = false
        };

    private static Holiday CreateFutureHoliday() =>
        Holiday.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Foundation Day",
            null,
            HolidayTypes.Company,
            new DateOnly(2099, 11, 15),
            null,
            null,
            false,
            DateTimeOffset.UtcNow);
}
