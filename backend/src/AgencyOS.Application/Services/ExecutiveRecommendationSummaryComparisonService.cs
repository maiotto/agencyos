using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public class ExecutiveRecommendationSummaryComparisonService
    : IExecutiveRecommendationSummaryComparisonService
{
    public ExecutiveRecommendationSummaryComparisonResponse Compare(
        ExecutiveRecommendationSummary summary,
        Recommendation recommendation)
    {
        var comparison = summary.Compare(recommendation);
        return new ExecutiveRecommendationSummaryComparisonResponse
        {
            Summary = ExecutiveRecommendationSummaryService.MapToResponse(summary),
            RecommendationId = recommendation.Id,
            RecommendationTitle = recommendation.Title,
            RecommendationScore = recommendation.Score,
            RecommendationRank = recommendation.Rank,
            RecommendationVersion = recommendation.Version,
            HasDifferences = comparison.HasDifferences,
            Differences = comparison.Differences
                .Select(difference => new ExecutiveRecommendationSummaryComparisonFieldResponse
                {
                    Path = difference.Path,
                    LeftValue = difference.LeftValue,
                    RightValue = difference.RightValue,
                    Changed = difference.Changed
                })
                .ToList()
        };
    }
}
