using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class PlanningTemplateServiceTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    private readonly Mock<IPlanningTemplateRepository> _templateRepository = new();
    private readonly Mock<IWorkingCalendarRepository> _workingCalendarRepository = new();
    private readonly Mock<IWorkingHoursRepository> _workingHoursRepository = new();
    private readonly Mock<ICapacityCalculatorService> _capacityCalculatorService = new();
    private readonly Mock<IWorkloadCalculatorService> _workloadCalculatorService = new();
    private readonly Mock<ILogger<PlanningTemplateService>> _logger = new();

    private PlanningTemplateService CreateService() =>
        new(
            _templateRepository.Object,
            _workingCalendarRepository.Object,
            _workingHoursRepository.Object,
            _capacityCalculatorService.Object,
            _workloadCalculatorService.Object,
            new AgencyOS.Application.Audit.NoOpAuditService(),
            _logger.Object);

    [Fact]
    public async Task CreateAsync_PersistsInactiveTemplate()
    {
        var calendar = CreateActiveCalendar();
        var hours = CreateActiveHours(calendar.Id);
        PlanningTemplate? persisted = null;

        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);
        _templateRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                CompanyId,
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _templateRepository
            .Setup(repository => repository.AddAsync(It.IsAny<PlanningTemplate>(), It.IsAny<CancellationToken>()))
            .Callback<PlanningTemplate, CancellationToken>((template, _) => persisted = template)
            .ReturnsAsync((PlanningTemplate template, CancellationToken _) => template);

        var result = await CreateService().CreateAsync(new CreatePlanningTemplateRequest
        {
            CompanyId = CompanyId,
            Name = "Standard Weekly",
            WorkingCalendarId = calendar.Id,
            WorkingHoursId = hours.Id,
            ResourceAvailabilityStrategy = ResourceAvailabilityStrategies.RequireActiveConfiguration,
            DefaultPlanningWindowDays = 7
        });

        Assert.NotNull(persisted);
        Assert.Equal(PlanningTemplateStatus.Inactive, persisted!.Status);
        Assert.Equal(result.Id, persisted.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenNameExists()
    {
        _templateRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                CompanyId,
                "Standard Weekly",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateService().CreateAsync(new CreatePlanningTemplateRequest
            {
                CompanyId = CompanyId,
                Name = "Standard Weekly",
                WorkingCalendarId = Guid.NewGuid(),
                WorkingHoursId = Guid.NewGuid(),
                DefaultPlanningWindowDays = 7
            }));
    }

    [Fact]
    public async Task ActivateAsync_RequiresActiveCalendarAndHours()
    {
        var calendar = CreateInactiveCalendar();
        var hours = CreateActiveHours(calendar.Id);
        var template = PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);

        _templateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().ActivateAsync(template.Id));
    }

    [Fact]
    public async Task DeleteAsync_RejectsActiveTemplate()
    {
        var template = CreateTemplate();
        template.Activate(DateTimeOffset.UtcNow);

        _templateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().DeleteAsync(template.Id));
    }

    [Fact]
    public async Task CloneAsync_CreatesNewInactiveTemplate()
    {
        var source = CreateTemplate();
        source.Activate(DateTimeOffset.UtcNow);
        PlanningTemplate? cloned = null;

        _templateRepository
            .Setup(repository => repository.GetByIdAsync(source.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);
        _templateRepository
            .Setup(repository => repository.ExistsByCompanyAndNameAsync(
                CompanyId,
                "Cloned Template",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _templateRepository
            .Setup(repository => repository.AddAsync(It.IsAny<PlanningTemplate>(), It.IsAny<CancellationToken>()))
            .Callback<PlanningTemplate, CancellationToken>((template, _) => cloned = template)
            .ReturnsAsync((PlanningTemplate template, CancellationToken _) => template);

        var result = await CreateService().CloneAsync(source.Id, new ClonePlanningTemplateRequest
        {
            Name = "Cloned Template"
        });

        Assert.NotNull(cloned);
        Assert.NotEqual(source.Id, cloned!.Id);
        Assert.Equal(PlanningTemplateStatus.Inactive, cloned.Status);
        Assert.Equal("Cloned Template", result.Name);
        Assert.Equal(source.WorkingCalendarId, cloned.WorkingCalendarId);
    }

    [Fact]
    public async Task ApplyAsync_RejectsInactiveTemplate()
    {
        var template = CreateTemplate();
        _templateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateService().ApplyAsync(template.Id, new ApplyPlanningTemplateRequest()));
    }

    [Fact]
    public async Task ApplyAsync_ProducesConfigurationWithoutMutatingTemplate()
    {
        var calendar = CreateActiveCalendar();
        var hours = CreateActiveHours(calendar.Id);
        var template = PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            new PlanningTemplateCapacityRules(80m, true),
            null,
            DateTimeOffset.UtcNow);
        template.Activate(DateTimeOffset.UtcNow);
        var originalUpdatedAt = template.UpdatedAt;

        _templateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);

        var applied = await CreateService().ApplyAsync(template.Id, new ApplyPlanningTemplateRequest
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7)
        });

        Assert.Equal(template.Id, applied.SourceTemplateId);
        Assert.Equal(calendar.Id, applied.WorkingCalendarId);
        Assert.Equal(hours.Id, applied.WorkingHoursId);
        Assert.Equal(new DateOnly(2026, 7, 1), applied.PeriodStartDate);
        Assert.Equal(new DateOnly(2026, 7, 7), applied.PeriodEndDate);
        Assert.Equal(originalUpdatedAt, template.UpdatedAt);
        Assert.Null(applied.CapacityResults);
        _templateRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<PlanningTemplate>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyAsync_CanCalculateCapacity()
    {
        var calendar = CreateActiveCalendar();
        var hours = CreateActiveHours(calendar.Id);
        var template = PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);
        template.Activate(DateTimeOffset.UtcNow);

        _templateRepository
            .Setup(repository => repository.GetByIdAsync(template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _workingCalendarRepository
            .Setup(repository => repository.GetByIdAsync(calendar.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calendar);
        _workingHoursRepository
            .Setup(repository => repository.GetByIdAsync(hours.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hours);
        _capacityCalculatorService
            .Setup(service => service.GetAllAsync(It.IsAny<CapacityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CapacityResponse>());
        _capacityCalculatorService
            .Setup(service => service.GetSummaryAsync(It.IsAny<CapacityQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CapacitySummaryResponse
            {
                PeriodStartDate = new DateOnly(2026, 7, 1),
                PeriodEndDate = new DateOnly(2026, 7, 7),
                ActiveResourceCount = 0
            });

        var applied = await CreateService().ApplyAsync(template.Id, new ApplyPlanningTemplateRequest
        {
            PeriodStartDate = new DateOnly(2026, 7, 1),
            PeriodEndDate = new DateOnly(2026, 7, 7),
            CalculateCapacity = true
        });

        Assert.NotNull(applied.CapacityResults);
        Assert.NotNull(applied.CapacitySummary);
    }

    [Fact]
    public void Validators_RejectInvalidWindowAndBlankCloneName()
    {
        var createValidator = new CreatePlanningTemplateRequestValidator();
        var createResult = createValidator.Validate(new CreatePlanningTemplateRequest
        {
            CompanyId = CompanyId,
            Name = "X",
            WorkingCalendarId = Guid.NewGuid(),
            WorkingHoursId = Guid.NewGuid(),
            DefaultPlanningWindowDays = 0
        });
        Assert.False(createResult.IsValid);

        var cloneValidator = new ClonePlanningTemplateRequestValidator();
        var cloneResult = cloneValidator.Validate(new ClonePlanningTemplateRequest { Name = " " });
        Assert.False(cloneResult.IsValid);
    }

    private PlanningTemplate CreateTemplate()
    {
        var calendar = CreateActiveCalendar();
        var hours = CreateActiveHours(calendar.Id);
        return PlanningTemplate.Create(
            CompanyId,
            "Standard Weekly",
            null,
            calendar.Id,
            hours.Id,
            ResourceAvailabilityStrategies.RequireActiveConfiguration,
            7,
            0,
            PlanningTemplateCapacityRules.Default,
            null,
            DateTimeOffset.UtcNow);
    }

    private static WorkingCalendar CreateActiveCalendar()
    {
        var calendar = WorkingCalendar.Create(
            CompanyId,
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);
        calendar.Activate(DateTimeOffset.UtcNow);
        return calendar;
    }

    private static WorkingCalendar CreateInactiveCalendar() =>
        WorkingCalendar.Create(
            CompanyId,
            "Calendar",
            new DateOnly(2099, 1, 1),
            null,
            WorkingDayNames.DefaultWeekdays,
            DateTimeOffset.UtcNow);

    private static WorkingHours CreateActiveHours(Guid calendarId)
    {
        var hours = WorkingHours.Create(
            calendarId,
            "Hours",
            new DateOnly(2099, 1, 1),
            null,
            CreateWeekdayDefinitions(),
            DateTimeOffset.UtcNow);
        hours.Activate(DateTimeOffset.UtcNow);
        return hours;
    }

    private static List<WorkingHoursDayDefinition> CreateWeekdayDefinitions() =>
        WorkingDayNames.All.Select(day => new WorkingHoursDayDefinition
        {
            DayOfWeek = day,
            Enabled = WorkingDayNames.DefaultWeekdays.Contains(day),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(18, 0),
            BreakStart = new TimeOnly(12, 0),
            BreakEnd = new TimeOnly(13, 0)
        }).ToList();
}
