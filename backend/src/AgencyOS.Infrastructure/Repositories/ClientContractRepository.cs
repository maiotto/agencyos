using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ClientContractRepository : IClientContractRepository
{
    private readonly ApplicationDbContext _context;

    public ClientContractRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<ClientContract> Items, int TotalCount)> GetPagedAsync(
        ContractQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ClientContracts
            .AsNoTracking()
            .Include(c => c.Client)
            .AsQueryable();

        if (parameters.ClientId.HasValue)
        {
            query = query.Where(c => c.ClientId == parameters.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(c => c.Status == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ContractType))
        {
            query = query.Where(c => c.BillingModel == parameters.ContractType);
        }

        if (parameters.StartDate.HasValue)
        {
            query = query.Where(c => c.StartDate >= parameters.StartDate.Value);
        }

        if (parameters.EndDate.HasValue)
        {
            query = query.Where(c => c.EndDate <= parameters.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        query = ApplyOrdering(query, parameters);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ClientContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ClientContracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithContractCodeAsync(
        string contractCode,
        Guid? excludeContractId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedContractCode = contractCode.Trim();

        var query = _context.ClientContracts
            .AsNoTracking()
            .Where(c => c.ContractNumber == normalizedContractCode);

        if (excludeContractId.HasValue)
        {
            query = query.Where(c => c.Id != excludeContractId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<ClientContract> AddAsync(ClientContract contract, CancellationToken cancellationToken = default)
    {
        _context.ClientContracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task<ClientContract> UpdateAsync(ClientContract contract, CancellationToken cancellationToken = default)
    {
        _context.ClientContracts.Update(contract);
        await _context.SaveChangesAsync(cancellationToken);
        return contract;
    }

    private static IQueryable<ClientContract> ApplyOrdering(
        IQueryable<ClientContract> query,
        ContractQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "client", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(c => c.Client.LegalName).ThenBy(c => c.Name)
                : query.OrderBy(c => c.Client.LegalName).ThenBy(c => c.Name);
        }

        if (string.Equals(parameters.OrderBy, "startDate", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(c => c.StartDate).ThenBy(c => c.Name)
                : query.OrderBy(c => c.StartDate).ThenBy(c => c.Name);
        }

        if (string.Equals(parameters.OrderBy, "estimatedValue", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(c => c.Value).ThenBy(c => c.Name)
                : query.OrderBy(c => c.Value).ThenBy(c => c.Name);
        }

        return descending
            ? query.OrderByDescending(c => c.Name).ThenBy(c => c.ContractNumber)
            : query.OrderBy(c => c.Name).ThenBy(c => c.ContractNumber);
    }
}
