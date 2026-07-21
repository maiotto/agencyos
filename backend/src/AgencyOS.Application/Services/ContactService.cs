using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IContactRepository contactRepository,
        IClientRepository clientRepository,
        ILogger<ContactService> logger)
    {
        _contactRepository = contactRepository;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<ContactResponse>> GetPagedAsync(
        ContactQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _contactRepository.GetPagedAsync(parameters, cancellationToken);

        var pageSize = Math.Max(1, parameters.PageSize);
        var page = Math.Max(1, parameters.Page);

        return new PagedResponse<ContactResponse>
        {
            Items = items.Select(item => MapToResponse(item.Contact, item.ClientId)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ContactResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetClientContactOrThrowAsync(id, cancellationToken);
        return MapToResponse(result.Contact, result.ClientId);
    }

    public async Task<ContactResponse> CreateAsync(
        CreateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureClientExistsAsync(request.ClientId, cancellationToken);
        await EnsureEmailIsUniqueForClientAsync(request.Email, request.ClientId, null, cancellationToken);
        EnsurePrimaryContactRules(request.IsPrimaryContact, request.Status);

        var now = DateTimeOffset.UtcNow;

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = NormalizeOptionalText(request.LastName),
            JobTitle = NormalizeOptionalText(request.JobTitle),
            Email = request.Email.Trim(),
            Phone = NormalizeOptionalText(request.Phone),
            IsPrimary = request.IsPrimaryContact && IsActiveStatus(request.Status),
            CreatedAt = now,
            UpdatedAt = now
        };

        ContactInactiveState.ApplyStatus(contact, request.Status, request.Mobile);

        if (contact.IsPrimary)
        {
            await _contactRepository.ClearPrimaryForClientAsync(request.ClientId, null, cancellationToken);
        }

        var clientContact = new ClientContact
        {
            ClientId = request.ClientId,
            ContactId = contact.Id,
            CreatedAt = now
        };

        var created = await _contactRepository.AddAsync(contact, clientContact, cancellationToken);

        _logger.LogInformation(
            "Contact Created: {ContactId} for Client {ClientId} ({FirstName})",
            created.Id,
            request.ClientId,
            created.FirstName);

        return MapToResponse(created, request.ClientId);
    }

    public async Task<ContactResponse> UpdateAsync(
        Guid id,
        UpdateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await GetClientContactOrThrowAsync(id, cancellationToken);
        var contact = result.Contact;
        var clientId = result.ClientId;

        await EnsureEmailIsUniqueForClientAsync(request.Email, clientId, id, cancellationToken);
        EnsurePrimaryContactRules(request.IsPrimaryContact, request.Status);

        contact.FirstName = request.FirstName.Trim();
        contact.LastName = NormalizeOptionalText(request.LastName);
        contact.JobTitle = NormalizeOptionalText(request.JobTitle);
        contact.Email = request.Email.Trim();
        contact.Phone = NormalizeOptionalText(request.Phone);
        ContactInactiveState.ApplyStatus(contact, request.Status, request.Mobile);

        var shouldBePrimary = request.IsPrimaryContact && IsActiveStatus(request.Status);

        if (shouldBePrimary && !contact.IsPrimary)
        {
            await _contactRepository.ClearPrimaryForClientAsync(clientId, id, cancellationToken);
        }

        contact.IsPrimary = shouldBePrimary;
        contact.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contactRepository.UpdateAsync(contact, cancellationToken);

        _logger.LogInformation(
            "Contact Updated: {ContactId} for Client {ClientId} ({FirstName})",
            updated.Id,
            clientId,
            updated.FirstName);

        return MapToResponse(updated, clientId);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetClientContactOrThrowAsync(id, cancellationToken);
        var contact = result.Contact;

        if (ContactInactiveState.IsInactive(contact))
        {
            return;
        }

        ContactInactiveState.MarkInactive(contact);
        contact.UpdatedAt = DateTimeOffset.UtcNow;

        await _contactRepository.UpdateAsync(contact, cancellationToken);

        _logger.LogInformation(
            "Contact Deactivated: {ContactId} for Client {ClientId} ({FirstName})",
            contact.Id,
            result.ClientId,
            contact.FirstName);
    }

    public async Task<ContactResponse> SetPrimaryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetClientContactOrThrowAsync(id, cancellationToken);
        var contact = result.Contact;
        var clientId = result.ClientId;

        if (ContactInactiveState.IsInactive(contact))
        {
            throw new BusinessRuleException("Inactive Contacts cannot become Primary.");
        }

        if (contact.IsPrimary)
        {
            return MapToResponse(contact, clientId);
        }

        await _contactRepository.ClearPrimaryForClientAsync(clientId, id, cancellationToken);

        contact.IsPrimary = true;
        contact.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _contactRepository.UpdateAsync(contact, cancellationToken);

        _logger.LogInformation(
            "Primary Contact Changed: {ContactId} for Client {ClientId} ({FirstName})",
            updated.Id,
            clientId,
            updated.FirstName);

        return MapToResponse(updated, clientId);
    }

    private async Task EnsureClientExistsAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            throw new NotFoundException($"Client with id '{clientId}' was not found.");
        }
    }

    private async Task<(Contact Contact, Guid ClientId)> GetClientContactOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _contactRepository.GetClientContactByIdAsync(id, cancellationToken);

        if (result is null)
        {
            throw new NotFoundException($"Contact with id '{id}' was not found.");
        }

        return result.Value;
    }

    private async Task EnsureEmailIsUniqueForClientAsync(
        string email,
        Guid clientId,
        Guid? excludeContactId,
        CancellationToken cancellationToken)
    {
        if (await _contactRepository.ExistsWithEmailForClientAsync(
                email,
                clientId,
                excludeContactId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A Contact with email '{email.Trim()}' already exists for this Client.");
        }
    }

    private static void EnsurePrimaryContactRules(bool isPrimaryContact, string status)
    {
        if (isPrimaryContact && !IsActiveStatus(status))
        {
            throw new BusinessRuleException("Inactive Contacts cannot become Primary.");
        }
    }

    private static bool IsActiveStatus(string status)
    {
        return string.Equals(status, ContactStatus.Active, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static ContactResponse MapToResponse(Contact contact, Guid clientId)
    {
        return new ContactResponse
        {
            Id = contact.Id,
            ClientId = clientId,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            JobTitle = contact.JobTitle,
            Email = contact.Email ?? string.Empty,
            Phone = contact.Phone,
            Mobile = ContactInactiveState.GetMobile(contact),
            IsPrimaryContact = contact.IsPrimary,
            Status = ContactInactiveState.GetStatus(contact),
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}
