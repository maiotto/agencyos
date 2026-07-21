using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public AssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Assignment>> GetAllAsync(
        AssignmentQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Assignment> AddAsync(
        Assignment assignment,
        CancellationToken cancellationToken = default)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<Assignment> UpdateAsync(
        Assignment assignment,
        CancellationToken cancellationToken = default)
    {
        _context.Assignments.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    private IQueryable<Assignment> BuildFilteredQuery(AssignmentQueryParameters parameters)
    {
        var query = _context.Assignments
            .AsNoTracking()
            .AsQueryable();

        if (parameters.TaskId.HasValue)
        {
            query = query.Where(a => a.TaskId == parameters.TaskId.Value);
        }

        if (parameters.MissionId.HasValue)
        {
            var missionId = parameters.MissionId.Value;
            query = query.Where(a =>
                _context.MissionTasks.Any(t => t.Id == a.TaskId && t.MissionId == missionId));
        }

        if (parameters.ExecutionResourceId.HasValue)
        {
            query = query.Where(a => a.ExecutionResourceId == parameters.ExecutionResourceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ResourceType))
        {
            var resourceType = parameters.ResourceType.Trim();
            query = query.Where(a =>
                _context.ExecutionResources.Any(r =>
                    r.Id == a.ExecutionResourceId
                    && EF.Functions.ILike(r.ResourceType, resourceType)));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            query = query.Where(a => EF.Functions.ILike(a.Status, status));
        }

        return query;
    }

    private static IQueryable<Assignment> ApplyOrdering(
        IQueryable<Assignment> query,
        AssignmentQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "plannedHours", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(a => a.PlannedHours).ThenBy(a => a.PlannedStartDate)
                : query.OrderBy(a => a.PlannedHours).ThenBy(a => a.PlannedStartDate);
        }

        return descending
            ? query.OrderByDescending(a => a.PlannedStartDate).ThenBy(a => a.Id)
            : query.OrderBy(a => a.PlannedStartDate).ThenBy(a => a.Id);
    }

    public async Task<IReadOnlyList<Assignment>> GetForCapacityCalculationAsync(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        Guid? executionResourceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assignments
            .AsNoTracking()
            .Where(a =>
                a.PlannedStartDate <= periodEndDate
                && a.PlannedEndDate >= periodStartDate
                && !EF.Functions.ILike(a.Status, AssignmentStatus.Cancelled)
                && !EF.Functions.ILike(a.Status, AssignmentStatus.Completed));

        if (executionResourceId.HasValue)
        {
            query = query.Where(a => a.ExecutionResourceId == executionResourceId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
