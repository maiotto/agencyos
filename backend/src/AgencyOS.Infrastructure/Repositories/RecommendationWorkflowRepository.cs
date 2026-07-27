using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class RecommendationWorkflowRepository : IRecommendationWorkflowRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendationWorkflowRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RecommendationWorkflow>> GetAllAsync(
        RecommendationWorkflowQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.Include(workflow => workflow.Transitions).ToListAsync(cancellationToken);
    }

    public async Task<RecommendationWorkflow?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.RecommendationWorkflows
            .Include(workflow => workflow.Transitions)
            .FirstOrDefaultAsync(workflow => workflow.Id == id, cancellationToken);
    }

    public async Task<RecommendationWorkflow> AddAsync(
        RecommendationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        _context.RecommendationWorkflows.Add(workflow);
        await _context.SaveChangesAsync(cancellationToken);
        return workflow;
    }

    public async Task<RecommendationWorkflow> UpdateAsync(
        RecommendationWorkflow workflow,
        CancellationToken cancellationToken = default)
    {
        var existingTransitionIds = await _context.RecommendationWorkflowTransitions
            .AsNoTracking()
            .Where(transition => transition.RecommendationWorkflowId == workflow.Id)
            .Select(transition => transition.Id)
            .ToListAsync(cancellationToken);
        var existingSet = existingTransitionIds.ToHashSet();

        var entry = _context.Entry(workflow);
        if (entry.State == EntityState.Detached)
        {
            _context.RecommendationWorkflows.Attach(workflow);
            entry.State = EntityState.Modified;
        }

        foreach (var transition in workflow.Transitions)
        {
            if (existingSet.Contains(transition.Id))
            {
                continue;
            }

            var transitionEntry = _context.Entry(transition);
            if (transitionEntry.State == EntityState.Detached)
            {
                _context.RecommendationWorkflowTransitions.Add(transition);
            }
            else
            {
                // Client-generated keys are discovered as Modified; force insert.
                transitionEntry.State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return workflow;
    }

    private IQueryable<RecommendationWorkflow> BuildFilteredQuery(
        RecommendationWorkflowQueryParameters parameters)
    {
        var query = _context.RecommendationWorkflows.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(workflow => workflow.Status.ToLower() == status);
        }

        if (parameters.RecommendationId.HasValue)
        {
            query = query.Where(workflow => workflow.RecommendationId == parameters.RecommendationId.Value);
        }

        if (parameters.DeliveryStrategyId.HasValue)
        {
            query = query.Where(workflow => workflow.DeliveryStrategyId == parameters.DeliveryStrategyId.Value);
        }

        if (parameters.ContractId.HasValue)
        {
            query = query.Where(workflow => workflow.ContractId == parameters.ContractId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            query = query.Where(workflow => workflow.MissionId == parameters.MissionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CreatedBy))
        {
            var createdBy = parameters.CreatedBy.Trim().ToLowerInvariant();
            query = query.Where(workflow => workflow.CreatedBy.ToLower().Contains(createdBy));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(workflow =>
                workflow.Title.ToLower().Contains(search)
                || (workflow.Summary != null && workflow.Summary.ToLower().Contains(search)));
        }

        return query;
    }

    private static IQueryable<RecommendationWorkflow> ApplyOrdering(
        IQueryable<RecommendationWorkflow> query,
        RecommendationWorkflowQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var orderBy = parameters.OrderBy?.Trim().ToLowerInvariant();

        return orderBy switch
        {
            "title" => descending
                ? query.OrderByDescending(workflow => workflow.Title)
                : query.OrderBy(workflow => workflow.Title),
            "status" => descending
                ? query.OrderByDescending(workflow => workflow.Status)
                : query.OrderBy(workflow => workflow.Status),
            "updatedat" => descending
                ? query.OrderByDescending(workflow => workflow.UpdatedAt)
                : query.OrderBy(workflow => workflow.UpdatedAt),
            _ => descending
                ? query.OrderByDescending(workflow => workflow.CreatedAt)
                : query.OrderBy(workflow => workflow.CreatedAt)
        };
    }
}
