using System.Collections.Concurrent;
using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;

namespace AgencyOS.Application.Services;

/// <summary>
/// Singleton, thread-safe in-memory store for temporary Cross-Portfolio Plan scenarios
/// (US-405 / DEC-405-001). Backed by a <c>ConcurrentDictionary</c> — no database table.
/// Applies an optional TTL and a maximum-size eviction so the store never grows unbounded
/// across a long-running process; the durable record of a simulation remains the Audit
/// trail (BR-2309), never this store.
/// </summary>
public class CrossPortfolioScenarioStore : ICrossPortfolioScenarioStore
{
    private const int MaxScenarios = 500;
    private static readonly TimeSpan ScenarioTimeToLive = TimeSpan.FromHours(24);

    private readonly ConcurrentDictionary<Guid, CrossPortfolioScenarioRecord> _scenarios = new();

    public void Add(CrossPortfolioScenarioRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        _scenarios[record.ScenarioId] = record;
        EvictExpiredAndExcess();
    }

    public CrossPortfolioScenarioRecord? Get(Guid scenarioId)
    {
        if (_scenarios.TryGetValue(scenarioId, out var record) && !IsExpired(record))
        {
            return record;
        }

        return null;
    }

    public IReadOnlyList<CrossPortfolioScenarioRecord> GetAll(Guid? companyId = null) =>
        _scenarios.Values
            .Where(record => !IsExpired(record))
            .Where(record => companyId is null || record.CompanyId == companyId)
            .OrderByDescending(record => record.CreatedAt)
            .ToList();

    public bool Remove(Guid scenarioId) => _scenarios.TryRemove(scenarioId, out _);

    private static bool IsExpired(CrossPortfolioScenarioRecord record) =>
        DateTimeOffset.UtcNow - record.CreatedAt > ScenarioTimeToLive;

    private void EvictExpiredAndExcess()
    {
        foreach (var pair in _scenarios)
        {
            if (IsExpired(pair.Value))
            {
                _scenarios.TryRemove(pair.Key, out _);
            }
        }

        var overflow = _scenarios.Count - MaxScenarios;
        if (overflow <= 0)
        {
            return;
        }

        var oldestKeys = _scenarios
            .OrderBy(pair => pair.Value.CreatedAt)
            .Take(overflow)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var key in oldestKeys)
        {
            _scenarios.TryRemove(key, out _);
        }
    }
}
