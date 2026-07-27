using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Recommendation>> QueryAsync(
        RecommendationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Recommendation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .FirstOrDefaultAsync(recommendation => recommendation.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Recommendation>> GetByCompanyIdAsync(
        Guid companyId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new RecommendationQueryParameters();
        parameters.CompanyId = companyId;
        return await QueryAsync(parameters, cancellationToken);
    }

    public async Task<IReadOnlyList<Recommendation>> GetByMissionIdAsync(
        Guid missionId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new RecommendationQueryParameters();
        parameters.MissionId = missionId;
        return await QueryAsync(parameters, cancellationToken);
    }

    public async Task<IReadOnlyList<Recommendation>> GetByContractIdAsync(
        Guid contractId,
        RecommendationQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new RecommendationQueryParameters();
        parameters.ContractId = contractId;
        return await QueryAsync(parameters, cancellationToken);
    }

    public async Task<IReadOnlyList<Recommendation>> GetVersionsByNumberAsync(
        string recommendationNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .AsNoTracking()
            .Where(recommendation => recommendation.RecommendationNumber == recommendationNumber)
            .OrderBy(recommendation => recommendation.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recommendation?> GetLatestByDeliveryStrategyAsync(
        Guid deliveryStrategyId,
        Guid contractId,
        Guid missionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Where(recommendation =>
                recommendation.DeliveryStrategyId == deliveryStrategyId
                && recommendation.ContractId == contractId
                && recommendation.MissionId == missionId)
            .OrderByDescending(recommendation => recommendation.Version)
            .ThenByDescending(recommendation => recommendation.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Recommendation> AddAsync(
        Recommendation recommendation,
        CancellationToken cancellationToken = default)
    {
        _context.Recommendations.Add(recommendation);
        await _context.SaveChangesAsync(cancellationToken);
        return recommendation;
    }

    public async Task AddRangeAsync(
        IReadOnlyList<Recommendation> recommendations,
        CancellationToken cancellationToken = default)
    {
        _context.Recommendations.AddRange(recommendations);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Recommendation> UpdateAsync(
        Recommendation recommendation,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(recommendation);
        if (entry.State == EntityState.Detached)
        {
            _context.Recommendations.Attach(recommendation);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return recommendation;
    }

    private IQueryable<Recommendation> BuildFilteredQuery(RecommendationQueryParameters parameters)
    {
        var query = _context.Recommendations.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(recommendation => recommendation.CompanyId == parameters.CompanyId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            query = query.Where(recommendation => recommendation.MissionId == parameters.MissionId.Value);
        }

        if (parameters.ContractId.HasValue)
        {
            query = query.Where(recommendation => recommendation.ContractId == parameters.ContractId.Value);
        }

        if (parameters.DeliveryStrategyId.HasValue)
        {
            query = query.Where(recommendation =>
                recommendation.DeliveryStrategyId == parameters.DeliveryStrategyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(recommendation => recommendation.Status.ToLower() == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.RecommendationNumber))
        {
            var number = parameters.RecommendationNumber.Trim().ToLowerInvariant();
            query = query.Where(recommendation =>
                recommendation.RecommendationNumber.ToLower().Contains(number));
        }

        if (parameters.Version.HasValue)
        {
            query = query.Where(recommendation => recommendation.Version == parameters.Version.Value);
        }

        if (parameters.Archived.HasValue)
        {
            query = query.Where(recommendation => recommendation.Archived == parameters.Archived.Value);
        }
        else if (!parameters.IncludeArchived)
        {
            query = query.Where(recommendation => !recommendation.Archived);
        }

        if (!string.IsNullOrWhiteSpace(parameters.GeneratedBy))
        {
            var generatedBy = parameters.GeneratedBy.Trim().ToLowerInvariant();
            query = query.Where(recommendation =>
                recommendation.GeneratedBy.ToLower().Contains(generatedBy));
        }

        if (parameters.GeneratedFrom.HasValue)
        {
            query = query.Where(recommendation => recommendation.GeneratedAt >= parameters.GeneratedFrom.Value);
        }

        if (parameters.GeneratedTo.HasValue)
        {
            query = query.Where(recommendation => recommendation.GeneratedAt <= parameters.GeneratedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(recommendation =>
                recommendation.Title.ToLower().Contains(search)
                || recommendation.RecommendationNumber.ToLower().Contains(search)
                || (recommendation.Summary != null && recommendation.Summary.ToLower().Contains(search))
                || (recommendation.Reason != null && recommendation.Reason.ToLower().Contains(search)));
        }

        return query;
    }

    private static IQueryable<Recommendation> ApplyOrdering(
        IQueryable<Recommendation> query,
        RecommendationQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var orderBy = parameters.OrderBy?.Trim().ToLowerInvariant();

        return orderBy switch
        {
            "title" => descending
                ? query.OrderByDescending(recommendation => recommendation.Title)
                : query.OrderBy(recommendation => recommendation.Title),
            "status" => descending
                ? query.OrderByDescending(recommendation => recommendation.Status)
                : query.OrderBy(recommendation => recommendation.Status),
            "version" => descending
                ? query.OrderByDescending(recommendation => recommendation.Version)
                : query.OrderBy(recommendation => recommendation.Version),
            "rank" => descending
                ? query.OrderByDescending(recommendation => recommendation.Rank)
                : query.OrderBy(recommendation => recommendation.Rank),
            "score" => descending
                ? query.OrderByDescending(recommendation => recommendation.Score)
                : query.OrderBy(recommendation => recommendation.Score),
            "number" or "recommendationnumber" => descending
                ? query.OrderByDescending(recommendation => recommendation.RecommendationNumber)
                : query.OrderBy(recommendation => recommendation.RecommendationNumber),
            _ => descending
                ? query.OrderByDescending(recommendation => recommendation.GeneratedAt)
                : query.OrderBy(recommendation => recommendation.GeneratedAt)
        };
    }
}
