using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IPortfolioRepository
{
    Task<IReadOnlyList<Portfolio>> GetAllAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCompanyAndNameAsync(
        Guid companyId,
        string name,
        Guid? excludePortfolioId = null,
        CancellationToken cancellationToken = default);

    Task<Portfolio> AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default);

    Task<Portfolio> UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default);

    Task DeleteAsync(Portfolio portfolio, CancellationToken cancellationToken = default);
}
