using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class RecommendationHistoryRepository : IRecommendationHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendationHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecommendationHistory>> QueryAsync(
        RecommendationHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<RecommendationHistory?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.RecommendationHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(history => history.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendationHistory>> GetVersionsByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == recommendationId, cancellationToken);

        if (recommendation is null)
        {
            return [];
        }

        return await GetVersionsByRecommendationNumberAsync(
            recommendation.RecommendationNumber,
            cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendationHistory>> GetVersionsByRecommendationNumberAsync(
        string recommendationNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recommendationNumber))
        {
            return [];
        }

        return await _context.RecommendationHistories
            .AsNoTracking()
            .Where(history =>
                history.RecommendationNumber == recommendationNumber
                && history.EventType == RecommendationHistoryEventType.VersionCreated)
            .OrderBy(history => history.RecommendationVersion)
            .ThenBy(history => history.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecommendationHistory?> GetVersionSnapshotByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RecommendationHistories
            .AsNoTracking()
            .Where(history =>
                history.RecommendationId == recommendationId
                && history.EventType == RecommendationHistoryEventType.VersionCreated)
            .OrderByDescending(history => history.CreatedAt)
            .ThenByDescending(history => history.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendationHistory>> GetTimelineByRecommendationIdAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == recommendationId, cancellationToken);

        if (recommendation is null)
        {
            return [];
        }

        return await _context.RecommendationHistories
            .AsNoTracking()
            .Where(history => history.RecommendationNumber == recommendation.RecommendationNumber)
            .OrderBy(history => history.CreatedAt)
            .ThenBy(history => history.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<RecommendationHistory> AddAsync(
        RecommendationHistory history,
        CancellationToken cancellationToken = default)
    {
        _context.RecommendationHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task AddRangeAsync(
        IReadOnlyList<RecommendationHistory> histories,
        CancellationToken cancellationToken = default)
    {
        _context.RecommendationHistories.AddRange(histories);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<RecommendationHistory> BuildFilteredQuery(
        RecommendationHistoryQueryParameters parameters)
    {
        var query = _context.RecommendationHistories.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(history => history.CompanyId == parameters.CompanyId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            query = query.Where(history => history.MissionId == parameters.MissionId.Value);
        }

        if (parameters.ContractId.HasValue)
        {
            query = query.Where(history => history.ContractId == parameters.ContractId.Value);
        }

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(history => history.RecommendationId == parameters.RecommendationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.RecommendationNumber))
        {
            var number = parameters.RecommendationNumber.Trim().ToLowerInvariant();
            query = query.Where(history => history.RecommendationNumber.ToLower().Contains(number));
        }

        if (parameters.Version.HasValue)
        {
            query = query.Where(history => history.RecommendationVersion == parameters.Version.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.WorkflowStatus))
        {
            var status = parameters.WorkflowStatus.Trim().ToLowerInvariant();
            query = query.Where(history =>
                history.WorkflowStatus != null && history.WorkflowStatus.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.EventType))
        {
            var eventType = parameters.EventType.Trim().ToLowerInvariant();
            query = query.Where(history => history.EventType.ToLower() == eventType);
        }

        if (parameters.CreatedFrom.HasValue)
        {
            query = query.Where(history => history.CreatedAt >= parameters.CreatedFrom.Value);
        }

        if (parameters.CreatedTo.HasValue)
        {
            query = query.Where(history => history.CreatedAt <= parameters.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(history =>
                history.Title.ToLower().Contains(search)
                || history.RecommendationNumber.ToLower().Contains(search)
                || (history.Summary != null && history.Summary.ToLower().Contains(search))
                || history.CreatedBy.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<RecommendationHistory> ApplyOrdering(
        IQueryable<RecommendationHistory> query,
        RecommendationHistoryQueryParameters parameters)
    {
        var descending = !string.Equals(parameters.OrderDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var orderBy = parameters.OrderBy?.Trim().ToLowerInvariant();

        return orderBy switch
        {
            "title" => descending
                ? query.OrderByDescending(history => history.Title)
                : query.OrderBy(history => history.Title),
            "version" => descending
                ? query.OrderByDescending(history => history.RecommendationVersion)
                : query.OrderBy(history => history.RecommendationVersion),
            "eventtype" => descending
                ? query.OrderByDescending(history => history.EventType)
                : query.OrderBy(history => history.EventType),
            "number" or "recommendationnumber" => descending
                ? query.OrderByDescending(history => history.RecommendationNumber)
                : query.OrderBy(history => history.RecommendationNumber),
            _ => descending
                ? query.OrderByDescending(history => history.CreatedAt)
                : query.OrderBy(history => history.CreatedAt)
        };
    }
}
