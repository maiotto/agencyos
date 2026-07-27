using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ExplainabilityRepository : IExplainabilityRepository
{
    private readonly ApplicationDbContext _context;

    public ExplainabilityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Explainability>> QueryAsync(
        ExplainabilityQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Explainability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Explainabilities
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Explainability>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Explainabilities
            .AsNoTracking()
            .Where(item => item.RecommendationId == recommendationId)
            .OrderByDescending(item => item.GenerationVersion)
            .ThenByDescending(item => item.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextGenerationVersionAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var latest = await _context.Explainabilities
            .AsNoTracking()
            .Where(item => item.RecommendationId == recommendationId)
            .Select(item => (int?)item.GenerationVersion)
            .MaxAsync(cancellationToken);

        return (latest ?? 0) + 1;
    }

    public async Task<Explainability> AddAsync(
        Explainability explainability,
        CancellationToken cancellationToken = default)
    {
        _context.Explainabilities.Add(explainability);
        await _context.SaveChangesAsync(cancellationToken);
        return explainability;
    }

    public async Task<Explainability> UpdateAsync(
        Explainability explainability,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(explainability);
        if (entry.State == EntityState.Detached)
        {
            _context.Explainabilities.Attach(explainability);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return explainability;
    }

    private IQueryable<Explainability> BuildFilteredQuery(ExplainabilityQueryParameters parameters)
    {
        var query = _context.Explainabilities.AsNoTracking().AsQueryable();

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(item => item.RecommendationId == parameters.RecommendationId.Value);
        }

        if (parameters.AIRecommendationId.HasValue)
        {
            query = query.Where(item => item.AIRecommendationId == parameters.AIRecommendationId.Value);
        }

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ExplanationType))
        {
            var type = parameters.ExplanationType.Trim().ToLowerInvariant();
            query = query.Where(item => item.ExplanationType.ToLower() == type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(item => item.Status.ToLower() == status);
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
                || item.DetailedExplanation.ToLower().Contains(search)
                || item.ConfidenceExplanation.ToLower().Contains(search)
                || item.GeneratedBy.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<Explainability> ApplyOrdering(
        IQueryable<Explainability> query,
        ExplainabilityQueryParameters parameters)
    {
        var descending = !string.Equals(parameters.OrderDirection, "asc", StringComparison.OrdinalIgnoreCase);
        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "generationversion" => descending
                ? query.OrderByDescending(item => item.GenerationVersion)
                : query.OrderBy(item => item.GenerationVersion),
            "explanationtype" => descending
                ? query.OrderByDescending(item => item.ExplanationType)
                : query.OrderBy(item => item.ExplanationType),
            "status" => descending
                ? query.OrderByDescending(item => item.Status)
                : query.OrderBy(item => item.Status),
            _ => descending
                ? query.OrderByDescending(item => item.GeneratedAt)
                : query.OrderBy(item => item.GeneratedAt)
        };
    }
}
