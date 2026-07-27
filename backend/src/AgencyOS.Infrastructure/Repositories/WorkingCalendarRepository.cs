using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class WorkingCalendarRepository : IWorkingCalendarRepository
{
    private readonly ApplicationDbContext _context;

    public WorkingCalendarRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkingCalendar>> GetAllAsync(
        WorkingCalendarQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<WorkingCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkingCalendars
            .FirstOrDefaultAsync(calendar => calendar.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkingCalendar>> GetActiveOverlappingAsync(
        Guid companyId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        Guid? excludeCalendarId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkingCalendars
            .AsNoTracking()
            .Where(calendar => calendar.CompanyId == companyId)
            .Where(calendar => calendar.Status.ToLower() == WorkingCalendarStatus.Active.ToLower());

        if (excludeCalendarId.HasValue)
        {
            query = query.Where(calendar => calendar.Id != excludeCalendarId.Value);
        }

        var candidates = await query.ToListAsync(cancellationToken);

        return candidates
            .Where(calendar => calendar.OverlapsPeriod(effectiveFrom, effectiveTo))
            .ToList();
    }

    public async Task<WorkingCalendar?> GetActiveCoveringDateAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var candidates = await _context.WorkingCalendars
            .AsNoTracking()
            .Where(calendar => calendar.CompanyId == companyId)
            .Where(calendar => calendar.Status.ToLower() == WorkingCalendarStatus.Active.ToLower())
            .Where(calendar => calendar.EffectiveFrom <= date)
            .Where(calendar => calendar.EffectiveTo == null || calendar.EffectiveTo >= date)
            .OrderByDescending(calendar => calendar.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault();
    }

    public async Task<WorkingCalendar> AddAsync(
        WorkingCalendar calendar,
        CancellationToken cancellationToken = default)
    {
        _context.WorkingCalendars.Add(calendar);
        await _context.SaveChangesAsync(cancellationToken);
        return calendar;
    }

    public async Task<WorkingCalendar> UpdateAsync(
        WorkingCalendar calendar,
        CancellationToken cancellationToken = default)
    {
        _context.WorkingCalendars.Update(calendar);
        await _context.SaveChangesAsync(cancellationToken);
        return calendar;
    }

    public async Task DeleteAsync(
        WorkingCalendar calendar,
        CancellationToken cancellationToken = default)
    {
        _context.WorkingCalendars.Remove(calendar);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<WorkingCalendar> BuildFilteredQuery(WorkingCalendarQueryParameters parameters)
    {
        var query = _context.WorkingCalendars
            .AsNoTracking()
            .AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(calendar => calendar.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(calendar => calendar.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim().ToLowerInvariant();
            query = query.Where(calendar => calendar.Name.ToLower().Contains(name));
        }

        return query;
    }

    private static IQueryable<WorkingCalendar> ApplyOrdering(
        IQueryable<WorkingCalendar> query,
        WorkingCalendarQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(calendar => calendar.Status).ThenBy(calendar => calendar.Name)
                : query.OrderBy(calendar => calendar.Status).ThenBy(calendar => calendar.Name);
        }

        if (string.Equals(parameters.OrderBy, "effectiveFrom", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(calendar => calendar.EffectiveFrom).ThenBy(calendar => calendar.Name)
                : query.OrderBy(calendar => calendar.EffectiveFrom).ThenBy(calendar => calendar.Name);
        }

        return descending
            ? query.OrderByDescending(calendar => calendar.Name).ThenBy(calendar => calendar.EffectiveFrom)
            : query.OrderBy(calendar => calendar.Name).ThenBy(calendar => calendar.EffectiveFrom);
    }
}
