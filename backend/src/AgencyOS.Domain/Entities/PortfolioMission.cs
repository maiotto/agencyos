namespace AgencyOS.Domain.Entities;

/// <summary>
/// Mission association within a Portfolio (US-109 / BR-903..BR-905).
/// </summary>
public class PortfolioMission
{
    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid MissionId { get; private set; }

    public int Priority { get; private set; }

    public DateTimeOffset IncludedAt { get; private set; }

    private PortfolioMission()
    {
    }

    internal static PortfolioMission Create(
        Guid portfolioId,
        Guid missionId,
        int priority,
        DateTimeOffset includedAt)
    {
        if (portfolioId == Guid.Empty)
        {
            throw new InvalidOperationException("PortfolioId is mandatory for portfolio missions.");
        }

        if (missionId == Guid.Empty)
        {
            throw new InvalidOperationException("MissionId is mandatory for portfolio missions.");
        }

        if (priority < 1)
        {
            throw new InvalidOperationException("Mission Priority must be at least 1.");
        }

        return new PortfolioMission
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            MissionId = missionId,
            Priority = priority,
            IncludedAt = includedAt
        };
    }

    internal void UpdatePriority(int priority)
    {
        if (priority < 1)
        {
            throw new InvalidOperationException("Mission Priority must be at least 1.");
        }

        Priority = priority;
    }
}
