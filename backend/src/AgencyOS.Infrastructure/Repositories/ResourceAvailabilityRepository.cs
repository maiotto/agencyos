using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ResourceAvailabilityRepository : IResourceAvailabilityRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceAvailabilityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ResourceAvailability>> GetAllAsync(
        ResourceAvailabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ResourceAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAvailabilities
            .Include(availability => availability.WeeklyAvailability)
            .Include(availability => availability.DailyOverrides)
            .FirstOrDefaultAsync(availability => availability.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceAvailability>> GetActiveOverlappingAsync(
        Guid executionResourceId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeResourceAvailabilityId = null,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = ResourceAvailabilityStatus.Active.ToLowerInvariant();

        var query = _context.ResourceAvailabilities
            .AsNoTracking()
            .Include(availability => availability.WeeklyAvailability)
            .Include(availability => availability.DailyOverrides)
            .Where(availability => availability.ExecutionResourceId == executionResourceId)
            .Where(availability => availability.Status.ToLower() == activeStatus);

        if (excludeResourceAvailabilityId.HasValue)
        {
            query = query.Where(availability => availability.Id != excludeResourceAvailabilityId.Value);
        }

        var candidates = await query.ToListAsync(cancellationToken);

        return candidates
            .Where(availability => availability.OverlapsPeriod(effectiveFrom, effectiveTo))
            .ToList();
    }

    public async Task<ResourceAvailability?> GetActiveCoveringDateAsync(
        Guid executionResourceId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = ResourceAvailabilityStatus.Active.ToLowerInvariant();

        var candidates = await _context.ResourceAvailabilities
            .AsNoTracking()
            .Include(availability => availability.WeeklyAvailability)
            .Include(availability => availability.DailyOverrides)
            .Where(availability => availability.ExecutionResourceId == executionResourceId)
            .Where(availability => availability.Status.ToLower() == activeStatus)
            .Where(availability => availability.EffectiveFrom <= date)
            .Where(availability => availability.EffectiveTo == null || availability.EffectiveTo >= date)
            .OrderByDescending(availability => availability.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault();
    }

    public async Task<IReadOnlyList<ResourceAvailability>> GetActiveCoveringPeriodAsync(
        Guid executionResourceId,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken = default)
    {
        return await GetActiveCoveringPeriodForResourcesAsync(
            [executionResourceId],
            periodStartDate,
            periodEndDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ResourceAvailability>> GetActiveCoveringPeriodForResourcesAsync(
        IReadOnlyCollection<Guid> executionResourceIds,
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        CancellationToken cancellationToken = default)
    {
        if (executionResourceIds.Count == 0)
        {
            return [];
        }

        var activeStatus = ResourceAvailabilityStatus.Active.ToLowerInvariant();
        var resourceIds = executionResourceIds.ToHashSet();

        var candidates = await _context.ResourceAvailabilities
            .AsNoTracking()
            .Include(availability => availability.WeeklyAvailability)
            .Include(availability => availability.DailyOverrides)
            .Where(availability => resourceIds.Contains(availability.ExecutionResourceId))
            .Where(availability => availability.Status.ToLower() == activeStatus)
            .Where(availability => availability.EffectiveFrom <= periodEndDate)
            .Where(availability => availability.EffectiveTo == null || availability.EffectiveTo >= periodStartDate)
            .OrderBy(availability => availability.ExecutionResourceId)
            .ThenByDescending(availability => availability.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return candidates;
    }

    public async Task<ResourceAvailability> AddAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default)
    {
        _context.ResourceAvailabilities.Add(resourceAvailability);
        await _context.SaveChangesAsync(cancellationToken);
        return resourceAvailability;
    }

    public async Task<ResourceAvailability> UpdateAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default)
    {
        await SyncChildrenAsync(resourceAvailability, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return resourceAvailability;
    }

    public async Task DeleteAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken = default)
    {
        _context.ResourceAvailabilities.Remove(resourceAvailability);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncChildrenAsync(
        ResourceAvailability resourceAvailability,
        CancellationToken cancellationToken)
    {
        var trackedWeekDays = _context.ChangeTracker
            .Entries<ResourceAvailabilityWeekDay>()
            .Where(entry => entry.Entity.ResourceAvailabilityId == resourceAvailability.Id)
            .Select(entry => entry.Entity)
            .ToList();

        if (trackedWeekDays.Count == 0)
        {
            trackedWeekDays = await _context.ResourceAvailabilityWeekDays
                .Where(day => day.ResourceAvailabilityId == resourceAvailability.Id)
                .ToListAsync(cancellationToken);
        }

        var currentWeekDayIds = resourceAvailability.WeeklyAvailability.Select(day => day.Id).ToHashSet();
        var obsoleteWeekDays = trackedWeekDays.Where(day => !currentWeekDayIds.Contains(day.Id)).ToList();

        if (obsoleteWeekDays.Count > 0)
        {
            _context.ResourceAvailabilityWeekDays.RemoveRange(obsoleteWeekDays);
        }

        foreach (var day in resourceAvailability.WeeklyAvailability)
        {
            var entry = _context.Entry(day);
            if (entry.State == EntityState.Detached)
            {
                _context.ResourceAvailabilityWeekDays.Add(day);
            }
        }

        var trackedOverrides = _context.ChangeTracker
            .Entries<ResourceAvailabilityDayOverride>()
            .Where(entry => entry.Entity.ResourceAvailabilityId == resourceAvailability.Id)
            .Select(entry => entry.Entity)
            .ToList();

        if (trackedOverrides.Count == 0)
        {
            trackedOverrides = await _context.ResourceAvailabilityDayOverrides
                .Where(day => day.ResourceAvailabilityId == resourceAvailability.Id)
                .ToListAsync(cancellationToken);
        }

        var currentOverrideIds = resourceAvailability.DailyOverrides.Select(day => day.Id).ToHashSet();
        var obsoleteOverrides = trackedOverrides.Where(day => !currentOverrideIds.Contains(day.Id)).ToList();

        if (obsoleteOverrides.Count > 0)
        {
            _context.ResourceAvailabilityDayOverrides.RemoveRange(obsoleteOverrides);
        }

        foreach (var day in resourceAvailability.DailyOverrides)
        {
            var entry = _context.Entry(day);
            if (entry.State == EntityState.Detached)
            {
                _context.ResourceAvailabilityDayOverrides.Add(day);
            }
        }
    }

    private IQueryable<ResourceAvailability> BuildFilteredQuery(ResourceAvailabilityQueryParameters parameters)
    {
        var query = _context.ResourceAvailabilities
            .AsNoTracking()
            .Include(availability => availability.WeeklyAvailability)
            .Include(availability => availability.DailyOverrides)
            .AsQueryable();

        if (parameters.ExecutionResourceId.HasValue)
        {
            query = query.Where(availability =>
                availability.ExecutionResourceId == parameters.ExecutionResourceId.Value);
        }

        if (parameters.WorkingCalendarId.HasValue)
        {
            query = query.Where(availability =>
                availability.WorkingCalendarId == parameters.WorkingCalendarId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(availability => availability.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim().ToLowerInvariant();
            query = query.Where(availability => availability.Name.ToLower().Contains(name));
        }

        return query;
    }

    private static IQueryable<ResourceAvailability> ApplyOrdering(
        IQueryable<ResourceAvailability> query,
        ResourceAvailabilityQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(availability => availability.Status).ThenBy(availability => availability.Name)
                : query.OrderBy(availability => availability.Status).ThenBy(availability => availability.Name);
        }

        if (string.Equals(parameters.OrderBy, "effectiveFrom", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(availability => availability.EffectiveFrom).ThenBy(availability => availability.Name)
                : query.OrderBy(availability => availability.EffectiveFrom).ThenBy(availability => availability.Name);
        }

        return descending
            ? query.OrderByDescending(availability => availability.Name).ThenBy(availability => availability.EffectiveFrom)
            : query.OrderBy(availability => availability.Name).ThenBy(availability => availability.EffectiveFrom);
    }
}
