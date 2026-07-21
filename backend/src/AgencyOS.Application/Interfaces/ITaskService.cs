using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(
        TaskQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskResponse> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
}
