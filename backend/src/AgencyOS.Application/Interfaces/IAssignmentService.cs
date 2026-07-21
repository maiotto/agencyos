using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IAssignmentService
{
    Task<IReadOnlyList<AssignmentResponse>> GetAllAsync(
        AssignmentQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AssignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentResponse> CreateAsync(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AssignmentResponse> UpdateAsync(
        Guid id,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AssignmentResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
