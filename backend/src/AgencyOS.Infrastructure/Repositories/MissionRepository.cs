using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class MissionRepository : IMissionRepository
{
    private readonly ApplicationDbContext _context;

    public MissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Mission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ThenBy(m => m.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(
        string code,
        Guid? excludeMissionId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();

        var query = _context.Missions
            .AsNoTracking()
            .Where(m => m.Code.ToLower() == normalizedCode);

        if (excludeMissionId.HasValue)
        {
            query = query.Where(m => m.Id != excludeMissionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Mission> AddAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        _context.Missions.Add(mission);
        await _context.SaveChangesAsync(cancellationToken);
        return mission;
    }

    public async Task<Mission> UpdateAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        _context.Missions.Update(mission);
        await _context.SaveChangesAsync(cancellationToken);
        return mission;
    }

    public async Task DeleteAsync(Mission mission, CancellationToken cancellationToken = default)
    {
        _context.Missions.Remove(mission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
