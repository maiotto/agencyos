using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// Produces advisory capacity/workload balancing recommendations across a set of Portfolio
/// participations (US-405 / BR-2306). Purely informational — never executes reallocation, AI
/// optimization, financial optimization, or hiring changes. Portfolio Mission priorities are
/// always preserved as supplied and never reordered.
/// </summary>
public interface ICrossPortfolioBalancingService
{
    IReadOnlyList<BalancingRecommendationResponse> BuildRecommendations(
        IReadOnlyList<PortfolioParticipationResponse> participations);
}
