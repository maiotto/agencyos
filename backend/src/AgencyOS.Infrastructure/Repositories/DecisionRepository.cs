using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class DecisionRepository : IDecisionRepository
{
    private readonly ApplicationDbContext _context;

    public DecisionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Decision>> GetAllAsync(
        DecisionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.Include(decision => decision.Timeline).ToListAsync(cancellationToken);
    }

    public async Task<Decision?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Decisions
            .Include(decision => decision.Timeline)
            .FirstOrDefaultAsync(decision => decision.Id == id, cancellationToken);
    }

    public async Task<Decision?> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Decisions
            .Include(decision => decision.Timeline)
            .FirstOrDefaultAsync(decision => decision.RecommendationId == recommendationId, cancellationToken);
    }

    public Task<bool> ExistsByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default) =>
        _context.Decisions.AnyAsync(
            decision => decision.RecommendationId == recommendationId,
            cancellationToken);

    public async Task<Decision> AddAsync(Decision decision, CancellationToken cancellationToken = default)
    {
        _context.Decisions.Add(decision);
        await _context.SaveChangesAsync(cancellationToken);
        return decision;
    }

    public async Task<Decision> UpdateAsync(Decision decision, CancellationToken cancellationToken = default)
    {
        var existingTimelineIds = await _context.DecisionTimelineEntries
            .AsNoTracking()
            .Where(entry => entry.DecisionId == decision.Id)
            .Select(entry => entry.Id)
            .ToListAsync(cancellationToken);
        var existingSet = existingTimelineIds.ToHashSet();

        var entry = _context.Entry(decision);
        if (entry.State == EntityState.Detached)
        {
            _context.Decisions.Attach(decision);
            entry.State = EntityState.Modified;
        }

        foreach (var timelineEntry in decision.Timeline)
        {
            if (existingSet.Contains(timelineEntry.Id))
            {
                continue;
            }

            var timelineEfEntry = _context.Entry(timelineEntry);
            if (timelineEfEntry.State == EntityState.Detached)
            {
                _context.DecisionTimelineEntries.Add(timelineEntry);
            }
            else
            {
                timelineEfEntry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return decision;
    }

    private IQueryable<Decision> BuildFilteredQuery(DecisionQueryParameters parameters)
    {
        var query = _context.Decisions.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(decision => decision.CompanyId == parameters.CompanyId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            query = query.Where(decision => decision.MissionId == parameters.MissionId.Value);
        }

        if (parameters.ContractId.HasValue)
        {
            query = query.Where(decision => decision.ContractId == parameters.ContractId.Value);
        }

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(decision => decision.RecommendationId == parameters.RecommendationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.DecisionStatus))
        {
            var status = parameters.DecisionStatus.Trim().ToLowerInvariant();
            query = query.Where(decision => decision.DecisionStatus.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ImplementationStatus))
        {
            var status = parameters.ImplementationStatus.Trim().ToLowerInvariant();
            query = query.Where(decision => decision.ImplementationStatus.ToLower() == status);
        }

        if (parameters.DecisionFrom.HasValue)
        {
            query = query.Where(decision => decision.DecisionDate >= parameters.DecisionFrom.Value);
        }

        if (parameters.DecisionTo.HasValue)
        {
            query = query.Where(decision => decision.DecisionDate <= parameters.DecisionTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(decision =>
                (decision.Outcome != null && decision.Outcome.ToLower().Contains(search))
                || (decision.BusinessValue != null && decision.BusinessValue.ToLower().Contains(search))
                || decision.CreatedBy.ToLower().Contains(search)
                || decision.RecommendationId.ToString().ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<Decision> ApplyOrdering(
        IQueryable<Decision> query,
        DecisionQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "updatedat" => descending
                ? query.OrderByDescending(decision => decision.UpdatedAt)
                : query.OrderBy(decision => decision.UpdatedAt),
            "decisionstatus" => descending
                ? query.OrderByDescending(decision => decision.DecisionStatus)
                : query.OrderBy(decision => decision.DecisionStatus),
            "implementationstatus" => descending
                ? query.OrderByDescending(decision => decision.ImplementationStatus)
                : query.OrderBy(decision => decision.ImplementationStatus),
            "createdat" => descending
                ? query.OrderByDescending(decision => decision.CreatedAt)
                : query.OrderBy(decision => decision.CreatedAt),
            _ => descending
                ? query.OrderByDescending(decision => decision.DecisionDate)
                : query.OrderBy(decision => decision.DecisionDate)
        };
    }
}
