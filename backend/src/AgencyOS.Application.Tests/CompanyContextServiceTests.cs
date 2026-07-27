using AgencyOS.Application.Audit;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Moq;

namespace AgencyOS.Application.Tests;

public class CompanyContextServiceTests
{
    private readonly Mock<ICompanyRepository> _repository = new();
    private readonly CompanyContext _context = new();

    private CompanyContextService CreateService() =>
        new(
            _repository.Object,
            _context,
            new NoOpNotificationGenerationService(),
            new NullAuditContext());

    [Fact]
    public async Task SelectAsync_ThrowsNotFoundWhenCompanyMissing()
    {
        var companyId = Guid.NewGuid();
        _repository
            .Setup(repository => repository.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().SelectAsync(companyId));
    }

    [Fact]
    public async Task SelectAsync_ThrowsBusinessRuleWhenCompanyNotActive_BR2003()
    {
        var company = CreateCompany();
        company.Deactivate(DateTimeOffset.UtcNow);

        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await Assert.ThrowsAsync<BusinessRuleException>(() => CreateService().SelectAsync(company.Id));
    }

    [Fact]
    public async Task SelectAsync_PopulatesContextWhenActive_BR2003()
    {
        var company = CreateCompany();
        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await CreateService().SelectAsync(company.Id);

        Assert.True(_context.IsSelected);
        Assert.Equal(company.Id, _context.CompanyId);
        Assert.Equal(company.CompanyCode, _context.CompanyCode);
        Assert.Equal(company.CompanyName, _context.CompanyName);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsSelectedCompanyWhenPresent()
    {
        var company = CreateCompany();
        _context.CompanyId = company.Id;
        _context.IsSelected = true;
        _repository
            .Setup(repository => repository.GetByIdAsync(company.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var response = await CreateService().GetActiveAsync();

        Assert.Equal(company.Id, response.Id);
    }

    [Fact]
    public async Task GetActiveAsync_FallsBackToDefaultCompanyWhenNoneSelected()
    {
        var defaultCompany = CreateCompany();
        _repository
            .Setup(repository => repository.GetByIdAsync(
                AgencyOSCompanies.DefaultCompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultCompany);

        var response = await CreateService().GetActiveAsync();

        Assert.Equal(defaultCompany.Id, response.Id);
    }

    [Fact]
    public async Task GetActiveAsync_ThrowsNotFoundWhenNoCompanyAvailable()
    {
        _repository
            .Setup(repository => repository.GetByIdAsync(
                AgencyOSCompanies.DefaultCompanyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetActiveAsync());
    }

    [Fact]
    public void Clear_ResetsContext()
    {
        _context.CompanyId = Guid.NewGuid();
        _context.CompanyCode = "CODE";
        _context.CompanyName = "Name";
        _context.IsSelected = true;

        CreateService().Clear();

        Assert.Null(_context.CompanyId);
        Assert.Null(_context.CompanyCode);
        Assert.Null(_context.CompanyName);
        Assert.False(_context.IsSelected);
    }

    private static Company CreateCompany() =>
        Company.Create(
            Guid.NewGuid(),
            "CODE" + Guid.NewGuid().ToString("N")[..6],
            "Test Company",
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
