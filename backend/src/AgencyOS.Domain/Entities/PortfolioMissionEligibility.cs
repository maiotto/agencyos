namespace AgencyOS.Domain.Entities;

/// <summary>
/// Missions eligible for Portfolio association (US-109 / BR-904).
/// Aligns with seed mission_status PLANNED and IN_PROGRESS.
/// </summary>
public static class PortfolioMissionEligibility
{
    public static readonly Guid PlannedStatusId =
        Guid.Parse("11111111-1111-4111-8121-000000000002");

    public static readonly Guid InProgressStatusId =
        Guid.Parse("11111111-1111-4111-8121-000000000003");

    public static bool IsActiveMissionStatus(Guid missionStatusId) =>
        missionStatusId == PlannedStatusId || missionStatusId == InProgressStatusId;
}
