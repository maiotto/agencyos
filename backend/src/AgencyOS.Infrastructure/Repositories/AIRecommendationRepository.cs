using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class AIRecommendationRepository : IAIRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    public AIRecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AIRecommendation>> QueryAsync(
        AIRecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<AIRecommendation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AIRecommendations
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AIRecommendation>> GetByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AIRecommendations
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
        var latest = await _context.AIRecommendations
            .AsNoTracking()
            .Where(item => item.RecommendationId == recommendationId)
            .Select(item => (int?)item.GenerationVersion)
            .MaxAsync(cancellationToken);

        return (latest ?? 0) + 1;
    }

    public async Task<AIRecommendation> AddAsync(
        AIRecommendation aiRecommendation,
        CancellationToken cancellationToken = default)
    {
        _context.AIRecommendations.Add(aiRecommendation);
        await _context.SaveChangesAsync(cancellationToken);
        return aiRecommendation;
    }

    public async Task<AIRecommendation> UpdateAsync(
        AIRecommendation aiRecommendation,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(aiRecommendation);
        if (entry.State == EntityState.Detached)
        {
            _context.AIRecommendations.Attach(aiRecommendation);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return aiRecommendation;
    }

    private IQueryable<AIRecommendation> BuildFilteredQuery(AIRecommendationQueryParameters parameters)
    {
        var query = _context.AIRecommendations.AsNoTracking().AsQueryable();

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(item => item.RecommendationId == parameters.RecommendationId.Value);
        }

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(item => item.Status.ToLower() == status);
        }

        if (parameters.MinConfidenceScore.HasValue)
        {
            query = query.Where(item => item.ConfidenceScore >= parameters.MinConfidenceScore.Value);
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
                || item.SuggestedDeliveryStrategy.ToLower().Contains(search)
                || item.Reasoning.ToLower().Contains(search)
                || item.GeneratedBy.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<AIRecommendation> ApplyOrdering(
        IQueryable<AIRecommendation> query,
        AIRecommendationQueryParameters parameters)
    {
        var descending = !string.Equals(parameters.OrderDirection, "asc", StringComparison.OrdinalIgnoreCase);
        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "confidence" or "confidencescore" => descending
                ? query.OrderByDescending(item => item.ConfidenceScore)
                : query.OrderBy(item => item.ConfidenceScore),
            "generationversion" => descending
                ? query.OrderByDescending(item => item.GenerationVersion)
                : query.OrderBy(item => item.GenerationVersion),
            "status" => descending
                ? query.OrderByDescending(item => item.Status)
                : query.OrderBy(item => item.Status),
            _ => descending
                ? query.OrderByDescending(item => item.GeneratedAt)
                : query.OrderBy(item => item.GeneratedAt)
        };
    }
}
