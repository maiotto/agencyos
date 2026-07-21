using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _context;

    public ClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        ClientQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(c => c.Status == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Industry))
        {
            query = query.Where(c => c.Segment == parameters.Industry);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.LegalName, $"%{parameters.CompanyName}%")
                || (c.TradeName != null && EF.Functions.ILike(c.TradeName, $"%{parameters.CompanyName}%")));
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

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithTaxIdentifierAsync(
        string taxIdentifier,
        Guid? excludeClientId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedTaxIdentifier = taxIdentifier.Trim();

        var query = _context.Clients
            .AsNoTracking()
            .Where(c => c.TaxId != null && c.TaxId == normalizedTaxIdentifier);

        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);
        return client;
    }

    public async Task<Client> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
        return client;
    }

    private static IQueryable<Client> ApplyOrdering(IQueryable<Client> query, ClientQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.LegalName)
                : query.OrderBy(c => c.CreatedAt).ThenBy(c => c.LegalName);
        }

        return descending
            ? query.OrderByDescending(c => c.LegalName).ThenByDescending(c => c.TradeName)
            : query.OrderBy(c => c.LegalName).ThenBy(c => c.TradeName);
    }
}
