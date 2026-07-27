using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ExecutionResourceRepository : IExecutionResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ExecutionResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExecutionResource>> GetAllAsync(
        ExecutionResourceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ExecutionResource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExecutionResources
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(
        string code,
        Guid? excludeResourceId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();

        var query = _context.ExecutionResources
            .AsNoTracking()
            .Where(r => r.Code.ToLower() == normalizedCode);

        if (excludeResourceId.HasValue)
        {
            query = query.Where(r => r.Id != excludeResourceId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<ExecutionResource> AddAsync(
        ExecutionResource resource,
        CancellationToken cancellationToken = default)
    {
        _context.ExecutionResources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);
        return resource;
    }

    public async Task<ExecutionResource> UpdateAsync(
        ExecutionResource resource,
        CancellationToken cancellationToken = default)
    {
        _context.ExecutionResources.Update(resource);
        await _context.SaveChangesAsync(cancellationToken);
        return resource;
    }

    private IQueryable<ExecutionResource> BuildFilteredQuery(ExecutionResourceQueryParameters parameters)
    {
        var query = _context.ExecutionResources
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.ResourceType))
        {
            var resourceType = parameters.ResourceType.Trim();
            query = query.Where(r => EF.Functions.ILike(r.ResourceType, resourceType));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            query = query.Where(r => EF.Functions.ILike(r.Status, status));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Skill))
        {
            var skill = parameters.Skill.Trim();
            query = query.Where(r =>
                r.Skills != null
                && r.Skills.Any(s => EF.Functions.ILike(s, skill)));
        }

        return query;
    }

    private static IQueryable<ExecutionResource> ApplyOrdering(
        IQueryable<ExecutionResource> query,
        ExecutionResourceQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "resourceType", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(r => r.ResourceType).ThenBy(r => r.Name)
                : query.OrderBy(r => r.ResourceType).ThenBy(r => r.Name);
        }

        return descending
            ? query.OrderByDescending(r => r.Name).ThenBy(r => r.Code)
            : query.OrderBy(r => r.Name).ThenBy(r => r.Code);
    }
}
