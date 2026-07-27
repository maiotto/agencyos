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

public class HolidaysControllerHttpTests : IClassFixture<HolidayApiFactory>
{
    private readonly HolidayApiFactory _factory;

    public HolidaysControllerHttpTests(HolidayApiFactory factory)
    {
        _factory = factory;
        _factory.HolidayService.Reset();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _factory.HolidayService
            .Setup(service => service.GetAllAsync(
                It.IsAny<HolidayQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HolidayResponse>());

        var response = await _factory.CreateClient().GetAsync("/holidays");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Filter_ReturnsOk()
    {
        _factory.HolidayService
            .Setup(service => service.FilterAsync(
                It.IsAny<HolidayQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HolidayResponse>());

        var response = await _factory.CreateClient().GetAsync("/holidays/filter?status=Active");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.GetByIdAsync(holidayId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Holiday with id '{holidayId}' was not found."));

        var response = await _factory.CreateClient().GetAsync($"/holidays/{holidayId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/holidays",
            new CreateHolidayRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateHolidayRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(holidayId));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/holidays",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateScope()
    {
        _factory.HolidayService
            .Setup(service => service.CreateAsync(
                It.IsAny<CreateHolidayRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Duplicate Holidays are not allowed for the same scope."));

        var response = await _factory.CreateClient().PostAsJsonAsync(
            "/holidays",
            CreateValidRequest());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.UpdateAsync(
                holidayId,
                It.IsAny<UpdateHolidayRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(holidayId));

        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/holidays/{holidayId}",
            new UpdateHolidayRequest
            {
                CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                Name = "Foundation Day",
                HolidayType = HolidayTypes.Company,
                HolidayDate = new DateOnly(2099, 11, 15)
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_ReturnsNoContent()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.ActivateAsync(holidayId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/holidays/{holidayId}/activate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.DeactivateAsync(holidayId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _factory.CreateClient().PostAsync($"/holidays/{holidayId}/deactivate", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenActive()
    {
        var holidayId = Guid.NewGuid();
        _factory.HolidayService
            .Setup(service => service.DeleteAsync(holidayId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("Deleting Active Holidays is prohibited. Deactivate first."));

        var response = await _factory.CreateClient().DeleteAsync($"/holidays/{holidayId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateHolidayRequest CreateValidRequest() =>
        new()
        {
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Foundation Day",
            HolidayType = HolidayTypes.Company,
            HolidayDate = new DateOnly(2099, 11, 15)
        };

    private static HolidayResponse CreateResponse(Guid holidayId) =>
        new()
        {
            Id = holidayId,
            CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
            Name = "Foundation Day",
            HolidayType = HolidayTypes.Company,
            HolidayDate = new DateOnly(2099, 11, 15),
            Status = HolidayStatus.Inactive,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}

public sealed class HolidayApiFactory : WebApplicationFactory<Program>
{
    public Mock<IHolidayService> HolidayService { get; } = new();

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
            services.RemoveAll<IHolidayService>();
            services.AddSingleton(HolidayService.Object);
        });
    }
}
