using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface ICompanyDecisionProfileRepository
{
    Task<CompanyDecisionProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyDecisionProfile>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
