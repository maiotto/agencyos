namespace AgencyOS.Domain.Entities;

/// <summary>
/// Holiday aggregate (US-102). Official source of non-working days for Planning.
/// </summary>
public class Holiday
{
    public Guid Id { get; private set; }

    public Guid? CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string HolidayType { get; private set; } = string.Empty;

    public DateOnly HolidayDate { get; private set; }

    public string? StateCode { get; private set; }

    public string? City { get; private set; }

    public bool Recurring { get; private set; }

    public string Status { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => HolidayStatus.IsActive(Status);

    public bool IsInactive => HolidayStatus.IsInactive(Status);

    private Holiday()
    {
    }

    public static Holiday Create(
        Guid? companyId,
        string name,
        string? description,
        string holidayType,
        DateOnly holidayDate,
        string? stateCode,
        string? city,
        bool recurring,
        DateTimeOffset createdAt)
    {
        var canonicalType = HolidayTypes.Canonicalize(holidayType);
        ValidateConfiguration(companyId, name, canonicalType, stateCode, city);

        var (normalizedState, normalizedCity) = NormalizeScope(canonicalType, stateCode, city);

        return new Holiday
        {
            Id = Guid.NewGuid(),
            CompanyId = ResolveCompanyId(canonicalType, companyId),
            Name = name.Trim(),
            Description = HolidayRules.NormalizeOptional(description),
            HolidayType = canonicalType,
            HolidayDate = holidayDate,
            StateCode = normalizedState,
            City = normalizedCity,
            Recurring = recurring,
            Status = HolidayStatus.Inactive,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    public void Reconfigure(
        Guid? companyId,
        string name,
        string? description,
        string holidayType,
        DateOnly holidayDate,
        string? stateCode,
        string? city,
        bool recurring,
        DateOnly asOfDate,
        DateTimeOffset updatedAt)
    {
        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must never be modified. Holidays that have already occurred cannot be reconfigured.");
        }

        var canonicalType = HolidayTypes.Canonicalize(holidayType);
        ValidateConfiguration(companyId, name, canonicalType, stateCode, city);

        var (normalizedState, normalizedCity) = NormalizeScope(canonicalType, stateCode, city);

        CompanyId = ResolveCompanyId(canonicalType, companyId);
        Name = name.Trim();
        Description = HolidayRules.NormalizeOptional(description);
        HolidayType = canonicalType;
        HolidayDate = holidayDate;
        StateCode = normalizedState;
        City = normalizedCity;
        Recurring = recurring;
        UpdatedAt = updatedAt;
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        if (IsActive)
        {
            return;
        }

        Status = HolidayStatus.Active;
        UpdatedAt = updatedAt;
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        if (IsInactive)
        {
            return;
        }

        Status = HolidayStatus.Inactive;
        UpdatedAt = updatedAt;
    }

    public void EnsureCanDelete(DateOnly asOfDate)
    {
        if (!HolidayStatus.CanDelete(Status))
        {
            throw new InvalidOperationException("Deleting Active Holidays is prohibited. Deactivate first.");
        }

        if (IsHistorical(asOfDate))
        {
            throw new InvalidOperationException(
                "Historical planning must never be modified. Holidays that have already occurred cannot be deleted.");
        }
    }

    public bool IsHistorical(DateOnly asOfDate) =>
        HolidayRules.IsHistorical(HolidayDate, Recurring, asOfDate);

    public bool OccursOn(DateOnly date) =>
        HolidayRules.OccursOn(HolidayDate, Recurring, date);

    public bool AffectsCompany(Guid companyId)
    {
        if (HolidayTypes.IsCompany(HolidayType))
        {
            return CompanyId == companyId;
        }

        return CompanyId is null || CompanyId == companyId;
    }

    public bool SameScopeAs(
        string holidayType,
        Guid? companyId,
        string? stateCode,
        string? city,
        DateOnly holidayDate,
        bool recurring) =>
        HolidayRules.SameScope(
            HolidayType,
            CompanyId,
            StateCode,
            City,
            HolidayDate,
            Recurring,
            holidayType,
            companyId,
            stateCode,
            city,
            holidayDate,
            recurring);

    private static Guid? ResolveCompanyId(string holidayType, Guid? companyId)
    {
        if (HolidayTypes.IsCompany(holidayType))
        {
            return companyId;
        }

        return companyId is null || companyId == Guid.Empty ? null : companyId;
    }

    private static void ValidateConfiguration(
        Guid? companyId,
        string name,
        string holidayType,
        string? stateCode,
        string? city)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Holiday Name is mandatory.");
        }

        if (!HolidayTypes.All.Contains(holidayType))
        {
            throw new InvalidOperationException("Holiday Type must be a valid holiday type.");
        }

        if (HolidayRules.RequiresCompanyId(holidayType) && (companyId is null || companyId == Guid.Empty))
        {
            throw new InvalidOperationException("Company Holiday requires CompanyId.");
        }

        if (HolidayRules.RequiresStateCode(holidayType) && string.IsNullOrWhiteSpace(stateCode))
        {
            throw new InvalidOperationException("StateCode is required for State and Municipal holidays.");
        }

        if (HolidayRules.RequiresCity(holidayType) && string.IsNullOrWhiteSpace(city))
        {
            throw new InvalidOperationException("Municipal Holiday requires StateCode and City.");
        }
    }

    private static (string? StateCode, string? City) NormalizeScope(
        string holidayType,
        string? stateCode,
        string? city)
    {
        if (HolidayRules.IgnoresStateAndCity(holidayType))
        {
            return (null, null);
        }

        return (HolidayRules.NormalizeStateCode(stateCode), HolidayRules.NormalizeCity(city));
    }
}
