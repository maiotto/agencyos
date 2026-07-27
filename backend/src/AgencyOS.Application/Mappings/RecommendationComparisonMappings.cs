using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Mappings;

public static class RecommendationComparisonMappings
{
    public static RecommendationComparisonResponse ToResponse(RecommendationComparison comparison)
    {
        var differences = comparison.Differences
            .Select(difference => new RecommendationComparisonFieldDiffResponse
            {
                Section = difference.Section,
                Path = difference.Path,
                LeftValue = difference.LeftValue,
                RightValue = difference.RightValue,
                Changed = difference.Changed
            })
            .ToList();

        var sections = differences
            .GroupBy(difference => difference.Section)
            .Select(group => new RecommendationComparisonSectionResponse
            {
                Section = group.Key,
                HasDifferences = group.Any(difference => difference.Changed),
                Fields = group.ToList()
            })
            .OrderBy(section => SectionOrder(section.Section))
            .ToList();

        return new RecommendationComparisonResponse
        {
            Left = MapHistory(comparison.Left),
            Right = MapHistory(comparison.Right),
            HasDifferences = comparison.HasDifferences,
            ScoreDelta = comparison.ScoreDelta,
            RankDelta = comparison.RankDelta,
            VersionDelta = comparison.VersionDelta,
            Differences = differences.Where(difference => difference.Changed).ToList(),
            Sections = sections
        };
    }

    private static int SectionOrder(string section) =>
        section switch
        {
            RecommendationComparisonSections.Metadata => 0,
            RecommendationComparisonSections.DeliveryStrategy => 1,
            RecommendationComparisonSections.Capacity => 2,
            RecommendationComparisonSections.Workload => 3,
            RecommendationComparisonSections.Payload => 4,
            RecommendationComparisonSections.Workflow => 5,
            _ => 99
        };

    private static RecommendationHistoryResponse MapHistory(RecommendationHistory history) =>
        new()
        {
            Id = history.Id,
            RecommendationId = history.RecommendationId,
            RecommendationNumber = history.RecommendationNumber,
            RecommendationVersion = history.RecommendationVersion,
            CompanyId = history.CompanyId,
            MissionId = history.MissionId,
            ContractId = history.ContractId,
            DeliveryStrategyId = history.DeliveryStrategyId,
            Title = history.Title,
            Summary = history.Summary,
            EventType = history.EventType,
            RecommendationStatus = history.RecommendationStatus,
            WorkflowStatus = history.WorkflowStatus,
            WorkflowId = history.WorkflowId,
            Approver = history.Approver,
            ApprovalDate = history.ApprovalDate,
            ApprovalComment = history.ApprovalComment,
            Score = history.Score,
            Rank = history.Rank,
            PlanningTemplateId = history.PlanningTemplateId,
            DecisionEngineVersion = history.DecisionEngineVersion,
            CapacitySnapshot = history.CapacitySnapshot,
            WorkloadSnapshot = history.WorkloadSnapshot,
            RecommendationPayload = history.RecommendationPayload,
            CreatedBy = history.CreatedBy,
            CreatedAt = history.CreatedAt
        };
}
