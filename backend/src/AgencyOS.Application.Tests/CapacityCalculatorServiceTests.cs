using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class CapacityCalculatorServiceTests
{
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<IAssignmentRepository> _assignmentRepository = new();
    private readonly Mock<IResourceAvailabilityRepository> _resourceAvailabilityRepository = new();
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<IHolidayRepository> _holidayRepository = new();
    private readonly Mock<ICapacityHistoryService> _capacityHistoryService = new();
    private readonly Mock<ILogger<CapacityCalculatorService>> _logger = new();

    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private CapacityCalculatorService CreateService()
    {
        _capacityHistoryService
            .Setup(service => service.PersistCompletedCalculationAsync(
                It.IsAny<CapacityResponse>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _capacityHistoryService
            .Setup(service => service.PersistCompletedCalculationsAsync(
                It.IsAny<IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new CapacityCalculatorService(
            _executionResourceRepository.Object,
            _assignmentRepository.Object,
            _resourceAvailabilityRepository.Object,
            _workingCalendarRepository.Object,
            _workingHoursRepository.Object,
            _holidayRepository.Object,
            _capacityHistoryService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task GetAllAsync_CalculatesPlannedHoursFromOperationalConfiguration()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        var calendar = CreateCalendar();
        var workingHours = CreateWorkingHours(calendar.Id);
        var availability = CreateAvailability(resourceId, calendar.Id, workingHours.Id);

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<ExecutionResourceQueryParameters>(query => query.Status == ExecutionResourceStatus.Active),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource>
            {
                CreateResource(resourceId, "RES-001", 40m)
            });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Assignment>
            {
                CreateAssignment(resourceId, 10m, AssignmentStatus.Planned)
            });

        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodForResourcesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAvailability> { availability });

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(workingHours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _holidayRepository
            .Setup(repository => repository.GetActiveForCompanyInRangeAsync(
                CompanyId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Holiday>());

        var service = CreateService();
        var result = await service.GetAllAsync(parameters);

        Assert.Single(result);
        Assert.Equal(40m, result[0].TotalCapacityHours);
        Assert.Equal(5, result[0].OperationalDayCount);
        Assert.Equal(10m, result[0].AllocatedHours);
        Assert.Equal(30m, result[0].AvailableHours);
        Assert.Equal(25m, result[0].UtilizationPercentage);
        Assert.Equal(7, result[0].OperationalDays.Count);

        _capacityHistoryService.Verify(
            service => service.PersistCompletedCalculationsAsync(
                It.Is<IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)>>(
                    payload => payload.Count == 1
                        && payload[0].CompanyId == CompanyId
                        && payload[0].Capacity.ExecutionResourceId == resourceId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_DoesNotPersistHistory_WhenCalculationFails()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource> { CreateResource(resourceId, "RES-001", 40m) });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Assignment>());

        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodForResourcesAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ResourceAvailability>());

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetAllAsync(parameters));

        _capacityHistoryService.Verify(
            service => service.PersistCompletedCalculationsAsync(
                It.IsAny<IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSummaryAsync_DoesNotPersistHistory()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        var calendar = CreateCalendar();
        var workingHours = CreateWorkingHours(calendar.Id);
        var availability = CreateAvailability(resourceId, calendar.Id, workingHours.Id);

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ExecutionResource> { CreateResource(resourceId, "RES-001", 40m) });

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Assignment>());

        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodForResourcesAsync(
                It.IsAny<IReadOnlyList<Guid>>(),
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAvailability> { availability });

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(workingHours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _holidayRepository
            .Setup(repository => repository.GetActiveForCompanyInRangeAsync(
                CompanyId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Holiday>());

        var service = CreateService();
        var summary = await service.GetSummaryAsync(parameters);

        Assert.Equal(1, summary.ActiveResourceCount);
        _capacityHistoryService.Verify(
            service => service.PersistCompletedCalculationsAsync(
                It.IsAny<IReadOnlyList<(CapacityResponse Capacity, Guid CompanyId)>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _capacityHistoryService.Verify(
            service => service.PersistCompletedCalculationAsync(
                It.IsAny<CapacityResponse>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByResourceIdAsync_IgnoresHolidaysAndUnavailableDays()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 3)
        };

        var calendar = CreateCalendar();
        var workingHours = CreateWorkingHours(calendar.Id);
        var availability = CreateAvailability(
            resourceId,
            calendar.Id,
            workingHours.Id,
            overrides:
            [
                new ResourceAvailabilityDayOverrideDefinition
                {
                    OverrideDate = new DateOnly(2026, 7, 2),
                    Available = false
                }
            ]);

        var holiday = Holiday.Create(
            CompanyId,
            "Independence Day",
            description: null,
            HolidayTypes.National,
            new DateOnly(2026, 7, 1),
            stateCode: null,
            city: null,
            recurring: false,
            DateTimeOffset.UtcNow);
        holiday.Activate(DateTimeOffset.UtcNow);

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResource(resourceId, "RES-001", 40m));

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                resourceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Assignment>());

        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodAsync(
                resourceId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceAvailability> { availability });

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);

        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(workingHours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workingHours);

        _holidayRepository
            .Setup(repository => repository.GetActiveForCompanyInRangeAsync(
                CompanyId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Holiday> { holiday });

        var service = CreateService();
        var result = await service.GetByResourceIdAsync(resourceId, parameters);

        // Jul 1 holiday, Jul 2 RA unavailable, Jul 3 Friday operational
        Assert.Equal(8m, result.TotalCapacityHours);
        Assert.Equal(1, result.OperationalDayCount);
        Assert.Equal(1, result.HolidayImpactDayCount);
        Assert.Equal(1, result.ResourceAvailabilityExcludedDayCount);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsWhenOperationalConfigurationMissing()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResource(resourceId, "RES-001", 40m));

        _assignmentRepository
            .Setup(repository => repository.GetForCapacityCalculationAsync(
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                resourceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Assignment>());

        _resourceAvailabilityRepository
            .Setup(repository => repository.GetActiveCoveringPeriodAsync(
                resourceId,
                parameters.PeriodStartDate,
                parameters.PeriodEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ResourceAvailability>());

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.GetByResourceIdAsync(resourceId, parameters));

        Assert.Contains("cannot fall back to Monday–Friday", ex.Message);
    }

    [Fact]
    public async Task GetByResourceIdAsync_ThrowsNotFoundWhenResourceIsInactive()
    {
        var resourceId = Guid.NewGuid();
        var parameters = new CapacityQueryParameters
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        };

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResource(resourceId, "RES-002", 40m, ExecutionResourceStatus.Inactive));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetByResourceIdAsync(resourceId, parameters));
    }

    [Fact]
    public void CapacityQueryParametersValidator_RequiresPlanningPeriod()
    {
        var validator = new CapacityQueryParametersValidator();
        var result = validator.Validate(new CapacityQueryParameters());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CapacityQueryParameters.PeriodStartDate));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CapacityQueryParameters.PeriodEndDate));
    }

    private static WorkingCalendar CreateCalendar()
    {
        var calendar = WorkingCalendar.Create(
            CompanyId,
            "Default Calendar",
            new DateOnly(2026, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);
        return calendar;
    }

    private static WorkingHours CreateWorkingHours(Guid calendarId)
    {
        var days = WorkingDayNames.Ordered.Select(day => new WorkingHoursDayDefinition
        {
            DayOfWeek = day,
            Enabled = WorkingDayNames.DefaultWeekdays.Contains(day),
            StartTime = WorkingDayNames.DefaultWeekdays.Contains(day) ? new TimeOnly(9, 0) : null,
            EndTime = WorkingDayNames.DefaultWeekdays.Contains(day) ? new TimeOnly(18, 0) : null,
            BreakStart = WorkingDayNames.DefaultWeekdays.Contains(day) ? new TimeOnly(12, 0) : null,
            BreakEnd = WorkingDayNames.DefaultWeekdays.Contains(day) ? new TimeOnly(13, 0) : null
        }).ToList();

        var workingHours = WorkingHours.Create(
            calendarId,
            "Standard Hours",
            new DateOnly(2026, 1, 1),
            null,
            days,
            DateTimeOffset.UtcNow);
        workingHours.Activate(DateTimeOffset.UtcNow);
        return workingHours;
    }

    private static ResourceAvailability CreateAvailability(
        Guid resourceId,
        Guid calendarId,
        Guid workingHoursId,
        IReadOnlyCollection<ResourceAvailabilityDayOverrideDefinition>? overrides = null)
    {
        var weekly = WorkingDayNames.Ordered.Select(day => new ResourceAvailabilityWeekDayDefinition
        {
            DayOfWeek = day,
            Enabled = WorkingDayNames.DefaultWeekdays.Contains(day)
        }).ToList();

        var availability = ResourceAvailability.Create(
            resourceId,
            calendarId,
            workingHoursId,
            "Standard Availability",
            new DateOnly(2026, 1, 1),
            null,
            weekly,
            overrides ?? [],
            DateTimeOffset.UtcNow);
        availability.Activate(DateTimeOffset.UtcNow);
        return availability;
    }

    private static ExecutionResource CreateResource(
        Guid id,
        string code,
        decimal capacityHoursPerWeek,
        string status = ExecutionResourceStatus.Active)
    {
        return new ExecutionResource
        {
            Id = id,
            Code = code,
            Name = $"Resource {code}",
            ResourceType = "Internal Human",
            Status = status,
            CapacityHoursPerWeek = capacityHoursPerWeek,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static Assignment CreateAssignment(Guid resourceId, decimal plannedHours, string status)
    {
        return new Assignment
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            ExecutionResourceId = resourceId,
            AssignmentRole = AssignmentRole.Responsible,
            PlannedHours = plannedHours,
            PlannedStartDate = new DateOnly(2026, 7, 1),
            PlannedEndDate = new DateOnly(2026, 7, 7),
            AllocationPercentage = 100,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
