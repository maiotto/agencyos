using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IMissionRepository
{
    Task<IReadOnlyList<Mission>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Mission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Mission> AddAsync(Mission mission, CancellationToken cancellationToken = default);

    Task<Mission> UpdateAsync(Mission mission, CancellationToken cancellationToken = default);

    Task DeleteAsync(Mission mission, CancellationToken cancellationToken = default);
}
