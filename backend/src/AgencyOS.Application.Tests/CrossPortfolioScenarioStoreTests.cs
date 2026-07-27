using AgencyOS.Application.DTOs;
using AgencyOS.Application.Services;

namespace AgencyOS.Application.Tests;

public class CrossPortfolioScenarioStoreTests
{
    [Fact]
    public void Add_ThenGet_ReturnsStoredRecord()
    {
        var store = new CrossPortfolioScenarioStore();
        var record = CreateRecord();

        store.Add(record);
        var retrieved = store.Get(record.ScenarioId);

        Assert.NotNull(retrieved);
        Assert.Equal(record.ScenarioId, retrieved!.ScenarioId);
    }

    [Fact]
    public void Get_ReturnsNull_WhenScenarioDoesNotExist()
    {
        var store = new CrossPortfolioScenarioStore();

        var retrieved = store.Get(Guid.NewGuid());

        Assert.Null(retrieved);
    }

    [Fact]
    public void GetAll_FiltersByCompanyId()
    {
        var store = new CrossPortfolioScenarioStore();
        var companyOne = Guid.NewGuid();
        var companyTwo = Guid.NewGuid();

        store.Add(CreateRecord(companyId: companyOne));
        store.Add(CreateRecord(companyId: companyOne));
        store.Add(CreateRecord(companyId: companyTwo));

        var companyOneScenarios = store.GetAll(companyOne);
        var allScenarios = store.GetAll();

        Assert.Equal(2, companyOneScenarios.Count);
        Assert.Equal(3, allScenarios.Count);
    }

    [Fact]
    public void GetAll_OrdersByCreatedAtDescending()
    {
        var store = new CrossPortfolioScenarioStore();
        var companyId = Guid.NewGuid();

        var older = CreateRecord(companyId: companyId, createdAt: DateTimeOffset.UtcNow.AddHours(-2));
        var newer = CreateRecord(companyId: companyId, createdAt: DateTimeOffset.UtcNow);

        store.Add(older);
        store.Add(newer);

        var scenarios = store.GetAll(companyId);

        Assert.Equal(newer.ScenarioId, scenarios[0].ScenarioId);
        Assert.Equal(older.ScenarioId, scenarios[1].ScenarioId);
    }

    [Fact]
    public void Remove_DeletesScenario()
    {
        var store = new CrossPortfolioScenarioStore();
        var record = CreateRecord();
        store.Add(record);

        var removed = store.Remove(record.ScenarioId);
        var retrieved = store.Get(record.ScenarioId);

        Assert.True(removed);
        Assert.Null(retrieved);
    }

    [Fact]
    public void Remove_ReturnsFalse_WhenScenarioDoesNotExist()
    {
        var store = new CrossPortfolioScenarioStore();

        var removed = store.Remove(Guid.NewGuid());

        Assert.False(removed);
    }

    [Fact]
    public void Get_ReturnsNull_WhenScenarioHasExpired()
    {
        var store = new CrossPortfolioScenarioStore();
        var expiredRecord = CreateRecord(createdAt: DateTimeOffset.UtcNow.AddHours(-25));

        store.Add(expiredRecord);
        var retrieved = store.Get(expiredRecord.ScenarioId);

        Assert.Null(retrieved);
    }

    [Fact]
    public void GetAll_ExcludesExpiredScenarios()
    {
        var store = new CrossPortfolioScenarioStore();
        var companyId = Guid.NewGuid();
        var expiredRecord = CreateRecord(companyId: companyId, createdAt: DateTimeOffset.UtcNow.AddHours(-25));
        var freshRecord = CreateRecord(companyId: companyId, createdAt: DateTimeOffset.UtcNow);

        store.Add(expiredRecord);
        store.Add(freshRecord);

        var scenarios = store.GetAll(companyId);

        Assert.Single(scenarios);
        Assert.Equal(freshRecord.ScenarioId, scenarios[0].ScenarioId);
    }

    [Fact]
    public void Add_ThrowsArgumentNullException_WhenRecordIsNull()
    {
        var store = new CrossPortfolioScenarioStore();

        Assert.Throws<ArgumentNullException>(() => store.Add(null!));
    }

    private static CrossPortfolioScenarioRecord CreateRecord(
        Guid? companyId = null,
        DateTimeOffset? createdAt = null)
    {
        var scenarioId = Guid.NewGuid();
        var resolvedCreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        return new CrossPortfolioScenarioRecord
        {
            ScenarioId = scenarioId,
            CompanyId = companyId ?? Guid.NewGuid(),
            ScenarioName = "Test Scenario",
            CreatedAt = resolvedCreatedAt,
            Scenario = new CrossPortfolioScenarioResponse
            {
                ScenarioId = scenarioId,
                CreatedAt = resolvedCreatedAt
            }
        };
    }
}
