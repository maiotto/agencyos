using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<(Contact Contact, Guid ClientId)> Items, int TotalCount)> GetPagedAsync(
        ContactQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ClientContacts
            .AsNoTracking()
            .Include(cc => cc.Contact)
            .Include(cc => cc.Client)
            .AsQueryable();

        if (parameters.ClientId.HasValue)
        {
            query = query.Where(cc => cc.ClientId == parameters.ClientId.Value);
        }

        if (parameters.IsPrimaryContact.HasValue)
        {
            query = query.Where(cc => cc.Contact.IsPrimary == parameters.IsPrimaryContact.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            if (string.Equals(status, ContactStatus.Inactive, StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(cc =>
                    cc.Contact.Mobile != null
                    && (cc.Contact.Mobile == "__INACTIVE__"
                        || cc.Contact.Mobile.StartsWith("__I|")));
            }
            else if (string.Equals(status, ContactStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(cc =>
                    cc.Contact.Mobile == null
                    || (cc.Contact.Mobile != "__INACTIVE__"
                        && !cc.Contact.Mobile.StartsWith("__I|")));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        var ordered = ApplyOrdering(query, parameters);

        var links = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = links
            .Select(link => (link.Contact, link.ClientId))
            .ToList();

        return (items, totalCount);
    }

    public async Task<(Contact Contact, Guid ClientId)?> GetClientContactByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var link = await _context.ClientContacts
            .Include(cc => cc.Contact)
            .FirstOrDefaultAsync(cc => cc.ContactId == id, cancellationToken);

        if (link is null)
        {
            return null;
        }

        return (link.Contact, link.ClientId);
    }

    public async Task<bool> ExistsWithEmailForClientAsync(
        string email,
        Guid clientId,
        Guid? excludeContactId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var query = _context.ClientContacts
            .AsNoTracking()
            .Where(cc => cc.ClientId == clientId
                && cc.Contact.Email != null
                && cc.Contact.Email.ToLower() == normalizedEmail);

        if (excludeContactId.HasValue)
        {
            query = query.Where(cc => cc.ContactId != excludeContactId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Contact> AddAsync(
        Contact contact,
        ClientContact clientContact,
        CancellationToken cancellationToken = default)
    {
        _context.Contacts.Add(contact);
        _context.ClientContacts.Add(clientContact);
        await _context.SaveChangesAsync(cancellationToken);
        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync(cancellationToken);
        return contact;
    }

    public async Task ClearPrimaryForClientAsync(
        Guid clientId,
        Guid? excludeContactId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ClientContacts
            .Include(cc => cc.Contact)
            .Where(cc => cc.ClientId == clientId && cc.Contact.IsPrimary);

        if (excludeContactId.HasValue)
        {
            query = query.Where(cc => cc.ContactId != excludeContactId.Value);
        }

        var links = await query.ToListAsync(cancellationToken);

        foreach (var link in links)
        {
            link.Contact.IsPrimary = false;
            link.Contact.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (links.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static IQueryable<ClientContact> ApplyOrdering(
        IQueryable<ClientContact> links,
        ContactQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "lastName", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? links.OrderByDescending(link => link.Contact.LastName).ThenBy(link => link.Contact.FirstName)
                : links.OrderBy(link => link.Contact.LastName).ThenBy(link => link.Contact.FirstName);
        }

        if (string.Equals(parameters.OrderBy, "client", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? links.OrderByDescending(link => link.Client.LegalName).ThenBy(link => link.Contact.FirstName)
                : links.OrderBy(link => link.Client.LegalName).ThenBy(link => link.Contact.FirstName);
        }

        return descending
            ? links.OrderByDescending(link => link.Contact.FirstName).ThenBy(link => link.Contact.LastName)
            : links.OrderBy(link => link.Contact.FirstName).ThenBy(link => link.Contact.LastName);
    }
}
