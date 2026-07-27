using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly IWorkingHoursRepository _workingHoursRepository;
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly ILogger<WorkingHoursService> _logger;

    public WorkingHoursService(
        IWorkingHoursRepository workingHoursRepository,
        IWorkingCalendarRepository workingCalendarRepository,
        ILogger<WorkingHoursService> logger)
    {
        _workingHoursRepository = workingHoursRepository;
        _workingCalendarRepository = workingCalendarRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkingHoursResponse>> GetAllAsync(
        WorkingHoursQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var items = await _workingHoursRepository.GetAllAsync(parameters, cancellationToken);
        return items.Select(MapToResponse).ToList();
    }

    public async Task<WorkingHoursResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workingHours = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(workingHours);
    }

    public async Task<WorkingHoursResponse> CreateAsync(
        CreateWorkingHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCalendarExistsAsync(request.WorkingCalendarId, cancellationToken);

        WorkingHours workingHours;

        try
        {
            workingHours = WorkingHours.Create(
                request.WorkingCalendarId,
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                MapDayDefinitions(request.Days),
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _workingHoursRepository.AddAsync(workingHours, cancellationToken);

        _logger.LogInformation(
            "Working Hours Created: {WorkingHoursId} ({Name}) Calendar={CalendarId}",
            created.Id,
            created.Name,
            created.WorkingCalendarId);

        return MapToResponse(created);
    }

    public async Task<WorkingHoursResponse> UpdateAsync(
        Guid id,
        UpdateWorkingHoursRequest request,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await GetOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            workingHours.Reconfigure(
                request.Name,
                request.EffectiveFrom,
                request.EffectiveTo,
                MapDayDefinitions(request.Days),
                today,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (workingHours.IsActive)
        {
            await EnsureNoActiveOverlapAsync(
                workingHours.WorkingCalendarId,
                workingHours.EffectiveFrom,
                workingHours.EffectiveTo,
                workingHours.Id,
                cancellationToken);
        }

        var updated = await _workingHoursRepository.UpdateAsync(workingHours, cancellationToken);

        _logger.LogInformation(
            "Working Hours Updated: {WorkingHoursId} ({Name})",
            updated.Id,
            updated.Name);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workingHours = await GetOrThrowAsync(id, cancellationToken);

        if (workingHours.IsActive)
        {
            return;
        }

        await EnsureNoActiveOverlapAsync(
            workingHours.WorkingCalendarId,
            workingHours.EffectiveFrom,
            workingHours.EffectiveTo,
            workingHours.Id,
            cancellationToken);

        workingHours.Activate(DateTimeOffset.UtcNow);
        await _workingHoursRepository.UpdateAsync(workingHours, cancellationToken);

        _logger.LogInformation(
            "Working Hours Activated: {WorkingHoursId} ({Name})",
            workingHours.Id,
            workingHours.Name);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workingHours = await GetOrThrowAsync(id, cancellationToken);

        if (workingHours.IsInactive)
        {
            return;
        }

        workingHours.Deactivate(DateTimeOffset.UtcNow);
        await _workingHoursRepository.UpdateAsync(workingHours, cancellationToken);

        _logger.LogInformation(
            "Working Hours Deactivated: {WorkingHoursId} ({Name})",
            workingHours.Id,
            workingHours.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workingHours = await GetOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            workingHours.EnsureCanDelete(today);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _workingHoursRepository.DeleteAsync(workingHours, cancellationToken);

        _logger.LogInformation(
            "Working Hours Deleted: {WorkingHoursId} ({Name})",
            workingHours.Id,
            workingHours.Name);
    }

    private async Task EnsureCalendarExistsAsync(Guid workingCalendarId, CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarRepository.GetByIdAsync(workingCalendarId, cancellationToken);

        if (calendar is null)
        {
            throw new NotFoundException($"Working Calendar with id '{workingCalendarId}' was not found.");
        }
    }

    private async Task EnsureNoActiveOverlapAsync(
        Guid workingCalendarId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var overlapping = await _workingHoursRepository.GetActiveOverlappingAsync(
            workingCalendarId,
            effectiveFrom,
            effectiveTo,
            excludeId,
            cancellationToken);

        if (overlapping.Count > 0)
        {
            throw new ConflictException(
                "Only one active Working Hours configuration may exist for a Working Calendar during the same effective period.");
        }
    }

    private async Task<WorkingHours> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var workingHours = await _workingHoursRepository.GetByIdAsync(id, cancellationToken);

        if (workingHours is null)
        {
            throw new NotFoundException($"Working Hours with id '{id}' was not found.");
        }

        return workingHours;
    }

    private static IReadOnlyList<WorkingHoursDayDefinition> MapDayDefinitions(
        IReadOnlyList<WorkingHoursDayRequest> days) =>
        days.Select(day => new WorkingHoursDayDefinition
        {
            DayOfWeek = day.DayOfWeek,
            Enabled = day.Enabled,
            StartTime = day.StartTime,
            EndTime = day.EndTime,
            BreakStart = day.BreakStart,
            BreakEnd = day.BreakEnd
        }).ToList();

    internal static WorkingHoursResponse MapToResponse(WorkingHours workingHours)
    {
        return new WorkingHoursResponse
        {
            Id = workingHours.Id,
            WorkingCalendarId = workingHours.WorkingCalendarId,
            Name = workingHours.Name,
            Status = workingHours.Status,
            EffectiveFrom = workingHours.EffectiveFrom,
            EffectiveTo = workingHours.EffectiveTo,
            Days = workingHours.Days
                .OrderBy(day => Array.FindIndex(
                    WorkingDayNames.Ordered.ToArray(),
                    ordered => string.Equals(ordered, day.DayOfWeek, StringComparison.OrdinalIgnoreCase)))
                .Select(day => new WorkingHoursDayResponse
                {
                    DayOfWeek = day.DayOfWeek,
                    Enabled = day.Enabled,
                    StartTime = day.StartTime,
                    EndTime = day.EndTime,
                    BreakStart = day.BreakStart,
                    BreakEnd = day.BreakEnd,
                    NetHours = day.NetHours
                })
                .ToList(),
            CreatedAt = workingHours.CreatedAt,
            UpdatedAt = workingHours.UpdatedAt
        };
    }
}
