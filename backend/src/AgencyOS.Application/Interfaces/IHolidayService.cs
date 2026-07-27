using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

public interface IHolidayService
{
    Task<IReadOnlyList<HolidayResponse>> GetAllAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HolidayResponse>> FilterAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<HolidayResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HolidayResponse> CreateAsync(
        CreateHolidayRequest request,
        CancellationToken cancellationToken = default);

    Task<HolidayResponse> UpdateAsync(
        Guid id,
        UpdateHolidayRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
