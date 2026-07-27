using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class ResourceAvailabilityServiceTests
{
    private readonly Mock<IResourceAvailabilityRepository> _availabilityRepository = new();
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IHolidayRepository> _holidayRepository = new();
    private readonly Mock<ILogger<ResourceAvailabilityService>> _logger = new();

    private ResourceAvailabilityService CreateService() =>
        new(
            _availabilityRepository.Object,
            _executionResourceRepository.Object,
            _workingCalendarRepository.Object,
            _workingHoursRepository.Object,
            _holidayRepository.Object,
            _logger.Object);

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource?)null);

        var request = CreateValidRequest();
        request.ExecutionResourceId = resourceId;

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_PersistsInactiveConfiguration()
    {
        ResourceAvailability? persisted = null;
        var calendar = CreateCalendar();
        var hours = CreateHours(calendar.Id);
        var resourceId = Guid.NewGuid();

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutionResource { Id = resourceId, Name = "Alice" });
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);
        _availabilityRepository
            .Setup(repository => repository.AddAsync(It.IsAny<ResourceAvailability>(), It.IsAny<CancellationToken>()))
            .Callback<ResourceAvailability, CancellationToken>((availability, _) => persisted = availability)
            .ReturnsAsync((ResourceAvailability availability, CancellationToken _) => availability);

        var request = CreateValidRequest();
        request.ExecutionResourceId = resourceId;
        request.WorkingCalendarId = calendar.Id;
        request.WorkingHoursId = hours.Id;

        var result = await CreateService().CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal(ResourceAvailabilityStatus.Inactive, persisted!.Status);
        Assert.Equal(result.Id, persisted.Id);
        Assert.Equal(7, result.WeeklyAvailability.Count);
    }

    [Fact]
    public async Task CreateAsync_RejectsHoursFromDifferentCalendar()
    {
        var calendar = CreateCalendar();
        var otherCalendar = CreateCalendar();
        var hours = CreateHours(otherCalendar.Id);
        var resourceId = Guid.NewGuid();

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutionResource { Id = resourceId, Name = "Alice" });
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);

        var request = CreateValidRequest();
        request.ExecutionResourceId = resourceId;
        request.WorkingCalendarId = calendar.Id;
        request.WorkingHoursId = hours.Id;

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().CreateAsync(request));
    }

    [Fact]
    public async Task ActivateAsync_ThrowsConflictWhenPeriodOverlaps()
    {
        var availability = CreateFutureAvailability();
        var overlapping = CreateFutureAvailability();
        overlapping.Activate(DateTimeOffset.UtcNow);

        _availabilityRepository
            .Setup(repository => repository.GetByIdAsync(availability.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);
        _availabilityRepository
            .Setup(repository => repository.GetActiveOverlappingAsync(
                availability.ExecutionResourceId,
                availability.EffectiveFrom,
                availability.EffectiveTo,
                availability.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overlapping });

        await Assert.ThrowsAsync<ConflictException>(() => CreateService().ActivateAsync(availability.Id));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenActive()
    {
        var availability = CreateFutureAvailability();
        availability.Activate(DateTimeOffset.UtcNow);

        _availabilityRepository
            .Setup(repository => repository.GetByIdAsync(availability.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().DeleteAsync(availability.Id));
    }

    [Fact]
    public async Task GetOperationalAvailabilityAsync_ThrowsWhenNoActiveConfig()
    {
        var resourceId = Guid.NewGuid();
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExecutionResource { Id = resourceId });
        _availabilityRepository
            .Setup(repository => repository.GetActiveCoveringDateAsync(
                resourceId,
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceAvailability?)null);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().GetOperationalAvailabilityAsync(resourceId, new DateOnly(2099, 1, 5)));
    }

    private static WorkingCalendar CreateCalendar() =>
        WorkingCalendar.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

    private static WorkingHours CreateHours(Guid calendarId) =>
        WorkingHours.Create(
            calendarId,
            "Hours",
            new DateOnly(2099, 1, 1),
            null,
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
                .ToList(),
            DateTimeOffset.UtcNow);

    private static ResourceAvailability CreateFutureAvailability() =>
        ResourceAvailability.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Availability",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            CreateWeeklyDefinitions(),
            [],
            DateTimeOffset.UtcNow);

    private static CreateResourceAvailabilityRequest CreateValidRequest() =>
        new()
        {
            ExecutionResourceId = Guid.NewGuid(),
            WorkingCalendarId = Guid.NewGuid(),
            WorkingHoursId = Guid.NewGuid(),
            Name = "Alice Standard Availability",
            EffectiveFrom = new DateOnly(2099, 1, 1),
            EffectiveTo = new DateOnly(2099, 12, 31),
            WeeklyAvailability = WorkingDayNames.Ordered
                .Select(day => new ResourceAvailabilityWeekDayRequest
                {
                    DayOfWeek = day,
                    Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
                })
                .ToList(),
            DailyOverrides = []
        };

    private static IReadOnlyList<ResourceAvailabilityWeekDayDefinition> CreateWeeklyDefinitions() =>
        WorkingDayNames.Ordered
            .Select(day => new ResourceAvailabilityWeekDayDefinition
            {
                DayOfWeek = day,
                Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
            })
            .ToList();
}
