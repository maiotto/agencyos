namespace AgencyOS.Domain.Entities;

/// <summary>
/// Immutable Capacity History aggregate (US-106 / BR-601..BR-609).
/// No update or delete operations.
/// </summary>
public class CapacityHistory
{
    public Guid Id { get; private set; }

    public Guid ExecutionResourceId { get; private set; }

    public Guid CompanyId { get; private set; }

    public DateTimeOffset CalculationDate { get; private set; }

    public DateOnly PeriodStart { get; private set; }

    public DateOnly PeriodEnd { get; private set; }

    public int WorkingDays { get; private set; }

    public int HolidayDays { get; private set; }

    public int AvailableDays { get; private set; }

    public decimal ConfiguredHours { get; private set; }

    public decimal AvailableHours { get; private set; }

    public decimal CapacityHours { get; private set; }

    public decimal AllocatedHours { get; private set; }

    public decimal UtilizationPercentage { get; private set; }

    public string CalculationVersion { get; private set; } = string.Empty;

    /// <summary>
    /// JSON snapshot of operational inputs and day breakdown (BR-609).
    /// </summary>
    public string OperationalInputsJson { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private CapacityHistory()
    {
    }

    public static CapacityHistory Create(
        Guid executionResourceId,
        Guid companyId,
        DateOnly periodStart,
        DateOnly periodEnd,
        int workingDays,
        int holidayDays,
        int availableDays,
        decimal configuredHours,
        decimal availableHours,
        decimal capacityHours,
        decimal allocatedHours,
        decimal utilizationPercentage,
        string calculationVersion,
        string operationalInputsJson,
        DateTimeOffset calculationDate)
    {
        if (executionResourceId == Guid.Empty)
        {
            throw new InvalidOperationException("Execution Resource is mandatory for capacity history.");
        }

        if (companyId == Guid.Empty)
        {
            throw new InvalidOperationException("CompanyId is mandatory for capacity history.");
        }

        if (periodEnd < periodStart)
        {
            throw new InvalidOperationException("PeriodEnd cannot be earlier than PeriodStart.");
        }

        if (string.IsNullOrWhiteSpace(calculationVersion))
        {
            throw new InvalidOperationException("CalculationVersion is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(operationalInputsJson))
        {
            throw new InvalidOperationException("Operational inputs snapshot is mandatory.");
        }

        if (workingDays < 0 || holidayDays < 0 || availableDays < 0)
        {
            throw new InvalidOperationException("Day counts cannot be negative.");
        }

        return new CapacityHistory
        {
            Id = Guid.NewGuid(),
            ExecutionResourceId = executionResourceId,
            CompanyId = companyId,
            CalculationDate = calculationDate,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            WorkingDays = workingDays,
            HolidayDays = holidayDays,
            AvailableDays = availableDays,
            ConfiguredHours = configuredHours,
            AvailableHours = availableHours,
            CapacityHours = capacityHours,
            AllocatedHours = allocatedHours,
            UtilizationPercentage = utilizationPercentage,
            CalculationVersion = calculationVersion.Trim(),
            OperationalInputsJson = operationalInputsJson,
            CreatedAt = calculationDate
        };
    }
}
