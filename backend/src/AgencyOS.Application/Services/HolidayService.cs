using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly IHolidayRepository _holidayRepository;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(
        IHolidayRepository holidayRepository,
        ILogger<HolidayService> logger)
    {
        _holidayRepository = holidayRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<HolidayResponse>> GetAllAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var holidays = await _holidayRepository.GetAllAsync(parameters, cancellationToken);
        return holidays.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<HolidayResponse>> FilterAsync(
        HolidayQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<HolidayResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await GetHolidayOrThrowAsync(id, cancellationToken);
        return MapToResponse(holiday);
    }

    public async Task<HolidayResponse> CreateAsync(
        CreateHolidayRequest request,
        CancellationToken cancellationToken = default)
    {
        Holiday holiday;

        try
        {
            holiday = Holiday.Create(
                request.CompanyId,
                request.Name,
                request.Description,
                request.HolidayType,
                request.HolidayDate,
                request.StateCode,
                request.City,
                request.Recurring,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await EnsureScopeIsUniqueAsync(holiday, null, cancellationToken);

        var created = await _holidayRepository.AddAsync(holiday, cancellationToken);

        _logger.LogInformation(
            "Holiday Created: {HolidayId} ({HolidayName}) Type={HolidayType}",
            created.Id,
            created.Name,
            created.HolidayType);

        return MapToResponse(created);
    }

    public async Task<HolidayResponse> UpdateAsync(
        Guid id,
        UpdateHolidayRequest request,
        CancellationToken cancellationToken = default)
    {
        var holiday = await GetHolidayOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            holiday.Reconfigure(
                request.CompanyId,
                request.Name,
                request.Description,
                request.HolidayType,
                request.HolidayDate,
                request.StateCode,
                request.City,
                request.Recurring,
                today,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await EnsureScopeIsUniqueAsync(holiday, holiday.Id, cancellationToken);

        var updated = await _holidayRepository.UpdateAsync(holiday, cancellationToken);

        _logger.LogInformation(
            "Holiday Updated: {HolidayId} ({HolidayName})",
            updated.Id,
            updated.Name);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await GetHolidayOrThrowAsync(id, cancellationToken);

        if (holiday.IsActive)
        {
            return;
        }

        await EnsureScopeIsUniqueAsync(holiday, holiday.Id, cancellationToken);

        holiday.Activate(DateTimeOffset.UtcNow);
        await _holidayRepository.UpdateAsync(holiday, cancellationToken);

        _logger.LogInformation(
            "Holiday Activated: {HolidayId} ({HolidayName})",
            holiday.Id,
            holiday.Name);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await GetHolidayOrThrowAsync(id, cancellationToken);

        if (holiday.IsInactive)
        {
            return;
        }

        holiday.Deactivate(DateTimeOffset.UtcNow);
        await _holidayRepository.UpdateAsync(holiday, cancellationToken);

        _logger.LogInformation(
            "Holiday Deactivated: {HolidayId} ({HolidayName})",
            holiday.Id,
            holiday.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await GetHolidayOrThrowAsync(id, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            holiday.EnsureCanDelete(today);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _holidayRepository.DeleteAsync(holiday, cancellationToken);

        _logger.LogInformation(
            "Holiday Deleted: {HolidayId} ({HolidayName})",
            holiday.Id,
            holiday.Name);
    }

    private async Task EnsureScopeIsUniqueAsync(
        Holiday holiday,
        Guid? excludeHolidayId,
        CancellationToken cancellationToken)
    {
        if (await _holidayRepository.ExistsWithSameScopeAsync(
                holiday.HolidayType,
                holiday.CompanyId,
                holiday.StateCode,
                holiday.City,
                holiday.HolidayDate,
                holiday.Recurring,
                excludeHolidayId,
                cancellationToken))
        {
            throw new ConflictException(
                "Duplicate Holidays are not allowed for the same scope.");
        }
    }

    private async Task<Holiday> GetHolidayOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var holiday = await _holidayRepository.GetByIdAsync(id, cancellationToken);

        if (holiday is null)
        {
            throw new NotFoundException($"Holiday with id '{id}' was not found.");
        }

        return holiday;
    }

    internal static HolidayResponse MapToResponse(Holiday holiday)
    {
        return new HolidayResponse
        {
            Id = holiday.Id,
            CompanyId = holiday.CompanyId,
            Name = holiday.Name,
            Description = holiday.Description,
            HolidayType = holiday.HolidayType,
            HolidayDate = holiday.HolidayDate,
            StateCode = holiday.StateCode,
            City = holiday.City,
            Recurring = holiday.Recurring,
            Status = holiday.Status,
            CreatedAt = holiday.CreatedAt,
            UpdatedAt = holiday.UpdatedAt
        };
    }
}
