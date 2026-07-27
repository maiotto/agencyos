using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly ApplicationDbContext _context;

    public PortfolioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Portfolio>> GetAllAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.Include(portfolio => portfolio.Missions).ToListAsync(cancellationToken);
    }

    public async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Portfolios
            .Include(portfolio => portfolio.Missions)
            .FirstOrDefaultAsync(portfolio => portfolio.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCompanyAndNameAsync(
        Guid companyId,
        string name,
        Guid? excludePortfolioId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var query = _context.Portfolios.AsNoTracking()
            .Where(portfolio =>
                portfolio.CompanyId == companyId
                && portfolio.Name.ToLower() == normalized);

        if (excludePortfolioId.HasValue)
        {
            query = query.Where(portfolio => portfolio.Id != excludePortfolioId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Portfolio> AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync(cancellationToken);
        return portfolio;
    }

    public async Task<Portfolio> UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        var existingMissionIds = await _context.PortfolioMissions
            .AsNoTracking()
            .Where(mission => mission.PortfolioId == portfolio.Id)
            .Select(mission => mission.Id)
            .ToListAsync(cancellationToken);
        var existingSet = existingMissionIds.ToHashSet();
        var currentIds = portfolio.Missions.Select(mission => mission.Id).ToHashSet();

        var obsolete = existingSet.Except(currentIds).ToList();
        if (obsolete.Count > 0)
        {
            var trackedObsolete = await _context.PortfolioMissions
                .Where(mission => obsolete.Contains(mission.Id))
                .ToListAsync(cancellationToken);
            _context.PortfolioMissions.RemoveRange(trackedObsolete);
        }

        var entry = _context.Entry(portfolio);
        if (entry.State == EntityState.Detached)
        {
            _context.Portfolios.Attach(portfolio);
            entry.State = EntityState.Modified;
        }

        foreach (var mission in portfolio.Missions)
        {
            if (existingSet.Contains(mission.Id))
            {
                continue;
            }

            var missionEntry = _context.Entry(mission);
            if (missionEntry.State == EntityState.Detached)
            {
                _context.PortfolioMissions.Add(mission);
            }
            else
            {
                missionEntry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return portfolio;
    }

    public async Task DeleteAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        _context.Portfolios.Remove(portfolio);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Portfolio> BuildFilteredQuery(PortfolioQueryParameters parameters)
    {
        var query = _context.Portfolios.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(portfolio => portfolio.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(portfolio => portfolio.Status.ToLower() == status);
        }

        if (parameters.PlanningTemplateId.HasValue)
        {
            query = query.Where(portfolio =>
                portfolio.PlanningTemplateId == parameters.PlanningTemplateId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            var missionId = parameters.MissionId.Value;
            query = query.Where(portfolio =>
                portfolio.Missions.Any(mission => mission.MissionId == missionId));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(portfolio =>
                portfolio.Name.ToLower().Contains(search)
                || (portfolio.Description != null && portfolio.Description.ToLower().Contains(search)));
        }

        if (parameters.PeriodStart.HasValue)
        {
            query = query.Where(portfolio => portfolio.PlanningPeriodEnd >= parameters.PeriodStart.Value);
        }

        if (parameters.PeriodEnd.HasValue)
        {
            query = query.Where(portfolio => portfolio.PlanningPeriodStart <= parameters.PeriodEnd.Value);
        }

        return query;
    }

    private static IQueryable<Portfolio> ApplyOrdering(
        IQueryable<Portfolio> query,
        PortfolioQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var orderBy = parameters.OrderBy?.Trim().ToLowerInvariant();

        return orderBy switch
        {
            "name" => descending
                ? query.OrderByDescending(portfolio => portfolio.Name)
                : query.OrderBy(portfolio => portfolio.Name),
            "status" => descending
                ? query.OrderByDescending(portfolio => portfolio.Status)
                : query.OrderBy(portfolio => portfolio.Status),
            "health" or "portfoliohealth" => descending
                ? query.OrderByDescending(portfolio => portfolio.PortfolioHealth)
                : query.OrderBy(portfolio => portfolio.PortfolioHealth),
            "periodstart" => descending
                ? query.OrderByDescending(portfolio => portfolio.PlanningPeriodStart)
                : query.OrderBy(portfolio => portfolio.PlanningPeriodStart),
            _ => descending
                ? query.OrderByDescending(portfolio => portfolio.UpdatedAt)
                : query.OrderBy(portfolio => portfolio.UpdatedAt)
        };
    }
}
