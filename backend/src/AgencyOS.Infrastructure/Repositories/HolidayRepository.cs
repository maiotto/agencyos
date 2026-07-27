using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly ApplicationDbContext _context;

    public HolidayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Holiday>> GetAllAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Holiday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Holidays
            .FirstOrDefaultAsync(holiday => holiday.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithSameScopeAsync(
        string holidayType,
        Guid? companyId,
        string? stateCode,
        string? city,
        DateOnly holidayDate,
        bool recurring,
        Guid? excludeHolidayId = null,
        CancellationToken cancellationToken = default)
    {
        var type = holidayType.Trim().ToLowerInvariant();
        var normalizedState = HolidayRules.NormalizeStateCode(stateCode);
        var normalizedCity = HolidayRules.NormalizeCity(city);

        var candidates = await _context.Holidays
            .AsNoTracking()
            .Where(holiday => holiday.HolidayType.ToLower() == type)
            .Where(holiday => holiday.Recurring == recurring)
            .Where(holiday => !excludeHolidayId.HasValue || holiday.Id != excludeHolidayId.Value)
            .ToListAsync(cancellationToken);

        return candidates.Any(holiday =>
            holiday.SameScopeAs(holidayType, companyId, normalizedState, normalizedCity, holidayDate, recurring));
    }

    public async Task<IReadOnlyList<Holiday>> GetActiveForCompanyOnDateAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = HolidayStatus.Active.ToLowerInvariant();

        var candidates = await _context.Holidays
            .AsNoTracking()
            .Where(holiday => holiday.Status.ToLower() == activeStatus)
            .Where(holiday =>
                holiday.CompanyId == null
                || holiday.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return candidates
            .Where(holiday => holiday.AffectsCompany(companyId) && holiday.OccursOn(date))
            .ToList();
    }

    public async Task<IReadOnlyList<Holiday>> GetActiveForCompanyInRangeAsync(
        Guid companyId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var activeStatus = HolidayStatus.Active.ToLowerInvariant();

        var candidates = await _context.Holidays
            .AsNoTracking()
            .Where(holiday => holiday.Status.ToLower() == activeStatus)
            .Where(holiday =>
                holiday.CompanyId == null
                || holiday.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return candidates
            .Where(holiday => holiday.AffectsCompany(companyId))
            .Where(holiday => OccursInRange(holiday, from, to))
            .OrderBy(holiday => holiday.HolidayDate)
            .ThenBy(holiday => holiday.Name)
            .ToList();
    }

    public async Task<Holiday> AddAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);
        return holiday;
    }

    public async Task<Holiday> UpdateAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        _context.Holidays.Update(holiday);
        await _context.SaveChangesAsync(cancellationToken);
        return holiday;
    }

    public async Task DeleteAsync(Holiday holiday, CancellationToken cancellationToken = default)
    {
        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Holiday> BuildFilteredQuery(HolidayQueryParameters parameters)
    {
        var query = _context.Holidays
            .AsNoTracking()
            .AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            var companyId = parameters.CompanyId.Value;
            query = query.Where(holiday =>
                holiday.CompanyId == null || holiday.CompanyId == companyId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(holiday => holiday.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.HolidayType))
        {
            var holidayType = parameters.HolidayType.Trim().ToLowerInvariant();
            query = query.Where(holiday => holiday.HolidayType.ToLower() == holidayType);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim().ToLowerInvariant();
            query = query.Where(holiday => holiday.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(parameters.StateCode))
        {
            var stateCode = parameters.StateCode.Trim().ToLowerInvariant();
            query = query.Where(holiday =>
                holiday.StateCode != null && holiday.StateCode.ToLower() == stateCode);
        }

        if (!string.IsNullOrWhiteSpace(parameters.City))
        {
            var city = parameters.City.Trim().ToLowerInvariant();
            query = query.Where(holiday =>
                holiday.City != null && holiday.City.ToLower().Contains(city));
        }

        if (parameters.Recurring.HasValue)
        {
            query = query.Where(holiday => holiday.Recurring == parameters.Recurring.Value);
        }

        if (parameters.FromDate.HasValue)
        {
            var fromDate = parameters.FromDate.Value;
            query = query.Where(holiday =>
                holiday.Recurring
                || holiday.HolidayDate >= fromDate);
        }

        if (parameters.ToDate.HasValue)
        {
            var toDate = parameters.ToDate.Value;
            query = query.Where(holiday =>
                holiday.Recurring
                || holiday.HolidayDate <= toDate);
        }

        return query;
    }

    private static IQueryable<Holiday> ApplyOrdering(
        IQueryable<Holiday> query,
        HolidayQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "name", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(holiday => holiday.Name).ThenBy(holiday => holiday.HolidayDate)
                : query.OrderBy(holiday => holiday.Name).ThenBy(holiday => holiday.HolidayDate);
        }

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(holiday => holiday.Status).ThenBy(holiday => holiday.HolidayDate)
                : query.OrderBy(holiday => holiday.Status).ThenBy(holiday => holiday.HolidayDate);
        }

        if (string.Equals(parameters.OrderBy, "holidayType", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(holiday => holiday.HolidayType).ThenBy(holiday => holiday.HolidayDate)
                : query.OrderBy(holiday => holiday.HolidayType).ThenBy(holiday => holiday.HolidayDate);
        }

        return descending
            ? query.OrderByDescending(holiday => holiday.HolidayDate).ThenBy(holiday => holiday.Name)
            : query.OrderBy(holiday => holiday.HolidayDate).ThenBy(holiday => holiday.Name);
    }

    private static bool OccursInRange(Holiday holiday, DateOnly from, DateOnly to)
    {
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (holiday.OccursOn(date))
            {
                return true;
            }
        }

        return false;
    }
}
