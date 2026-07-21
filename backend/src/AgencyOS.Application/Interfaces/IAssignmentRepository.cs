using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IAssignmentRepository
{
    Task<IReadOnlyList<Assignment>> GetAllAsync(
        AssignmentQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Assignment> AddAsync(Assignment assignment, CancellationToken cancellationToken = default);

    Task<Assignment> UpdateAsync(Assignment assignment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Assignment>> GetForCapacityCalculationAsync(
        DateOnly periodStartDate,
        DateOnly periodEndDate,
        Guid? executionResourceId = null,
        CancellationToken cancellationToken = default);
}
