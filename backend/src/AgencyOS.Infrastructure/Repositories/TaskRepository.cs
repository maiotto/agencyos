using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MissionTask>> GetAllAsync(
        TaskQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);

        return await query
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<MissionTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MissionTasks
            .Include(t => t.Status)
            .Include(t => t.Type)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithCodeForMissionAsync(
        Guid missionId,
        string code,
        Guid? excludeTaskId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();

        var query = _context.MissionTasks
            .AsNoTracking()
            .Where(t => t.MissionId == missionId && t.Code == normalizedCode);

        if (excludeTaskId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTaskId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<TaskStatusLookup?> GetStatusByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<TaskStatusLookup?> GetStatusByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.TaskStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
    }

    public async Task<TaskTypeLookup?> GetTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<MissionTask> AddAsync(MissionTask task, CancellationToken cancellationToken = default)
    {
        _context.MissionTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<MissionTask> UpdateAsync(MissionTask task, CancellationToken cancellationToken = default)
    {
        _context.MissionTasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task DeleteAsync(MissionTask task, CancellationToken cancellationToken = default)
    {
        _context.MissionTasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<MissionTask> BuildFilteredQuery(TaskQueryParameters parameters)
    {
        var query = _context.MissionTasks
            .AsNoTracking()
            .Include(t => t.Status)
            .Include(t => t.Type)
            .AsQueryable();

        if (parameters.MissionId.HasValue)
        {
            query = query.Where(t => t.MissionId == parameters.MissionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            query = query.Where(t => t.Status != null && EF.Functions.ILike(t.Status.Code, status));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Priority))
        {
            var priority = parameters.Priority.Trim();
            query = query.Where(t => EF.Functions.ILike(t.Priority, priority));
        }

        if (parameters.TaskTypeId.HasValue)
        {
            query = query.Where(t => t.TaskTypeId == parameters.TaskTypeId.Value);
        }

        return query;
    }
}
