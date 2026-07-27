using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class WorkingHoursRepository : IWorkingHoursRepository
{
    private readonly ApplicationDbContext _context;

    public WorkingHoursRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkingHours>> GetAllAsync(
        WorkingHoursQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<WorkingHours?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkingHours
            .Include(hours => hours.Days)
            .FirstOrDefaultAsync(hours => hours.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkingHours>> GetActiveOverlappingAsync(
        Guid workingCalendarId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeWorkingHoursId = null,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = WorkingHoursStatus.Active.ToLowerInvariant();

        var query = _context.WorkingHours
            .AsNoTracking()
            .Include(hours => hours.Days)
            .Where(hours => hours.WorkingCalendarId == workingCalendarId)
            .Where(hours => hours.Status.ToLower() == activeStatus);

        if (excludeWorkingHoursId.HasValue)
        {
            query = query.Where(hours => hours.Id != excludeWorkingHoursId.Value);
        }

        var candidates = await query.ToListAsync(cancellationToken);

        return candidates
            .Where(hours => hours.OverlapsPeriod(effectiveFrom, effectiveTo))
            .ToList();
    }

    public async Task<WorkingHours?> GetActiveCoveringDateAsync(
        Guid workingCalendarId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = WorkingHoursStatus.Active.ToLowerInvariant();

        var candidates = await _context.WorkingHours
            .AsNoTracking()
            .Include(hours => hours.Days)
            .Where(hours => hours.WorkingCalendarId == workingCalendarId)
            .Where(hours => hours.Status.ToLower() == activeStatus)
            .Where(hours => hours.EffectiveFrom <= date)
            .Where(hours => hours.EffectiveTo == null || hours.EffectiveTo >= date)
            .OrderByDescending(hours => hours.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault();
    }

    public async Task<WorkingHours> AddAsync(
        WorkingHours workingHours,
        CancellationToken cancellationToken = default)
    {
        _context.WorkingHours.Add(workingHours);
        await _context.SaveChangesAsync(cancellationToken);
        return workingHours;
    }

    public async Task<WorkingHours> UpdateAsync(
        WorkingHours workingHours,
        CancellationToken cancellationToken = default)
    {
        var trackedDays = _context.ChangeTracker
            .Entries<WorkingHoursDay>()
            .Where(entry => entry.Entity.WorkingHoursId == workingHours.Id)
            .Select(entry => entry.Entity)
            .ToList();

        if (trackedDays.Count == 0)
        {
            trackedDays = await _context.WorkingHoursDays
                .Where(day => day.WorkingHoursId == workingHours.Id)
                .ToListAsync(cancellationToken);
        }

        var currentDayIds = workingHours.Days.Select(day => day.Id).ToHashSet();
        var obsoleteDays = trackedDays.Where(day => !currentDayIds.Contains(day.Id)).ToList();

        if (obsoleteDays.Count > 0)
        {
            _context.WorkingHoursDays.RemoveRange(obsoleteDays);
        }

        foreach (var day in workingHours.Days)
        {
            var entry = _context.Entry(day);
            if (entry.State == EntityState.Detached)
            {
                _context.WorkingHoursDays.Add(day);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return workingHours;
    }

    public async Task DeleteAsync(
        WorkingHours workingHours,
        CancellationToken cancellationToken = default)
    {
        _context.WorkingHours.Remove(workingHours);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkingHours> BuildFilteredQuery(WorkingHoursQueryParameters parameters)
    {
        var query = _context.WorkingHours
            .AsNoTracking()
            .Include(hours => hours.Days)
            .AsQueryable();

        if (parameters.WorkingCalendarId.HasValue)
        {
            query = query.Where(hours => hours.WorkingCalendarId == parameters.WorkingCalendarId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(hours => hours.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim().ToLowerInvariant();
            query = query.Where(hours => hours.Name.ToLower().Contains(name));
        }

        return query;
    }

    private static IQueryable<WorkingHours> ApplyOrdering(
        IQueryable<WorkingHours> query,
        WorkingHoursQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(hours => hours.Status).ThenBy(hours => hours.Name)
                : query.OrderBy(hours => hours.Status).ThenBy(hours => hours.Name);
        }

        if (string.Equals(parameters.OrderBy, "effectiveFrom", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(hours => hours.EffectiveFrom).ThenBy(hours => hours.Name)
                : query.OrderBy(hours => hours.EffectiveFrom).ThenBy(hours => hours.Name);
        }

        return descending
            ? query.OrderByDescending(hours => hours.Name).ThenBy(hours => hours.EffectiveFrom)
            : query.OrderBy(hours => hours.Name).ThenBy(hours => hours.EffectiveFrom);
    }
}
