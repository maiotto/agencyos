using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly ApplicationDbContext _context;

    public LeadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetPagedAsync(
        LeadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Include(l => l.LeadContacts)
            .ThenInclude(lc => lc.Contact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(l => l.Status == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Company))
        {
            query = query.Where(l => EF.Functions.ILike(l.CompanyName, $"%{parameters.Company}%"));
        }

        if (parameters.AssignedUserId.HasValue)
        {
            query = query.Where(l => l.OwnerId == parameters.AssignedUserId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var items = await query
            .OrderBy(l => l.CompanyName)
            .ThenBy(l => l.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .Include(l => l.LeadContacts)
            .ThenInclude(lc => lc.Contact)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Guid?> GetClientIdByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.Clients
            .AsNoTracking()
            .Where(c => c.LeadId == leadId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> GetClientIdsByLeadIdsAsync(
        IReadOnlyCollection<Guid> leadIds,
        CancellationToken cancellationToken = default)
    {
        if (leadIds.Count == 0)
        {
            return new Dictionary<Guid, Guid>();
        }

        return await _context.Clients
            .AsNoTracking()
            .Where(c => c.LeadId.HasValue && leadIds.Contains(c.LeadId.Value))
            .ToDictionaryAsync(c => c.LeadId!.Value, c => c.Id, cancellationToken);
    }

    public async Task<bool> ExistsActiveLeadWithEmailAsync(
        string email,
        Guid? excludeLeadId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var query = _context.LeadContacts
            .AsNoTracking()
            .Where(lc => lc.Contact.Email != null
                && lc.Contact.Email.ToLower() == normalizedEmail
                && lc.Lead.Status != LeadStatus.Archived
                && lc.Lead.Status != LeadStatus.Converted);

        if (excludeLeadId.HasValue)
        {
            query = query.Where(lc => lc.LeadId != excludeLeadId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task<Lead> UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task<Client> ConvertLeadAsync(
        Lead lead,
        Client client,
        CancellationToken cancellationToken = default)
    {
        _context.Clients.Add(client);
        _context.Leads.Update(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return client;
    }
}
