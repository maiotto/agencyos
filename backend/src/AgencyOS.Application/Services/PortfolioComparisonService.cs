using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Side-by-side Portfolio comparison (US-404 / BR-2202). Builds a small, mission-scoped
/// Recommendation/Decision set for exactly the two Portfolios being compared (per-mission
/// repository calls, BR-2208) and reuses <see cref="PortfolioAnalyticsSnapshotBuilder"/> so the
/// same read-only projection logic backs both the Overview and Compare endpoints.
/// </summary>
public class PortfolioComparisonService : IPortfolioComparisonService
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly IDashboardHealthCalculationService _healthCalculationService;

    public PortfolioComparisonService(
        IPortfolioRepository portfolioRepository,
        IRecommendationRepository recommendationRepository,
        IDecisionRepository decisionRepository,
        IDashboardHealthCalculationService healthCalculationService)
    {
        _portfolioRepository = portfolioRepository;
        _recommendationRepository = recommendationRepository;
        _decisionRepository = decisionRepository;
        _healthCalculationService = healthCalculationService;
    }

    public async Task<PortfolioComparisonResponse> CompareAsync(
        Guid companyId,
        PortfolioCompareQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (parameters.LeftPortfolioId == Guid.Empty || parameters.RightPortfolioId == Guid.Empty)
        {
            throw new BusinessRuleException("LeftPortfolioId and RightPortfolioId are required.");
        }

        if (parameters.LeftPortfolioId == parameters.RightPortfolioId)
        {
            throw new BusinessRuleException("LeftPortfolioId and RightPortfolioId must be different.");
        }

        var left = await GetPortfolioForCompanyAsync(parameters.LeftPortfolioId, companyId, cancellationToken);
        var right = await GetPortfolioForCompanyAsync(parameters.RightPortfolioId, companyId, cancellationToken);

        var missionIds = left.Missions.Select(mission => mission.MissionId)
            .Concat(right.Missions.Select(mission => mission.MissionId))
            .Distinct()
            .ToList();

        var recommendations = new List<Recommendation>();
        var decisions = new List<Decision>();

        foreach (var missionId in missionIds)
        {
            var missionRecommendations = await _recommendationRepository.GetByMissionIdAsync(
                missionId,
                new RecommendationQueryParameters
                {
                    CompanyId = companyId,
                    GeneratedFrom = parameters.From,
                    GeneratedTo = parameters.To,
                    IncludeArchived = true
                },
                cancellationToken);
            recommendations.AddRange(missionRecommendations);

            var missionDecisions = await _decisionRepository.GetAllAsync(
                new DecisionQueryParameters
                {
                    CompanyId = companyId,
                    MissionId = missionId,
                    DecisionFrom = parameters.From,
                    DecisionTo = parameters.To
                },
                cancellationToken);
            decisions.AddRange(missionDecisions);
        }

        var leftSnapshot = PortfolioAnalyticsSnapshotBuilder.Build(left, recommendations, decisions);
        var rightSnapshot = PortfolioAnalyticsSnapshotBuilder.Build(right, recommendations, decisions);

        var leftSide = ToSide(leftSnapshot);
        var rightSide = ToSide(rightSnapshot);

        return new PortfolioComparisonResponse
        {
            CompanyId = companyId,
            GeneratedAt = DateTimeOffset.UtcNow,
            Left = leftSide,
            Right = rightSide,
            FieldDiffs = BuildFieldDiffs(leftSide, rightSide)
        };
    }

    private async Task<Portfolio> GetPortfolioForCompanyAsync(
        Guid portfolioId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(portfolioId, cancellationToken);
        if (portfolio is null)
        {
            throw new NotFoundException($"Portfolio with id '{portfolioId}' was not found.");
        }

        if (portfolio.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                $"Portfolio '{portfolioId}' does not belong to the resolved Company.");
        }

        return portfolio;
    }

    private PortfolioComparisonSideResponse ToSide(PortfolioAnalyticsSnapshot snapshot) => new()
    {
        PortfolioId = snapshot.PortfolioId,
        Name = snapshot.Name,
        Status = snapshot.Status,
        Health = _healthCalculationService.CalculateOverall([snapshot.PortfolioHealth]),
        MissionCount = snapshot.MissionCount,
        UtilizationPercentage = snapshot.UtilizationPercentage,
        WorkloadPercentage = snapshot.WorkloadPercentage,
        RecommendationCount = snapshot.RecommendationCount,
        AverageRecommendationScore = snapshot.AverageRecommendationScore,
        DecisionCount = snapshot.DecisionCount,
        CompletedDecisionCount = snapshot.CompletedDecisionCount,
        CancelledDecisionCount = snapshot.CancelledDecisionCount,
        DrillDownPath = snapshot.DrillDownPath
    };

    private static IReadOnlyList<PortfolioComparisonFieldDiff> BuildFieldDiffs(
        PortfolioComparisonSideResponse left,
        PortfolioComparisonSideResponse right) =>
    [
        Diff("Status", left.Status, right.Status),
        Diff("Health", left.Health.Status, right.Health.Status),
        DiffDecimal("MissionCount", left.MissionCount, right.MissionCount),
        DiffDecimal("UtilizationPercentage", left.UtilizationPercentage, right.UtilizationPercentage),
        DiffDecimal("WorkloadPercentage", left.WorkloadPercentage, right.WorkloadPercentage),
        DiffDecimal("RecommendationCount", left.RecommendationCount, right.RecommendationCount),
        DiffDecimal("AverageRecommendationScore", left.AverageRecommendationScore, right.AverageRecommendationScore),
        DiffDecimal("DecisionCount", left.DecisionCount, right.DecisionCount),
        DiffDecimal("CompletedDecisionCount", left.CompletedDecisionCount, right.CompletedDecisionCount),
        DiffDecimal("CancelledDecisionCount", left.CancelledDecisionCount, right.CancelledDecisionCount)
    ];

    private static PortfolioComparisonFieldDiff Diff(string field, string leftValue, string rightValue) => new()
    {
        Field = field,
        LeftValue = leftValue,
        RightValue = rightValue,
        Delta = null
    };

    private static PortfolioComparisonFieldDiff DiffDecimal(string field, decimal? leftValue, decimal? rightValue) => new()
    {
        Field = field,
        LeftValue = leftValue?.ToString("0.##"),
        RightValue = rightValue?.ToString("0.##"),
        Delta = leftValue.HasValue && rightValue.HasValue
            ? decimal.Round(rightValue.Value - leftValue.Value, 2, MidpointRounding.AwayFromZero)
            : null
    };
}
