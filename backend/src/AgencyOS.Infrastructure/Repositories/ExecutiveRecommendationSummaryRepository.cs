using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ExecutiveRecommendationSummaryRepository : IExecutiveRecommendationSummaryRepository
{
    private readonly ApplicationDbContext _context;

    public ExecutiveRecommendationSummaryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExecutiveRecommendationSummary>> QueryAsync(
        ExecutiveRecommendationSummaryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ExecutiveRecommendationSummary?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ExecutiveRecommendationSummaries
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ExecutiveRecommendationSummary>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        // BR-1810: Archived summaries remain queryable.
        return await _context.ExecutiveRecommendationSummaries
            .AsNoTracking()
            .Where(item => item.RecommendationId == recommendationId)
            .OrderByDescending(item => item.SummaryVersion)
            .ThenByDescending(item => item.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextSummaryVersionAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var latest = await _context.ExecutiveRecommendationSummaries
            .AsNoTracking()
            .Where(item => item.RecommendationId == recommendationId)
            .Select(item => (int?)item.SummaryVersion)
            .MaxAsync(cancellationToken);

        return (latest ?? 0) + 1;
    }

    public async Task<ExecutiveRecommendationSummary> AddAsync(
        ExecutiveRecommendationSummary summary,
        CancellationToken cancellationToken = default)
    {
        _context.ExecutiveRecommendationSummaries.Add(summary);
        await _context.SaveChangesAsync(cancellationToken);
        return summary;
    }

    public async Task<ExecutiveRecommendationSummary> UpdateAsync(
        ExecutiveRecommendationSummary summary,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(summary);
        if (entry.State == EntityState.Detached)
        {
            _context.ExecutiveRecommendationSummaries.Attach(summary);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return summary;
    }

    private IQueryable<ExecutiveRecommendationSummary> BuildFilteredQuery(
        ExecutiveRecommendationSummaryQueryParameters parameters)
    {
        var query = _context.ExecutiveRecommendationSummaries.AsNoTracking().AsQueryable();

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(item => item.RecommendationId == parameters.RecommendationId.Value);
        }

        if (parameters.AIRecommendationId.HasValue)
        {
            query = query.Where(item => item.AIRecommendationId == parameters.AIRecommendationId.Value);
        }

        if (parameters.ExplainabilityId.HasValue)
        {
            query = query.Where(item => item.ExplainabilityId == parameters.ExplainabilityId.Value);
        }

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == parameters.CompanyId.Value);
        }

        if (!parameters.IncludeArchived && string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(item => item.Status == ExecutiveRecommendationSummaryStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(item => item.Status.ToLower() == status);
        }

        if (parameters.MinConfidenceLevel.HasValue)
        {
            query = query.Where(item => item.ConfidenceLevel >= parameters.MinConfidenceLevel.Value);
        }

        if (parameters.GeneratedFrom.HasValue)
        {
            query = query.Where(item => item.GeneratedAt >= parameters.GeneratedFrom.Value);
        }

        if (parameters.GeneratedTo.HasValue)
        {
            query = query.Where(item => item.GeneratedAt <= parameters.GeneratedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(item =>
                item.ExecutiveSummary.ToLower().Contains(search)
                || item.BusinessImpact.ToLower().Contains(search)
                || item.RecommendedActions.ToLower().Contains(search)
                || item.GeneratedBy.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<ExecutiveRecommendationSummary> ApplyOrdering(
        IQueryable<ExecutiveRecommendationSummary> query,
        ExecutiveRecommendationSummaryQueryParameters parameters)
    {
        var descending = !string.Equals(parameters.OrderDirection, "asc", StringComparison.OrdinalIgnoreCase);
        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "confidence" or "confidencelevel" => descending
                ? query.OrderByDescending(item => item.ConfidenceLevel)
                : query.OrderBy(item => item.ConfidenceLevel),
            "summaryversion" => descending
                ? query.OrderByDescending(item => item.SummaryVersion)
                : query.OrderBy(item => item.SummaryVersion),
            "status" => descending
                ? query.OrderByDescending(item => item.Status)
                : query.OrderBy(item => item.Status),
            _ => descending
                ? query.OrderByDescending(item => item.GeneratedAt)
                : query.OrderBy(item => item.GeneratedAt)
        };
    }
}
