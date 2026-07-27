using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Compares two in-memory Cross-Portfolio Plan scenarios field-by-field (US-405).
/// </summary>
public interface ICrossPortfolioScenarioComparisonService
{
    ScenarioComparisonResponse Compare(
        CrossPortfolioScenarioRecord left,
        CrossPortfolioScenarioRecord right);
}
