using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgencyOS.Application.Tests;

public class ExecutionResourceServiceTests
{
    private readonly Mock<IExecutionResourceRepository> _executionResourceRepository = new();
    private readonly Mock<ILogger<ExecutionResourceService>> _logger = new();

    private ExecutionResourceService CreateService() =>
        new(_executionResourceRepository.Object, _logger.Object);

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(resourceId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResources()
    {
        var resource = CreateResource(Guid.NewGuid());

        _executionResourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ExecutionResourceQueryParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { resource });

        var service = CreateService();
        var result = await service.GetAllAsync(new ExecutionResourceQueryParameters());

        Assert.Single(result);
        Assert.Equal(resource.Code, result[0].Code);
        Assert.Equal(resource.Skills, result[0].Skills);
    }

    [Fact]
    public async Task CreateAsync_PersistsResource()
    {
        ExecutionResource? persisted = null;

        SetupUniqueCode();
        SetupAdd(resource => persisted = resource);

        var service = CreateService();
        var result = await service.CreateAsync(CreateValidCreateRequest());

        Assert.NotNull(persisted);
        Assert.Equal("RES-001", persisted!.Code);
        Assert.Equal("Senior Delivery Consultant", persisted.Name);
        Assert.Equal(ExecutionResourceStatus.Active, persisted.Status);
        Assert.Equal("RES-001", result.Code);
    }

    [Fact]
    public async Task CreateAsync_PersistsCanonicalResourceTypeAndStatus()
    {
        ExecutionResource? persisted = null;

        SetupUniqueCode();
        SetupAdd(resource => persisted = resource);

        var request = CreateValidCreateRequest();
        request.ResourceType = "  internal human  ";
        request.Status = "aCTIVE";

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Equal(ExecutionResourceType.InternalHuman, persisted!.ResourceType);
        Assert.Equal(ExecutionResourceStatus.Active, persisted.Status);
    }

    [Fact]
    public async Task CreateAsync_NormalizesOptionalTextAndSkills()
    {
        ExecutionResource? persisted = null;

        SetupUniqueCode();
        SetupAdd(resource => persisted = resource);

        var request = CreateValidCreateRequest();
        request.Currency = "   ";
        request.Availability = "   ";
        request.Notes = "   ";
        request.Skills = new[] { " Discovery ", "discovery", "  " };

        var service = CreateService();
        await service.CreateAsync(request);

        Assert.NotNull(persisted);
        Assert.Null(persisted!.Currency);
        Assert.Null(persisted.Availability);
        Assert.Null(persisted.Notes);
        Assert.Equal(new[] { "Discovery" }, persisted.Skills);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictWhenCodeAlreadyExists()
    {
        _executionResourceRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                "RES-001",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateValidCreateRequest()));

        Assert.Contains("RES-001", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundWhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(resourceId, CreateValidUpdateRequest()));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflictWhenCodeBelongsToAnotherResource()
    {
        var resourceId = Guid.NewGuid();
        SetupExistingResource(resourceId);

        _executionResourceRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                "RES-001",
                resourceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(resourceId, CreateValidUpdateRequest()));

        Assert.Contains("RES-001", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_AppliesRequestedValues()
    {
        var resourceId = Guid.NewGuid();
        var resource = SetupExistingResource(resourceId);

        _executionResourceRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                It.IsAny<string>(),
                resourceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _executionResourceRepository
            .Setup(repository => repository.UpdateAsync(
                It.IsAny<ExecutionResource>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource updated, CancellationToken _) => updated);

        var request = CreateValidUpdateRequest();
        request.Name = "Lead Delivery Consultant";
        request.CapacityHoursPerWeek = 32;

        var service = CreateService();
        var result = await service.UpdateAsync(resourceId, request);

        Assert.Equal("Lead Delivery Consultant", resource.Name);
        Assert.Equal(32, resource.CapacityHoursPerWeek);
        Assert.Equal("Lead Delivery Consultant", result.Name);
    }

    [Fact]
    public async Task DeactivateAsync_SetsInactiveStatus()
    {
        var resourceId = Guid.NewGuid();
        var resource = SetupExistingResource(resourceId);

        _executionResourceRepository
            .Setup(repository => repository.UpdateAsync(resource, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var service = CreateService();
        await service.DeactivateAsync(resourceId);

        Assert.Equal(ExecutionResourceStatus.Inactive, resource.Status);
        _executionResourceRepository.Verify(
            repository => repository.UpdateAsync(resource, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_IsIdempotentWhenAlreadyInactive()
    {
        var resourceId = Guid.NewGuid();
        var resource = SetupExistingResource(resourceId);
        resource.Status = ExecutionResourceStatus.Inactive;

        var service = CreateService();
        await service.DeactivateAsync(resourceId);

        _executionResourceRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<ExecutionResource>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_ThrowsNotFoundWhenResourceMissing()
    {
        var resourceId = Guid.NewGuid();
        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExecutionResource?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeactivateAsync(resourceId));
    }

    private void SetupUniqueCode()
    {
        _executionResourceRepository
            .Setup(repository => repository.ExistsWithCodeAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupAdd(Action<ExecutionResource> capture)
    {
        _executionResourceRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<ExecutionResource>(),
                It.IsAny<CancellationToken>()))
            .Callback<ExecutionResource, CancellationToken>((resource, _) => capture(resource))
            .ReturnsAsync((ExecutionResource resource, CancellationToken _) => resource);
    }

    private ExecutionResource SetupExistingResource(Guid resourceId)
    {
        var resource = CreateResource(resourceId);

        _executionResourceRepository
            .Setup(repository => repository.GetByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        return resource;
    }

    private static CreateExecutionResourceRequest CreateValidCreateRequest()
    {
        return new CreateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40,
            CostRate = 120,
            Currency = "BRL",
            Skills = new[] { "Discovery", "Solution Design" },
            Availability = "Monday to Friday, business hours",
            Notes = "Allocated to strategic accounts"
        };
    }

    private static UpdateExecutionResourceRequest CreateValidUpdateRequest()
    {
        return new UpdateExecutionResourceRequest
        {
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40,
            CostRate = 120,
            Currency = "BRL",
            Skills = new[] { "Discovery" },
            Availability = "Monday to Friday, business hours",
            Notes = "Allocated to strategic accounts"
        };
    }

    private static ExecutionResource CreateResource(Guid id)
    {
        var now = DateTimeOffset.UtcNow;

        return new ExecutionResource
        {
            Id = id,
            Code = "RES-001",
            Name = "Senior Delivery Consultant",
            ResourceType = ExecutionResourceType.InternalHuman,
            Status = ExecutionResourceStatus.Active,
            CapacityHoursPerWeek = 40,
            CostRate = 120,
            Currency = "BRL",
            Skills = ["Discovery", "Solution Design"],
            Availability = "Monday to Friday, business hours",
            Notes = "Allocated to strategic accounts",
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
