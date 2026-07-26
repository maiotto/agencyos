using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace AgencyOS.Application.Tests;

public class ContactsControllerHttpTests : IClassFixture<ContactApiFactory>
{
    private readonly ContactApiFactory _factory;

    public ContactsControllerHttpTests(ContactApiFactory factory)
    {
        _factory = factory;
        _factory.ContactService.Reset();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk()
    {
        _factory.ContactService
            .Setup(service => service.GetPagedAsync(
                It.IsAny<ContactQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<ContactResponse>
            {
                Items = [],
                Page = 1,
                PageSize = 20,
                TotalCount = 0,
                TotalPages = 0
            });

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/contacts?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenContactExists()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactResponse
            {
                Id = contactId,
                ClientId = Guid.NewGuid(),
                FirstName = "Alex",
                Email = "alex@acme.example",
                Status = ContactStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/contacts/{contactId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenContactMissing()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Contact with id '{contactId}' was not found."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"/contacts/{contactId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contacts", new CreateContactRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenRequestIsValid()
    {
        var contactId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateContactRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactResponse
            {
                Id = contactId,
                ClientId = clientId,
                FirstName = "Alex",
                Email = "alex@acme.example",
                Status = ContactStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contacts", new CreateContactRequest
        {
            ClientId = clientId,
            FirstName = "Alex",
            Email = "alex@acme.example",
            Status = ContactStatus.Active
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenEmailDuplicates()
    {
        var clientId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateContactRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException(
                "A Contact with email 'alex@acme.example' already exists for this Client."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/contacts", new CreateContactRequest
        {
            ClientId = clientId,
            FirstName = "Alex",
            Email = "alex@acme.example",
            Status = ContactStatus.Active
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenRequestIsValid()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.UpdateAsync(
                contactId,
                It.IsAny<UpdateContactRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactResponse
            {
                Id = contactId,
                ClientId = Guid.NewGuid(),
                FirstName = "Alex Updated",
                Email = "alex@acme.example",
                Status = ContactStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PutAsJsonAsync($"/contacts/{contactId}", new UpdateContactRequest
        {
            FirstName = "Alex Updated",
            Email = "alex@acme.example",
            Status = ContactStatus.Active
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.DeactivateAsync(contactId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.DeleteAsync($"/contacts/{contactId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SetPrimary_ReturnsOk()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.SetPrimaryAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContactResponse
            {
                Id = contactId,
                ClientId = Guid.NewGuid(),
                FirstName = "Alex",
                Email = "alex@acme.example",
                IsPrimaryContact = true,
                Status = ContactStatus.Active
            });

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contacts/{contactId}/primary", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SetPrimary_ReturnsBadRequest_WhenInactive()
    {
        var contactId = Guid.NewGuid();
        _factory.ContactService
            .Setup(service => service.SetPrimaryAsync(contactId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Inactive Contacts cannot become Primary."));

        var httpClient = _factory.CreateClient();
        var response = await httpClient.PatchAsync($"/contacts/{contactId}/primary", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class ContactApiFactory : WebApplicationFactory<Program>
{
    public Mock<IContactService> ContactService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=127.0.0.1;Port=54322;Database=postgres;Username=postgres;Password=postgres"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IContactService>();
            services.AddSingleton(ContactService.Object);
        });
    }
}
