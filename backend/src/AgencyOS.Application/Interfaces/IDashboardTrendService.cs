namespace AgencyOS.Application.Interfaces;

using AgencyOS.Application.DTOs;

/// <summary>
/// Computes simple period-over-period trend indicators (US-403 / BR-2106).
/// Pure calculation — never queries a repository directly.
/// </summary>
public interface IDashboardTrendService
{
    TrendIndicator Calculate(int currentValue, int previousValue);

    TrendIndicator Calculate(decimal currentValue, decimal previousValue);
}
