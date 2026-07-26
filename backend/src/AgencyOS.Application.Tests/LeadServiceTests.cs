using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class LeadServiceTests
{
    private readonly Mock<ILeadRepository> _leadRepository = new();
    private readonly Mock<ILogger<LeadService>> _logger = new();

    private LeadService CreateService() => new(_leadRepository.Object, _logger.Object);

    [Fact]
    public async Task CreateAsync_RejectsNonInitialStatus()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new CreateLeadRequest
            {
                CompanyName = "Acme",
                LeadName = "Acme Lead",
                Source = "Referral",
                Status = LeadStatus.Won
            }));

        Assert.Contains(LeadStatus.Prospect, exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsInvalidLifecycleTransition()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Prospect);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(leadId, CreateUpdateRequest(LeadStatus.Won)));

        Assert.Contains("Invalid Lead status transition", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_AllowsApprovedForwardTransition()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Prospect);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _leadRepository
            .Setup(repository => repository.UpdateAsync(lead, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _leadRepository
            .Setup(repository => repository.GetClientIdByLeadIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var service = CreateService();
        var result = await service.UpdateAsync(leadId, CreateUpdateRequest(LeadStatus.Qualified));

        Assert.Equal(LeadStatus.Qualified, result.Status);
    }

    [Fact]
    public async Task ConvertAsync_RejectsWhenLeadIsNotWon()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Qualified);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ConvertAsync(leadId, CreateConvertRequest()));

        Assert.Equal("Only Leads in status Won can be converted.", exception.Message);
    }

    [Fact]
    public async Task ConvertAsync_CreatesClientFromRequestWhenLeadIsWon()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Won);
        Client? persistedClient = null;

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _leadRepository
            .Setup(repository => repository.GetClientIdByLeadIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _leadRepository
            .Setup(repository => repository.ConvertLeadAsync(
                lead,
                It.IsAny<Client>(),
                It.IsAny<IReadOnlyCollection<ClientContact>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Lead, Client, IReadOnlyCollection<ClientContact>, CancellationToken>(
                (_, client, _, _) => persistedClient = client)
            .ReturnsAsync((Lead _, Client client, IReadOnlyCollection<ClientContact> _, CancellationToken _) => client);

        var request = CreateConvertRequest();
        var service = CreateService();
        var result = await service.ConvertAsync(leadId, request);

        Assert.NotNull(persistedClient);
        Assert.Equal(request.LegalName, persistedClient!.LegalName);
        Assert.Equal(request.TaxIdentifier, persistedClient.TaxId);
        Assert.Equal(leadId, persistedClient.LeadId);
        Assert.Equal(LeadStatus.Converted, lead.Status);
        Assert.Equal(persistedClient.Id, result.ClientId);
    }

    [Fact]
    public async Task ConvertAsync_BindsExistingLeadContactsToClient()
    {
        var leadId = Guid.NewGuid();
        var primaryContactId = Guid.NewGuid();
        var secondaryContactId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var lead = CreateLead(leadId, LeadStatus.Won);
        lead.LeadContacts.Add(new LeadContact
        {
            LeadId = leadId,
            ContactId = primaryContactId,
            CreatedAt = now,
            Contact = new Contact
            {
                Id = primaryContactId,
                FirstName = "Alex",
                Email = "alex@acme.example",
                Mobile = "+1-555-0199",
                IsPrimary = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        });
        lead.LeadContacts.Add(new LeadContact
        {
            LeadId = leadId,
            ContactId = secondaryContactId,
            CreatedAt = now,
            Contact = new Contact
            {
                Id = secondaryContactId,
                FirstName = "Sam",
                Email = "sam@acme.example",
                IsPrimary = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        });

        IReadOnlyCollection<ClientContact>? boundContacts = null;
        Client? persistedClient = null;

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _leadRepository
            .Setup(repository => repository.GetClientIdByLeadIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        _leadRepository
            .Setup(repository => repository.ConvertLeadAsync(
                lead,
                It.IsAny<Client>(),
                It.IsAny<IReadOnlyCollection<ClientContact>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Lead, Client, IReadOnlyCollection<ClientContact>, CancellationToken>(
                (_, client, contacts, _) =>
                {
                    persistedClient = client;
                    boundContacts = contacts;
                })
            .ReturnsAsync((Lead _, Client client, IReadOnlyCollection<ClientContact> _, CancellationToken _) => client);

        var service = CreateService();
        await service.ConvertAsync(leadId, CreateConvertRequest());

        Assert.NotNull(persistedClient);
        Assert.NotNull(boundContacts);
        Assert.Equal(2, boundContacts!.Count);
        Assert.All(boundContacts, link => Assert.Equal(persistedClient!.Id, link.ClientId));
        Assert.Contains(boundContacts, link => link.ContactId == primaryContactId);
        Assert.Contains(boundContacts, link => link.ContactId == secondaryContactId);
        Assert.True(lead.LeadContacts.Single(link => link.ContactId == primaryContactId).Contact!.IsPrimary);
        Assert.False(lead.LeadContacts.Single(link => link.ContactId == secondaryContactId).Contact!.IsPrimary);
    }

    [Fact]
    public async Task ArchiveAsync_RejectsConvertedLead()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Converted);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ArchiveAsync(leadId));

        Assert.Equal("Converted Leads cannot be archived.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_RejectsConvertedLeadAsReadOnly()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Converted);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(leadId, CreateUpdateRequest(LeadStatus.Converted)));

        Assert.Equal("Converted Leads cannot be edited.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenEmailAlreadyExists()
    {
        _leadRepository
            .Setup(repository => repository.ExistsActiveLeadWithEmailAsync(
                "alex@acme.example",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateLeadRequest
            {
                CompanyName = "Acme",
                LeadName = "Alex",
                Email = "alex@acme.example",
                Source = "Referral",
                Status = LeadStatus.Prospect
            }));

        Assert.Contains("alex@acme.example", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenLeadMissing()
    {
        var leadId = Guid.NewGuid();
        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(leadId));
    }

    [Fact]
    public async Task ConvertAsync_RejectsWhenLeadAlreadyConverted()
    {
        var leadId = Guid.NewGuid();
        var lead = CreateLead(leadId, LeadStatus.Won);

        _leadRepository
            .Setup(repository => repository.GetByIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);

        _leadRepository
            .Setup(repository => repository.GetClientIdByLeadIdAsync(leadId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ConvertAsync(leadId, CreateConvertRequest()));

        Assert.Equal("Lead has already been converted.", exception.Message);
    }

    private static Lead CreateLead(Guid id, string status)
    {
        return new Lead
        {
            Id = id,
            Code = "LED-TEST001",
            CompanyName = "Acme",
            TradeName = "Acme Lead",
            Source = "Referral",
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static UpdateLeadRequest CreateUpdateRequest(string status)
    {
        return new UpdateLeadRequest
        {
            CompanyName = "Acme",
            LeadName = "Acme Lead",
            Source = "Referral",
            Status = status
        };
    }

    private static ConvertLeadRequest CreateConvertRequest()
    {
        return new ConvertLeadRequest
        {
            LegalName = "Acme Corporation Ltd",
            TaxIdentifier = "12.345.678/0001-90",
            TradeName = "Acme"
        };
    }
}
