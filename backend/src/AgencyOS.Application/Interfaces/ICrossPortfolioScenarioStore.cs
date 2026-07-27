using AgencyOS.Application.DTOs;

namespace AgencyOS.Application.Interfaces;

/// <summary>
/// In-memory, thread-safe store for temporary Cross-Portfolio Plan scenarios (US-405 / DEC-405-001).
/// Registered as a singleton and backed by a <c>ConcurrentDictionary</c> — never persisted to a
/// database table. Scenarios only live for the current process lifetime; the durable record of a
/// simulation is the Audit trail (BR-2309).
/// </summary>
public interface ICrossPortfolioScenarioStore
{
    void Add(CrossPortfolioScenarioRecord record);

    CrossPortfolioScenarioRecord? Get(Guid scenarioId);

    IReadOnlyList<CrossPortfolioScenarioRecord> GetAll(Guid? companyId = null);

    bool Remove(Guid scenarioId);
}
