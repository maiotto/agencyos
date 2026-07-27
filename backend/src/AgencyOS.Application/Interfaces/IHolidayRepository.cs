using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IHolidayRepository
{
    Task<IReadOnlyList<Holiday>> GetAllAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Holiday?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithSameScopeAsync(
        string holidayType,
        Guid? companyId,
        string? stateCode,
        string? city,
        DateOnly holidayDate,
        bool recurring,
        Guid? excludeHolidayId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Holiday>> GetActiveForCompanyOnDateAsync(
        Guid companyId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Holiday>> GetActiveForCompanyInRangeAsync(
        Guid companyId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<Holiday> AddAsync(Holiday holiday, CancellationToken cancellationToken = default);

    Task<Holiday> UpdateAsync(Holiday holiday, CancellationToken cancellationToken = default);

    Task DeleteAsync(Holiday holiday, CancellationToken cancellationToken = default);
}
