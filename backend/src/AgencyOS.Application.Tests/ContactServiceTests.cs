using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class ContactServiceTests
{
    private readonly Mock<IContactRepository> _contactRepository = new();
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<ILogger<ContactService>> _logger = new();

    private ContactService CreateService() =>
        new(_contactRepository.Object, _clientRepository.Object, _logger.Object);

    [Fact]
    public async Task GetPagedAsync_ReturnsClampedPageSize()
    {
        _contactRepository
            .Setup(repository => repository.GetPagedAsync(
                It.IsAny<ContactQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<(Contact, Guid)>(), 0));

        var service = CreateService();
        var result = await service.GetPagedAsync(new ContactQueryParameters
        {
            Page = 1,
            PageSize = 500
        });

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenContactMissing()
    {
        var contactId = Guid.NewGuid();
        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((Contact, Guid)?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(contactId));
    }

    [Fact]
    public async Task CreateAsync_PersistsContactAndClearsPreviousPrimary()
    {
        var clientId = Guid.NewGuid();
        Contact? persisted = null;
        ClientContact? persistedLink = null;

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        _contactRepository
            .Setup(repository => repository.ExistsWithEmailForClientAsync(
                "alex@acme.example",
                clientId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contactRepository
            .Setup(repository => repository.ClearPrimaryForClientAsync(
                clientId,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _contactRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Contact>(),
                It.IsAny<ClientContact>(),
                It.IsAny<CancellationToken>()))
            .Callback<Contact, ClientContact, CancellationToken>((contact, link, _) =>
            {
                persisted = contact;
                persistedLink = link;
            })
            .ReturnsAsync((Contact contact, ClientContact _, CancellationToken _) => contact);

        var service = CreateService();
        var result = await service.CreateAsync(new CreateContactRequest
        {
            ClientId = clientId,
            FirstName = "Alex",
            Email = "alex@acme.example",
            Mobile = "+1-555-0199",
            IsPrimaryContact = true,
            Status = ContactStatus.Active
        });

        Assert.NotNull(persisted);
        Assert.NotNull(persistedLink);
        Assert.Equal(clientId, persistedLink!.ClientId);
        Assert.True(persisted!.IsPrimary);
        Assert.Equal("+1-555-0199", persisted.Mobile);
        Assert.Equal(ContactStatus.Active, result.Status);
        Assert.Equal("+1-555-0199", result.Mobile);

        _contactRepository.Verify(
            repository => repository.ClearPrimaryForClientAsync(clientId, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenEmailExistsForClient()
    {
        var clientId = Guid.NewGuid();

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        _contactRepository
            .Setup(repository => repository.ExistsWithEmailForClientAsync(
                "alex@acme.example",
                clientId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateContactRequest
            {
                ClientId = clientId,
                FirstName = "Alex",
                Email = "alex@acme.example",
                Status = ContactStatus.Active
            }));

        Assert.Contains("alex@acme.example", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundWhenClientMissing()
    {
        var clientId = Guid.NewGuid();
        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(new CreateContactRequest
            {
                ClientId = clientId,
                FirstName = "Alex",
                Email = "alex@acme.example",
                Status = ContactStatus.Active
            }));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var clientId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var contact = CreateContact(contactId, "+1-555-0100", isPrimary: false);

        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((contact, clientId));

        _contactRepository
            .Setup(repository => repository.ExistsWithEmailForClientAsync(
                "updated@acme.example",
                clientId,
                contactId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _contactRepository
            .Setup(repository => repository.UpdateAsync(contact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var service = CreateService();
        var result = await service.UpdateAsync(contactId, new UpdateContactRequest
        {
            FirstName = "Updated",
            Email = "updated@acme.example",
            Mobile = "+1-555-0199",
            IsPrimaryContact = false,
            Status = ContactStatus.Active
        });

        Assert.Equal("Updated", contact.FirstName);
        Assert.Equal("updated@acme.example", contact.Email);
        Assert.Equal("+1-555-0199", result.Mobile);
        Assert.Equal(ContactStatus.Active, result.Status);
    }

    [Fact]
    public async Task DeactivateAsync_PreservesMobileAndClearsPrimary()
    {
        var clientId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var contact = CreateContact(contactId, "+1-555-0199", isPrimary: true);

        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((contact, clientId));

        _contactRepository
            .Setup(repository => repository.UpdateAsync(contact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var service = CreateService();
        await service.DeactivateAsync(contactId);

        Assert.False(contact.IsPrimary);
        Assert.Equal(ContactStatus.Inactive, ContactInactiveState.GetStatus(contact));
        Assert.Equal("+1-555-0199", ContactInactiveState.GetMobile(contact));
        Assert.StartsWith("__I|", contact.Mobile);
    }

    [Fact]
    public async Task DeactivateAsync_IsIdempotentWhenAlreadyInactive()
    {
        var clientId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var contact = CreateContact(contactId, "+1-555-0199", isPrimary: false);
        ContactInactiveState.MarkInactive(contact);

        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((contact, clientId));

        var service = CreateService();
        await service.DeactivateAsync(contactId);

        _contactRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetPrimaryAsync_RejectsInactiveContact()
    {
        var clientId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var contact = CreateContact(contactId, "+1-555-0199", isPrimary: false);
        ContactInactiveState.MarkInactive(contact);

        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((contact, clientId));

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.SetPrimaryAsync(contactId));

        Assert.Equal("Inactive Contacts cannot become Primary.", exception.Message);
    }

    [Fact]
    public async Task SetPrimaryAsync_ClearsPreviousPrimaryAndSetsContact()
    {
        var clientId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var contact = CreateContact(contactId, "+1-555-0199", isPrimary: false);

        _contactRepository
            .Setup(repository => repository.GetClientContactByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((contact, clientId));

        _contactRepository
            .Setup(repository => repository.ClearPrimaryForClientAsync(
                clientId,
                contactId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _contactRepository
            .Setup(repository => repository.UpdateAsync(contact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var service = CreateService();
        var result = await service.SetPrimaryAsync(contactId);

        Assert.True(contact.IsPrimary);
        Assert.True(result.IsPrimaryContact);
        _contactRepository.Verify(
            repository => repository.ClearPrimaryForClientAsync(clientId, contactId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RejectsPrimaryWhenInactive()
    {
        var clientId = Guid.NewGuid();

        _clientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                LegalName = "Acme",
                Status = ClientStatus.Active,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        _contactRepository
            .Setup(repository => repository.ExistsWithEmailForClientAsync(
                It.IsAny<string>(),
                clientId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new CreateContactRequest
            {
                ClientId = clientId,
                FirstName = "Alex",
                Email = "alex@acme.example",
                IsPrimaryContact = true,
                Status = ContactStatus.Inactive
            }));

        Assert.Equal("Inactive Contacts cannot become Primary.", exception.Message);
    }

    private static Contact CreateContact(Guid id, string mobile, bool isPrimary)
    {
        return new Contact
        {
            Id = id,
            FirstName = "Alex",
            Email = "alex@acme.example",
            Mobile = mobile,
            IsPrimary = isPrimary,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }
}
