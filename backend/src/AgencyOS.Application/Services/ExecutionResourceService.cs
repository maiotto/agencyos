using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class ExecutionResourceService : IExecutionResourceService
{
    private readonly IExecutionResourceRepository _executionResourceRepository;
    private readonly ILogger<ExecutionResourceService> _logger;

    public ExecutionResourceService(
        IExecutionResourceRepository executionResourceRepository,
        ILogger<ExecutionResourceService> logger)
    {
        _executionResourceRepository = executionResourceRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExecutionResourceResponse>> GetAllAsync(
        ExecutionResourceQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var resources = await _executionResourceRepository.GetAllAsync(parameters, cancellationToken);
        return resources.Select(MapToResponse).ToList();
    }

    public async Task<ExecutionResourceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceOrThrowAsync(id, cancellationToken);
        return MapToResponse(resource);
    }

    public async Task<ExecutionResourceResponse> CreateAsync(
        CreateExecutionResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureResourceCodeIsUniqueAsync(request.Code, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var resource = new ExecutionResource
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            ResourceType = request.ResourceType.Trim(),
            Status = request.Status.Trim(),
            CapacityHoursPerWeek = request.CapacityHoursPerWeek,
            CostRate = request.CostRate,
            Currency = NormalizeOptionalText(request.Currency),
            Skills = NormalizeSkills(request.Skills),
            Availability = NormalizeOptionalText(request.Availability),
            Notes = NormalizeOptionalText(request.Notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _executionResourceRepository.AddAsync(resource, cancellationToken);

        _logger.LogInformation(
            "Execution Resource Created: {ResourceId} ({ResourceCode})",
            created.Id,
            created.Code);

        return MapToResponse(created);
    }

    public async Task<ExecutionResourceResponse> UpdateAsync(
        Guid id,
        UpdateExecutionResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceOrThrowAsync(id, cancellationToken);

        await EnsureResourceCodeIsUniqueAsync(request.Code, id, cancellationToken);

        resource.Code = request.Code.Trim();
        resource.Name = request.Name.Trim();
        resource.ResourceType = request.ResourceType.Trim();
        resource.Status = request.Status.Trim();
        resource.CapacityHoursPerWeek = request.CapacityHoursPerWeek;
        resource.CostRate = request.CostRate;
        resource.Currency = NormalizeOptionalText(request.Currency);
        resource.Skills = NormalizeSkills(request.Skills);
        resource.Availability = NormalizeOptionalText(request.Availability);
        resource.Notes = NormalizeOptionalText(request.Notes);
        resource.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _executionResourceRepository.UpdateAsync(resource, cancellationToken);

        _logger.LogInformation(
            "Execution Resource Updated: {ResourceId} ({ResourceCode})",
            updated.Id,
            updated.Code);

        return MapToResponse(updated);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceOrThrowAsync(id, cancellationToken);

        if (string.Equals(resource.Status, ExecutionResourceStatus.Inactive, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        resource.Status = ExecutionResourceStatus.Inactive;
        resource.UpdatedAt = DateTimeOffset.UtcNow;

        await _executionResourceRepository.UpdateAsync(resource, cancellationToken);

        _logger.LogInformation(
            "Execution Resource Deactivated: {ResourceId} ({ResourceCode})",
            resource.Id,
            resource.Code);
    }

    private async Task<ExecutionResource> GetResourceOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _executionResourceRepository.GetByIdAsync(id, cancellationToken);

        if (resource is null)
        {
            throw new NotFoundException($"Execution Resource with id '{id}' was not found.");
        }

        return resource;
    }

    private async Task EnsureResourceCodeIsUniqueAsync(
        string code,
        Guid? excludeResourceId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim();

        if (await _executionResourceRepository.ExistsWithCodeAsync(
                normalizedCode,
                excludeResourceId,
                cancellationToken))
        {
            throw new ConflictException(
                $"An Execution Resource with code '{normalizedCode}' already exists.");
        }
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string[]? NormalizeSkills(IReadOnlyList<string>? skills)
    {
        if (skills is null || skills.Count == 0)
        {
            return null;
        }

        return skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(skill => skill.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static ExecutionResourceResponse MapToResponse(ExecutionResource resource)
    {
        return new ExecutionResourceResponse
        {
            Id = resource.Id,
            Code = resource.Code,
            Name = resource.Name,
            ResourceType = resource.ResourceType,
            Status = resource.Status,
            CapacityHoursPerWeek = resource.CapacityHoursPerWeek,
            CostRate = resource.CostRate,
            Currency = resource.Currency,
            Skills = resource.Skills ?? [],
            Availability = resource.Availability,
            Notes = resource.Notes,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt
        };
    }
}
