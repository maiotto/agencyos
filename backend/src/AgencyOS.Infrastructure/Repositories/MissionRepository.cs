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
            .ToListAsync(cancellationToken);
    }

    public async Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Missions
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
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
