using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

/// <summary>
/// Company Decision Profile CRUD, versioning, and lifecycle service (US-401 / BR-1901..BR-1910).
/// </summary>
public class CompanyDecisionProfileService : ICompanyDecisionProfileService
{
    private readonly ICompanyDecisionProfileRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<CompanyDecisionProfileService> _logger;

    public CompanyDecisionProfileService(
        ICompanyDecisionProfileRepository repository,
        ICompanyRepository companyRepository,
        IAuditService auditService,
        ILogger<CompanyDecisionProfileService> logger)
    {
        _repository = repository;
        _companyRepository = companyRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyDecisionProfileResponse>> GetAllAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.QueryAsync(parameters, cancellationToken);
        return profiles.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<CompanyDecisionProfileResponse>> FilterAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<CompanyDecisionProfileResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(profile);
    }

    public async Task<IReadOnlyList<CompanyDecisionProfileResponse>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetByCompanyIdAsync(companyId, cancellationToken);
        return profiles.Select(MapToResponse).ToList();
    }

    public async Task<CompanyDecisionProfileResponse> GetDefaultActiveAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetDefaultActiveAsync(companyId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException(
                $"No default Active Company Decision Profile was found for company '{companyId}'.");
        }

        return MapToResponse(profile);
    }

    public async Task<CompanyDecisionProfileResponse> CreateAsync(
        CreateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueCodeAsync(request.CompanyId, request.Code, excludeProfileFamilyId: null, cancellationToken);
        await EnsureUniqueActiveNameAsync(
            request.CompanyId,
            request.Name,
            excludeProfileFamilyId: null,
            cancellationToken);

        CompanyDecisionProfile profile;
        try
        {
            profile = CompanyDecisionProfile.Create(
                request.CompanyId,
                request.Code,
                request.Name,
                request.Description,
                SerializeWeights(request.PriorityWeights),
                request.CapacityWeight,
                request.WorkloadWeight,
                request.CostWeight,
                request.RiskWeight,
                request.QualityWeight,
                request.PreferredStrategy,
                request.PreferredCapacityThreshold,
                request.PreferredWorkloadThreshold,
                request.DefaultProfile,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (profile.DefaultProfile)
        {
            await EnsureNoOtherDefaultAsync(profile.CompanyId, excludeProfileFamilyId: null, cancellationToken);
        }

        var created = await _repository.AddAsync(profile, cancellationToken);

        await RecordAuditAsync(
            created,
            AuditEventTypes.Created,
            "CompanyDecisionProfile.Create",
            previousState: null,
            cancellationToken);

        _logger.LogInformation(
            "Company Decision Profile Created: {ProfileId} ({Name}) Company={CompanyId}",
            created.Id,
            created.Name,
            created.CompanyId);

        return MapToResponse(created);
    }

    public async Task<CompanyDecisionProfileResponse> UpdateAsync(
        Guid id,
        UpdateCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var current = await GetOrThrowAsync(id, cancellationToken);
        var latest = await _repository.GetLatestByFamilyAsync(current.ProfileFamilyId, cancellationToken);

        if (latest is not null && latest.Id != current.Id)
        {
            throw new BusinessRuleException(
                "Only the latest version of a Company Decision Profile can be updated (BR-1905).");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)
            && !string.Equals(request.Name.Trim(), current.Name, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureUniqueActiveNameAsync(
                current.CompanyId,
                request.Name,
                current.ProfileFamilyId,
                cancellationToken);
        }

        CompanyDecisionProfile newVersion;
        try
        {
            newVersion = current.CreateNewVersion(
                request.Name,
                request.Description,
                SerializeWeights(request.PriorityWeights),
                request.CapacityWeight,
                request.WorkloadWeight,
                request.CostWeight,
                request.RiskWeight,
                request.QualityWeight,
                request.PreferredStrategy,
                request.PreferredCapacityThreshold,
                request.PreferredWorkloadThreshold,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var wasDefault = current.DefaultProfile;
        var previousState = AuditService.SerializeState(new { current.Id, current.Version, current.Status });

        if (wasDefault)
        {
            current.ClearDefault(DateTimeOffset.UtcNow);
        }

        try
        {
            current.Deactivate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(current, cancellationToken);
        var created = await _repository.AddAsync(newVersion, cancellationToken);

        await RecordAuditAsync(
            created,
            AuditEventTypes.VersionCreated,
            "CompanyDecisionProfile.Update",
            previousState,
            cancellationToken);

        _logger.LogInformation(
            "Company Decision Profile New Version Created: {ProfileId} v{Version} (Previous={PreviousId})",
            created.Id,
            created.Version,
            current.Id);

        return MapToResponse(created);
    }

    public async Task<CompanyDecisionProfileResponse> CloneAsync(
        Guid id,
        CloneCompanyDecisionProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await GetOrThrowAsync(id, cancellationToken);

        await EnsureUniqueCodeAsync(source.CompanyId, request.Code, excludeProfileFamilyId: null, cancellationToken);
        await EnsureUniqueActiveNameAsync(
            source.CompanyId,
            request.Name,
            excludeProfileFamilyId: null,
            cancellationToken);

        CompanyDecisionProfile clone;
        try
        {
            clone = source.Clone(request.Name, request.Code, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _repository.AddAsync(clone, cancellationToken);

        await RecordAuditAsync(
            created,
            AuditEventTypes.Created,
            "CompanyDecisionProfile.Clone",
            previousState: AuditService.SerializeState(new { SourceId = source.Id }),
            cancellationToken);

        _logger.LogInformation(
            "Company Decision Profile Cloned: Source={SourceId} Clone={CloneId} ({Name})",
            source.Id,
            created.Id,
            created.Name);

        return MapToResponse(created);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);

        if (profile.IsActive)
        {
            return;
        }

        try
        {
            profile.Activate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(profile, cancellationToken);
        await RecordAuditAsync(
            profile,
            AuditEventTypes.Activated,
            "CompanyDecisionProfile.Activate",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Decision Profile Activated: {ProfileId}", profile.Id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);

        if (profile.IsInactive)
        {
            return;
        }

        try
        {
            profile.Deactivate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(profile, cancellationToken);
        await RecordAuditAsync(
            profile,
            AuditEventTypes.Deactivated,
            "CompanyDecisionProfile.Deactivate",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Decision Profile Deactivated: {ProfileId}", profile.Id);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            profile.Archive(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(profile, cancellationToken);
        await RecordAuditAsync(
            profile,
            AuditEventTypes.Archived,
            "CompanyDecisionProfile.Archive",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Decision Profile Archived: {ProfileId}", profile.Id);
    }

    public async Task<CompanyDecisionProfileResponse> SetDefaultAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);

        if (profile.DefaultProfile)
        {
            return MapToResponse(profile);
        }

        await EnsureNoOtherDefaultAsync(profile.CompanyId, profile.ProfileFamilyId, cancellationToken);

        try
        {
            profile.SetDefault(true, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(profile, cancellationToken);
        await RecordAuditAsync(
            profile,
            AuditEventTypes.StatusChanged,
            "CompanyDecisionProfile.SetDefault",
            previousState: null,
            cancellationToken);

        await SyncCompanyDecisionProfileAsync(profile, cancellationToken);

        _logger.LogInformation("Company Decision Profile Set As Default: {ProfileId}", profile.Id);

        return MapToResponse(profile);
    }

    /// <summary>
    /// Keeps Company.DecisionProfileId aligned with the active default profile (BR-2007).
    /// Best-effort: a missing company must not fail the SetDefault operation.
    /// </summary>
    private async Task SyncCompanyDecisionProfileAsync(
        CompanyDecisionProfile profile,
        CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(profile.CompanyId, cancellationToken);
        if (company is null)
        {
            return;
        }

        company.AssignDecisionProfile(profile.Id, DateTimeOffset.UtcNow);
        await _companyRepository.UpdateAsync(company, cancellationToken);
    }

    public async Task<CompanyDecisionProfileResponse> ClearDefaultAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetOrThrowAsync(id, cancellationToken);

        profile.ClearDefault(DateTimeOffset.UtcNow);
        await _repository.UpdateAsync(profile, cancellationToken);
        await RecordAuditAsync(
            profile,
            AuditEventTypes.StatusChanged,
            "CompanyDecisionProfile.ClearDefault",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Decision Profile Default Cleared: {ProfileId}", profile.Id);

        return MapToResponse(profile);
    }

    private async Task EnsureUniqueActiveNameAsync(
        Guid companyId,
        string name,
        Guid? excludeProfileFamilyId,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsActiveNameAsync(companyId, name, excludeProfileFamilyId, cancellationToken))
        {
            throw new ConflictException(
                $"An Active Company Decision Profile named '{name.Trim()}' already exists for this company (BR-1902).");
        }
    }

    private async Task EnsureUniqueCodeAsync(
        Guid companyId,
        string code,
        Guid? excludeProfileFamilyId,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsCodeAsync(companyId, code, excludeProfileFamilyId, cancellationToken))
        {
            throw new ConflictException(
                $"A Company Decision Profile with Code '{code.Trim()}' already exists for this company.");
        }
    }

    private async Task EnsureNoOtherDefaultAsync(
        Guid companyId,
        Guid? excludeProfileFamilyId,
        CancellationToken cancellationToken)
    {
        var existingDefault = await _repository.GetDefaultActiveAsync(companyId, cancellationToken);
        if (existingDefault is null)
        {
            return;
        }

        if (excludeProfileFamilyId.HasValue && existingDefault.ProfileFamilyId == excludeProfileFamilyId.Value)
        {
            return;
        }

        throw new ConflictException(
            "Company already has a default Active Company Decision Profile (BR-1901). Clear it before assigning a new default.");
    }

    private async Task<CompanyDecisionProfile> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException($"Company Decision Profile with id '{id}' was not found.");
        }

        return profile;
    }

    private async Task RecordAuditAsync(
        CompanyDecisionProfile profile,
        string eventType,
        string action,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.CompanyDecisionProfile,
                EntityId = profile.Id,
                EntityVersion = profile.Version.ToString(),
                EventType = eventType,
                Action = action,
                CompanyId = profile.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                PreviousState = previousState,
                CurrentState = AuditService.SerializeState(new
                {
                    profile.Id,
                    profile.ProfileFamilyId,
                    profile.Code,
                    profile.Name,
                    profile.Status,
                    profile.Version,
                    profile.DefaultProfile
                })
            },
            cancellationToken);
    }

    private static string SerializeWeights(IReadOnlyList<DecisionProfileDimensionRequest> weights)
    {
        var settings = weights
            .Select(weight => new DecisionProfileDimensionSetting
            {
                Dimension = weight.Dimension,
                Weight = weight.Weight,
                PreferHigherValues = weight.PreferHigherValues
            })
            .ToList();

        return CompanyDecisionProfile.SerializeDimensions(settings);
    }

    internal static CompanyDecisionProfileResponse MapToResponse(CompanyDecisionProfile profile) =>
        new()
        {
            Id = profile.Id,
            CompanyId = profile.CompanyId,
            ProfileFamilyId = profile.ProfileFamilyId,
            Code = profile.Code,
            Name = profile.Name,
            Description = profile.Description,
            Status = profile.Status,
            Dimensions = profile.Dimensions
                .Select(dimension => new DecisionProfileDimensionRequest
                {
                    Dimension = dimension.Dimension,
                    Weight = dimension.Weight,
                    PreferHigherValues = dimension.PreferHigherValues
                })
                .ToList(),
            CapacityWeight = profile.CapacityWeight,
            WorkloadWeight = profile.WorkloadWeight,
            CostWeight = profile.CostWeight,
            RiskWeight = profile.RiskWeight,
            QualityWeight = profile.QualityWeight,
            PreferredStrategy = profile.PreferredStrategy,
            PreferredCapacityThreshold = profile.PreferredCapacityThreshold,
            PreferredWorkloadThreshold = profile.PreferredWorkloadThreshold,
            DefaultProfile = profile.DefaultProfile,
            Version = profile.Version,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            ArchivedAt = profile.ArchivedAt
        };
}
