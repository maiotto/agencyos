using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class WorkingCalendarServiceTests
{
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<IHolidayRepository> _holidayRepository = new();
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<ILogger<WorkingCalendarService>> _logger = new();

    private WorkingCalendarService CreateService() =>
        new(
            _workingCalendarRepository.Object,
            _holidayRepository.Object,
            _workingHoursRepository.Object,
            _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenCalendarMissing()
    {
        var calendarId = Guid.NewGuid();
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendarId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkingCalendar?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(calendarId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedCalendars()
    {
        var calendar = CreateFutureCalendar();

        _workingCalendarRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<WorkingCalendarQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { calendar });

        var service = CreateService();
        var result = await service.GetAllAsync(new WorkingCalendarQueryParameters());

        Assert.Single(result);
        Assert.Equal(calendar.Name, result[0].Name);
        Assert.Equal(calendar.WorkingDays, result[0].WorkingDays);
    }

    [Fact]
    public async Task CreateAsync_PersistsInactiveCalendar()
    {
        WorkingCalendar? persisted = null;

        _workingCalendarRepository
            .Setup(repository => repository.AddAsync(It.IsAny<WorkingCalendar>(), It.IsAny<CancellationToken>()))
            .Callback<WorkingCalendar, CancellationToken>((calendar, _) => persisted = calendar)
            .ReturnsAsync((WorkingCalendar calendar, CancellationToken _) => calendar);

        var service = CreateService();
        var result = await service.CreateAsync(CreateValidCreateRequest());

        Assert.NotNull(persisted);
        Assert.Equal(WorkingCalendarStatus.Inactive, persisted!.Status);
        Assert.Equal("Standard Agency Week", persisted.Name);
        Assert.Equal(WorkingDayNames.DefaultWeekdays, persisted.WorkingDays);
        Assert.Equal(persisted.Id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBusinessRuleWhenWorkingDaysMissing()
    {
        var service = CreateService();
        var request = CreateValidCreateRequest();
        request.WorkingDays = [];

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFutureInactiveCalendar()
    {
        var calendar = CreateFutureCalendar();

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingCalendarRepository
            .Setup(repository => repository.UpdateAsync(calendar, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();
        var result = await service.UpdateAsync(calendar.Id, new UpdateWorkingCalendarRequest
        {
            Name = "Updated Week",
            EffectiveFrom = new DateOnly(2026, 9, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            WorkingDays = ["Monday", "Tuesday", "Wednesday"]
        });

        Assert.Equal("Updated Week", result.Name);
        Assert.Equal(new DateOnly(2026, 9, 1), result.EffectiveFrom);
        Assert.Equal(new[] { "Monday", "Tuesday", "Wednesday" }, result.WorkingDays);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBusinessRuleWhenCalendarIsHistorical()
    {
        var calendar = WorkingCalendar.Create(
            Guid.NewGuid(),
            "Historical",
            new DateOnly(2020, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(calendar.Id, new UpdateWorkingCalendarRequest
            {
                Name = "Changed",
                EffectiveFrom = new DateOnly(2020, 2, 1),
                WorkingDays = WorkingDayNames.DefaultWeekdays
            }));
    }

    [Fact]
    public async Task ActivateAsync_ActivatesWhenNoOverlap()
    {
        var calendar = CreateFutureCalendar();

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingCalendarRepository
            .Setup(repository => repository.GetActiveOverlappingAsync(
                calendar.CompanyId,
                calendar.EffectiveFrom,
                calendar.EffectiveTo,
                calendar.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<WorkingCalendar>());
        _workingCalendarRepository
            .Setup(repository => repository.UpdateAsync(calendar, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();
        await service.ActivateAsync(calendar.Id);

        Assert.Equal(WorkingCalendarStatus.Active, calendar.Status);
    }

    [Fact]
    public async Task ActivateAsync_ThrowsConflictWhenPeriodOverlaps()
    {
        var calendar = CreateFutureCalendar();
        var overlapping = CreateFutureCalendar();
        overlapping.Activate(DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingCalendarRepository
            .Setup(repository => repository.GetActiveOverlappingAsync(
                calendar.CompanyId,
                calendar.EffectiveFrom,
                calendar.EffectiveTo,
                calendar.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overlapping });

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() => service.ActivateAsync(calendar.Id));
    }

    [Fact]
    public async Task ActivateAsync_IsIdempotentWhenAlreadyActive()
    {
        var calendar = CreateFutureCalendar();
        calendar.Activate(DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();
        await service.ActivateAsync(calendar.Id);

        _workingCalendarRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<WorkingCalendar>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_SetsInactive()
    {
        var calendar = CreateFutureCalendar();
        calendar.Activate(DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingCalendarRepository
            .Setup(repository => repository.UpdateAsync(calendar, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();
        await service.DeactivateAsync(calendar.Id);

        Assert.Equal(WorkingCalendarStatus.Inactive, calendar.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesInactiveFutureCalendar()
    {
        var calendar = CreateFutureCalendar();

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingCalendarRepository
            .Setup(repository => repository.DeleteAsync(calendar, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();
        await service.DeleteAsync(calendar.Id);

        _workingCalendarRepository.Verify(
            repository => repository.DeleteAsync(calendar, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenCalendarIsActive()
    {
        var calendar = CreateFutureCalendar();
        calendar.Activate(DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(calendar.Id));
    }

    [Fact]
    public async Task GetActiveForCompanyAsync_ReturnsMappedCalendar()
    {
        var companyId = Guid.NewGuid();
        var calendar = WorkingCalendar.Create(
            companyId,
            "Active",
            new DateOnly(2026, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);

        _workingCalendarRepository
            .Setup(repository => repository.GetActiveCoveringDateAsync(
                companyId,
                new DateOnly(2026, 7, 26),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        var service = CreateService();
        var result = await service.GetActiveForCompanyAsync(companyId, new DateOnly(2026, 7, 26));

        Assert.NotNull(result);
        Assert.Equal(calendar.Id, result!.Id);
        Assert.Equal(WorkingCalendarStatus.Active, result.Status);
    }

    private static CreateWorkingCalendarRequest CreateValidCreateRequest()
    {
        return new CreateWorkingCalendarRequest
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Standard Agency Week",
            EffectiveFrom = new DateOnly(2026, 8, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            WorkingDays = WorkingDayNames.DefaultWeekdays
        };
    }

    private static WorkingCalendar CreateFutureCalendar()
    {
        return WorkingCalendar.Create(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            "Standard Agency Week",
            new DateOnly(2099, 1, 1),
            new DateOnly(2099, 12, 31),
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
    }
}
