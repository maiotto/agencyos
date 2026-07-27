using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Application.Tests;

public class CompanyRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsCompany()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "ACME1", name: "Acme One");

        await repository.AddAsync(company);
        var loaded = await repository.GetByIdAsync(company.Id);

        Assert.NotNull(loaded);
        Assert.Equal(company.CompanyCode, loaded!.CompanyCode);
        Assert.Equal(company.CompanyName, loaded.CompanyName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenMissing()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);

        var loaded = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(loaded);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesArchivedByDefault_BR2009()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var active = CreateCompany(code: "ACT1", name: "Active Co");
        var archived = CreateCompany(code: "ARC1", name: "Archived Co");
        archived.Archive(DateTimeOffset.UtcNow);
        await repository.AddAsync(active);
        await repository.AddAsync(archived);

        var results = await repository.GetAllAsync(new CompanyQueryParameters());

        Assert.Contains(results, company => company.Id == active.Id);
        Assert.DoesNotContain(results, company => company.Id == archived.Id);
    }

    [Fact]
    public async Task GetAllAsync_IncludesArchivedWhenRequested_BR2009()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var archived = CreateCompany(code: "ARC2", name: "Archived Co Two");
        archived.Archive(DateTimeOffset.UtcNow);
        await repository.AddAsync(archived);

        var results = await repository.GetAllAsync(new CompanyQueryParameters { IncludeArchived = true });

        Assert.Contains(results, company => company.Id == archived.Id);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatusAndSearch()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "SEARCH1", name: "Findable Company");
        await repository.AddAsync(company);

        var byStatus = await repository.GetAllAsync(
            new CompanyQueryParameters { Status = CompanyStatus.Active });
        var bySearch = await repository.GetAllAsync(
            new CompanyQueryParameters { Search = "Findable" });

        Assert.Contains(byStatus, item => item.Id == company.Id);
        Assert.Contains(bySearch, item => item.Id == company.Id);
    }

    [Fact]
    public async Task ExistsCodeAsync_IsCaseInsensitive_BR2002()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "UniqueCode9", name: "Unique Name Nine");
        await repository.AddAsync(company);

        var exists = await repository.ExistsCodeAsync("uniquecode9");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsCodeAsync_ExcludesGivenCompany()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "SelfCode", name: "Self Company");
        await repository.AddAsync(company);

        var exists = await repository.ExistsCodeAsync("SelfCode", excludeCompanyId: company.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsNameAsync_IsCaseInsensitive_BR2001()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "NameCode1", name: "Distinct Company Name");
        await repository.AddAsync(company);

        var exists = await repository.ExistsNameAsync("distinct company name");

        Assert.True(exists);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange()
    {
        await using var context = CreateContext();
        var repository = new CompanyRepository(context);
        var company = CreateCompany(code: "UpdateCode1", name: "Update Company");
        await repository.AddAsync(company);

        company.Deactivate(DateTimeOffset.UtcNow);
        await repository.UpdateAsync(company);

        var reloaded = await repository.GetByIdAsync(company.Id);
        Assert.Equal(CompanyStatus.Inactive, reloaded!.Status);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Company CreateCompany(string code, string name) =>
        Company.Create(
            Guid.NewGuid(),
            code,
            name,
            null,
            "UTC",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
}
