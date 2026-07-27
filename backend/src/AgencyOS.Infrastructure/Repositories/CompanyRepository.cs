using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Companies.FirstOrDefaultAsync(company => company.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(
        CompanyQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Company> query = _context.Companies.AsNoTracking();

        if (!parameters.IncludeArchived)
        {
            query = query.Where(company => company.Status != CompanyStatus.Archived);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            query = query.Where(company => company.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(company =>
                company.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || company.CompanyCode.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var items = await query.ToListAsync(cancellationToken);
        return ApplyOrdering(items, parameters).ToList();
    }

    public async Task<bool> ExistsCodeAsync(
        string companyCode,
        Guid? excludeCompanyId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = companyCode.Trim();

        var items = await _context.Companies
            .AsNoTracking()
            .Where(company => !excludeCompanyId.HasValue || company.Id != excludeCompanyId.Value)
            .ToListAsync(cancellationToken);

        return items.Any(company => string.Equals(company.CompanyCode, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsNameAsync(
        string companyName,
        Guid? excludeCompanyId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = companyName.Trim();

        var items = await _context.Companies
            .AsNoTracking()
            .Where(company => !excludeCompanyId.HasValue || company.Id != excludeCompanyId.Value)
            .ToListAsync(cancellationToken);

        return items.Any(company => string.Equals(company.CompanyName, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);
        return company;
    }

    public async Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);
        return company;
    }

    private static IEnumerable<Company> ApplyOrdering(
        IEnumerable<Company> query,
        CompanyQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(company => company.Status).ThenBy(company => company.CompanyName)
                : query.OrderBy(company => company.Status).ThenBy(company => company.CompanyName);
        }

        if (string.Equals(parameters.OrderBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(company => company.CreatedAt)
                : query.OrderBy(company => company.CreatedAt);
        }

        if (string.Equals(parameters.OrderBy, "companyCode", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(company => company.CompanyCode)
                : query.OrderBy(company => company.CompanyCode);
        }

        return descending
            ? query.OrderByDescending(company => company.CompanyName)
            : query.OrderBy(company => company.CompanyName);
    }
}
