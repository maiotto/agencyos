using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IMissionService
{
    Task<IReadOnlyList<MissionResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MissionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MissionResponse> CreateAsync(CreateMissionRequest request, CancellationToken cancellationToken = default);

    Task<MissionResponse> UpdateAsync(Guid id, UpdateMissionRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
