using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Validators;
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

namespace AgencyOS.Application.Tests;

public class NotificationQueryParametersValidatorTests
{
    private readonly NotificationQueryParametersValidator _validator = new();

    [Fact]
    public void AcceptsValidFilters()
    {
        var result = _validator.Validate(new NotificationQueryParameters
        {
            Category = NotificationCategory.Decision,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Unread,
            OrderBy = "createdAt",
            OrderDirection = "desc"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RejectsUnknownCategoryAndInvertedDates()
    {
        var result = _validator.Validate(new NotificationQueryParameters
        {
            Category = "Nope",
            CreatedFrom = DateTimeOffset.UtcNow,
            CreatedTo = DateTimeOffset.UtcNow.AddDays(-1)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Category" || error.ErrorMessage.Contains("Category"));
    }
}

public class NotificationsControllerHttpTests : IClassFixture<NotificationsApiFactory>
{
    private readonly NotificationsApiFactory _factory;

    public NotificationsControllerHttpTests(NotificationsApiFactory factory)
    {
        _factory = factory;
        _factory.QueryService.Reset();
        _factory.NotificationService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.QueryService
            .Setup(service => service.GetAllAsync(
                It.IsAny<NotificationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<NotificationResponse>());

        var response = await _factory.CreateClient().GetAsync("/notifications");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.QueryService
            .Setup(service => service.FilterAsync(
                It.IsAny<NotificationQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<NotificationResponse>());

        var response = await _factory.CreateClient().GetAsync("/notifications/filter?category=Decision");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _factory.QueryService
            .Setup(service => service.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var response = await _factory.CreateClient().GetAsync($"/notifications/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkReadUnreadArchive_ReturnOk()
    {
        var id = Guid.NewGuid();
        var responseItem = CreateResponse(id);
        var archived = CreateResponse(id);
        archived.Archived = true;
        _factory.NotificationService
            .Setup(service => service.MarkReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseItem);
        _factory.NotificationService
            .Setup(service => service.MarkUnreadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseItem);
        _factory.NotificationService
            .Setup(service => service.ArchiveAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(archived);

        var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/notifications/{id}/read", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/notifications/{id}/unread", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/notifications/{id}/archive", null)).StatusCode);
    }

    private static NotificationResponse CreateResponse(Guid id) =>
        new()
        {
            Id = id,
            CompanyId = AgencyOSCompanies.DefaultCompanyId,
            UserId = "planner",
            Title = "Title",
            Message = "Message",
            Category = NotificationCategory.Decision,
            Priority = NotificationPriority.High,
            Status = NotificationStatus.Unread,
            SourceEntity = NotificationSourceEntities.Decision,
            SourceEntityId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Archived = false,
            NavigationPath = "/decisions/" + Guid.NewGuid()
        };
}

public sealed class NotificationsApiFactory : WebApplicationFactory<Program>
{
    public Mock<INotificationQueryService> QueryService { get; } = new();
    public Mock<INotificationService> NotificationService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Database=agencyos_test;Username=test;Password=test"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<INotificationQueryService>();
            services.RemoveAll<INotificationService>();
            services.AddSingleton(QueryService.Object);
            services.AddSingleton(NotificationService.Object);
        });
    }
}
