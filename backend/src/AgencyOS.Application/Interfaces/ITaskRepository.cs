using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<MissionTask>> GetAllAsync(
        TaskQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<MissionTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithCodeForMissionAsync(
        Guid missionId,
        string code,
        Guid? excludeTaskId = null,
        CancellationToken cancellationToken = default);

    Task<TaskStatusLookup?> GetStatusByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskStatusLookup?> GetStatusByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<TaskTypeLookup?> GetTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MissionTask> AddAsync(MissionTask task, CancellationToken cancellationToken = default);

    Task<MissionTask> UpdateAsync(MissionTask task, CancellationToken cancellationToken = default);

    Task DeleteAsync(MissionTask task, CancellationToken cancellationToken = default);
}
