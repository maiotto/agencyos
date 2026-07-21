using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class LeadService : ILeadService
{
    private const string ClientStatusActive = "Active";

    private readonly ILeadRepository _leadRepository;
    private readonly ILogger<LeadService> _logger;

    public LeadService(ILeadRepository leadRepository, ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<LeadResponse>> GetPagedAsync(
        LeadQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _leadRepository.GetPagedAsync(parameters, cancellationToken);

        var clientIds = await _leadRepository.GetClientIdsByLeadIdsAsync(
            items.Select(lead => lead.Id).ToList(),
            cancellationToken);

        var responses = items
            .Select(lead => MapToResponse(
                lead,
                clientIds.TryGetValue(lead.Id, out var clientId) ? clientId : null))
            .ToList();

        var pageSize = Math.Max(1, parameters.PageSize);
        var page = Math.Max(1, parameters.Page);

        return new PagedResponse<LeadResponse>
        {
            Items = responses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<LeadResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadOrThrowAsync(id, cancellationToken);
        var clientId = await _leadRepository.GetClientIdByLeadIdAsync(id, cancellationToken);
        return MapToResponse(lead, clientId);
    }

    public async Task<LeadResponse> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureEmailIsUniqueAsync(request.Email, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Code = GenerateLeadCode(),
            CompanyName = request.CompanyName,
            TradeName = request.LeadName,
            Website = request.Website,
            Segment = request.Segment,
            Source = request.Source,
            Status = request.Status,
            EstimatedRevenue = request.EstimatedContractValue,
            OwnerId = request.AssignedUserId,
            Notes = request.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (!string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(request.Phone))
        {
            var contact = CreatePrimaryContact(request.LeadName, request.Email, request.Phone, now);
            lead.LeadContacts.Add(new LeadContact
            {
                LeadId = lead.Id,
                ContactId = contact.Id,
                CreatedAt = now,
                Contact = contact
            });
        }

        var created = await _leadRepository.AddAsync(lead, cancellationToken);

        _logger.LogInformation("Lead Created: {LeadId} ({LeadCode})", created.Id, created.Code);

        return MapToResponse(created, null);
    }

    public async Task<LeadResponse> UpdateAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadOrThrowAsync(id, cancellationToken);

        EnsureLeadCanBeEdited(lead);
        ValidateStatusTransition(lead.Status, request.Status);
        await EnsureEmailIsUniqueAsync(request.Email, id, cancellationToken);

        lead.CompanyName = request.CompanyName;
        lead.TradeName = request.LeadName;
        lead.Website = request.Website;
        lead.Segment = request.Segment;
        lead.Source = request.Source;
        lead.Status = request.Status;
        lead.EstimatedRevenue = request.EstimatedContractValue;
        lead.OwnerId = request.AssignedUserId;
        lead.Notes = request.Notes;
        lead.UpdatedAt = DateTimeOffset.UtcNow;

        UpdatePrimaryContact(lead, request.LeadName, request.Email, request.Phone);

        var updated = await _leadRepository.UpdateAsync(lead, cancellationToken);
        var clientId = await _leadRepository.GetClientIdByLeadIdAsync(id, cancellationToken);

        _logger.LogInformation("Lead Updated: {LeadId} ({LeadCode})", updated.Id, updated.Code);

        return MapToResponse(updated, clientId);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadOrThrowAsync(id, cancellationToken);

        if (string.Equals(lead.Status, LeadStatus.Archived, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        lead.Status = LeadStatus.Archived;
        lead.UpdatedAt = DateTimeOffset.UtcNow;

        await _leadRepository.UpdateAsync(lead, cancellationToken);

        _logger.LogInformation("Lead Archived: {LeadId} ({LeadCode})", lead.Id, lead.Code);
    }

    public async Task<ConvertLeadResponse> ConvertAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await GetLeadOrThrowAsync(id, cancellationToken);

        if (!LeadStatus.Convertible.Contains(lead.Status))
        {
            throw new BusinessRuleException("Only active Leads can be converted.");
        }

        var existingClientId = await _leadRepository.GetClientIdByLeadIdAsync(id, cancellationToken);
        if (existingClientId.HasValue)
        {
            throw new BusinessRuleException("Lead has already been converted.");
        }

        var now = DateTimeOffset.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            LegalName = lead.CompanyName,
            TradeName = lead.TradeName,
            Website = lead.Website,
            Segment = lead.Segment,
            Status = ClientStatusActive,
            AccountOwner = lead.OwnerId,
            CreatedAt = now,
            UpdatedAt = now
        };

        lead.Status = LeadStatus.Converted;
        lead.UpdatedAt = now;

        await _leadRepository.ConvertLeadAsync(lead, client, cancellationToken);

        _logger.LogInformation(
            "Lead Converted: {LeadId} ({LeadCode}) to Client {ClientId}",
            lead.Id,
            lead.Code,
            client.Id);

        return new ConvertLeadResponse
        {
            Lead = MapToResponse(lead, client.Id),
            ClientId = client.Id
        };
    }

    private async Task<Lead> GetLeadOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(id, cancellationToken);

        if (lead is null)
        {
            throw new NotFoundException($"Lead with id '{id}' was not found.");
        }

        return lead;
    }

    private static void EnsureLeadCanBeEdited(Lead lead)
    {
        if (string.Equals(lead.Status, LeadStatus.Archived, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Archived Leads cannot be edited.");
        }

        if (string.Equals(lead.Status, LeadStatus.Converted, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Converted Leads cannot be edited.");
        }
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        if (string.Equals(currentStatus, LeadStatus.Converted, StringComparison.OrdinalIgnoreCase)
            && string.Equals(newStatus, LeadStatus.Prospect, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Converted Leads cannot return to Prospect.");
        }
    }

    private async Task EnsureEmailIsUniqueAsync(
        string? email,
        Guid? excludeLeadId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var normalizedEmail = email.Trim();

        if (await _leadRepository.ExistsActiveLeadWithEmailAsync(normalizedEmail, excludeLeadId, cancellationToken))
        {
            throw new ConflictException($"A Lead with email '{normalizedEmail}' already exists.");
        }
    }

    private static string GenerateLeadCode()
    {
        return $"LED-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private static Contact CreatePrimaryContact(string leadName, string? email, string? phone, DateTimeOffset now)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = leadName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            IsPrimary = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static void UpdatePrimaryContact(Lead lead, string leadName, string? email, string? phone)
    {
        var primaryContact = lead.LeadContacts
            .Select(link => link.Contact)
            .FirstOrDefault(contact => contact.IsPrimary);

        if (primaryContact is null)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var contact = CreatePrimaryContact(leadName, email, phone, now);
            lead.LeadContacts.Add(new LeadContact
            {
                LeadId = lead.Id,
                ContactId = contact.Id,
                CreatedAt = now,
                Contact = contact
            });

            return;
        }

        primaryContact.FirstName = leadName;
        primaryContact.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        primaryContact.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        primaryContact.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static LeadResponse MapToResponse(Lead lead, Guid? clientId)
    {
        var primaryContact = lead.LeadContacts
            .Select(link => link.Contact)
            .FirstOrDefault(contact => contact.IsPrimary);

        return new LeadResponse
        {
            Id = lead.Id,
            Code = lead.Code,
            CompanyName = lead.CompanyName,
            LeadName = lead.TradeName ?? string.Empty,
            Email = primaryContact?.Email,
            Phone = primaryContact?.Phone,
            Source = lead.Source,
            Status = lead.Status,
            EstimatedContractValue = lead.EstimatedRevenue,
            AssignedUserId = lead.OwnerId,
            Website = lead.Website,
            Segment = lead.Segment,
            Notes = lead.Notes,
            ClientId = clientId,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
