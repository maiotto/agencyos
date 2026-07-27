using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class WorkingHoursServiceTests
{
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<ILogger<WorkingHoursService>> _logger = new();

    private WorkingHoursService CreateService() =>
        new(_workingHoursRepository.Object, _workingCalendarRepository.Object, _logger.Object);

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenCalendarMissing()
    {
        var calendarId = Guid.NewGuid();
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendarId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingCalendar?)null);

        var request = CreateValidRequest();
        request.WorkingCalendarId = calendarId;

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_PersistsInactiveConfiguration()
    {
        WorkingHours? persisted = null;
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        _workingHoursRepository
            .Setup(repository => repository.AddAsync(It.IsAny<WorkingHours>(), It.IsAny<CancellationToken>()))
            .Callback<WorkingHours, CancellationToken>((hours, _) => persisted = hours)
            .ReturnsAsync((WorkingHours hours, CancellationToken _) => hours);

        var request = CreateValidRequest();
        request.WorkingCalendarId = calendar.Id;

        var result = await CreateService().CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal(WorkingHoursStatus.Inactive, persisted!.Status);
        Assert.Equal(result.Id, persisted.Id);
        Assert.Equal(5, result.Days.Count(day => day.Enabled));
    }

    [Fact]
    public async Task ActivateAsync_ThrowsConflictWhenPeriodOverlaps()
    {
        var hours = CreateFutureHours();
        var overlapping = CreateFutureHours();
        overlapping.Activate(DateTimeOffset.UtcNow);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);
        _workingHoursRepository
            .Setup(repository => repository.GetActiveOverlappingAsync(
                hours.WorkingCalendarId,
                hours.EffectiveFrom,
                hours.EffectiveTo,
                hours.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overlapping });

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().ActivateAsync(hours.Id));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenActive()
    {
        var hours = CreateFutureHours();
        hours.Activate(DateTimeOffset.UtcNow);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().DeleteAsync(hours.Id));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenHistorical()
    {
        var hours = WorkingHours.Create(
            Guid.NewGuid(),
            "Historical",
            new DateOnly(2020, 1, 1),
            null,
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().UpdateAsync(hours.Id, new UpdateWorkingHoursRequest
            {
                Name = "Changed",
                EffectiveFrom = new DateOnly(2020, 2, 1),
                Days = CreateValidRequest().Days
            }));
    }

    private static CreateWorkingHoursRequest CreateValidRequest() =>
        new()
        {
            WorkingCalendarId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            Name = "Standard Office Hours",
            EffectiveFrom = new DateOnly(2099, 1, 1),
            EffectiveTo = new DateOnly(2099, 12, 31),
            Days = CreateWeekdayDefinitions()
                .Select(day => new WorkingHoursDayRequest
                {
                    DayOfWeek = day.DayOfWeek,
                    Enabled = day.Enabled,
                    StartTime = day.StartTime,
                    EndTime = day.EndTime,
                    BreakStart = day.BreakStart,
                    BreakEnd = day.BreakEnd
                })
                .ToList()
        };

    private static WorkingHours CreateFutureHours() =>
        WorkingHours.Create(
            Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb"),
            "Standard Office Hours",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);

    private static IReadOnlyList<WorkingHoursDayDefinition> CreateWeekdayDefinitions() =>
        WorkingDayNames.DefaultWeekdays
            .Select(day => new WorkingHoursDayDefinition
            {
                DayOfWeek = day,
                Enabled = true,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(18, 0),
                BreakStart = new TimeOnly(12, 0),
                BreakEnd = new TimeOnly(13, 0)
            })
            .ToList();
}
