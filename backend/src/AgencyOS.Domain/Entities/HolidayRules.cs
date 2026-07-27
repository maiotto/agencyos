namespace AgencyOS.Domain.Entities;

/// <summary>
/// Domain rules for Holiday Management (US-102 / BR-201..BR-213).
/// </summary>
public static class HolidayRules
{
    public static bool RequiresCompanyId(string holidayType) =>
        HolidayTypes.IsCompany(holidayType);

    public static bool RequiresStateCode(string holidayType) =>
        HolidayTypes.IsState(holidayType) || HolidayTypes.IsMunicipal(holidayType);

    public static bool RequiresCity(string holidayType) =>
        HolidayTypes.IsMunicipal(holidayType);

    public static bool IgnoresStateAndCity(string holidayType) =>
        HolidayTypes.IsNational(holidayType);

    public static bool IsHistorical(DateOnly holidayDate, bool recurring, DateOnly asOfDate)
    {
        if (!recurring)
        {
            return holidayDate <= asOfDate;
        }

        var occurrenceThisYear = new DateOnly(asOfDate.Year, holidayDate.Month, holidayDate.Day);
        return occurrenceThisYear <= asOfDate;
    }

    public static bool OccursOn(DateOnly holidayDate, bool recurring, DateOnly date)
    {
        if (!recurring)
        {
            return holidayDate == date;
        }

        return holidayDate.Month == date.Month && holidayDate.Day == date.Day;
    }

    public static bool SameScope(
        string holidayTypeA,
        Guid? companyIdA,
        string? stateCodeA,
        string? cityA,
        DateOnly holidayDateA,
        bool recurringA,
        string holidayTypeB,
        Guid? companyIdB,
        string? stateCodeB,
        string? cityB,
        DateOnly holidayDateB,
        bool recurringB)
    {
        if (!string.Equals(holidayTypeA, holidayTypeB, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (recurringA != recurringB)
        {
            return false;
        }

        if (recurringA)
        {
            if (holidayDateA.Month != holidayDateB.Month || holidayDateA.Day != holidayDateB.Day)
            {
                return false;
            }
        }
        else if (holidayDateA != holidayDateB)
        {
            return false;
        }

        if (HolidayTypes.IsNational(holidayTypeA))
        {
            return Nullable.Equals(companyIdA, companyIdB);
        }

        if (HolidayTypes.IsCompany(holidayTypeA))
        {
            return companyIdA == companyIdB;
        }

        if (HolidayTypes.IsState(holidayTypeA))
        {
            return Nullable.Equals(companyIdA, companyIdB)
                && string.Equals(NormalizeOptional(stateCodeA), NormalizeOptional(stateCodeB), StringComparison.OrdinalIgnoreCase);
        }

        return Nullable.Equals(companyIdA, companyIdB)
            && string.Equals(NormalizeOptional(stateCodeA), NormalizeOptional(stateCodeB), StringComparison.OrdinalIgnoreCase)
            && string.Equals(NormalizeOptional(cityA), NormalizeOptional(cityB), StringComparison.OrdinalIgnoreCase);
    }

    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string? NormalizeStateCode(string? stateCode)
    {
        var normalized = NormalizeOptional(stateCode);
        return normalized?.ToUpperInvariant();
    }

    public static string? NormalizeCity(string? city) => NormalizeOptional(city);
}
