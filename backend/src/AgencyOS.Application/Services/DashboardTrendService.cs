using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Deterministic period-over-period trend calculation (US-403 / BR-2106).
/// </summary>
public class DashboardTrendService : IDashboardTrendService
{
    private const decimal FlatToleranceDeltaPercent = 0.01m;

    public TrendIndicator Calculate(int currentValue, int previousValue) =>
        Calculate((decimal)currentValue, (decimal)previousValue, currentValue, previousValue);

    public TrendIndicator Calculate(decimal currentValue, decimal previousValue) =>
        Calculate(currentValue, previousValue, (int)Math.Round(currentValue), (int)Math.Round(previousValue));

    private static TrendIndicator Calculate(
        decimal currentValue,
        decimal previousValue,
        int currentValueRounded,
        int previousValueRounded)
    {
        var deltaPercent = CalculateDeltaPercent(currentValue, previousValue);

        var direction = deltaPercent > FlatToleranceDeltaPercent
            ? "Up"
            : deltaPercent < -FlatToleranceDeltaPercent
                ? "Down"
                : "Flat";

        return new TrendIndicator
        {
            Direction = direction,
            DeltaPercent = decimal.Round(deltaPercent, 2, MidpointRounding.AwayFromZero),
            CurrentValue = currentValueRounded,
            PreviousValue = previousValueRounded
        };
    }

    private static decimal CalculateDeltaPercent(decimal currentValue, decimal previousValue)
    {
        if (previousValue == 0)
        {
            return currentValue == 0 ? 0m : 100m;
        }

        return (currentValue - previousValue) / previousValue * 100m;
    }
}
