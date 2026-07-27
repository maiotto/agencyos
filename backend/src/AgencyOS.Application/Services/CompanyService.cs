using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

/// <summary>
/// Company CRUD and lifecycle service (US-402 / BR-2001..BR-2010).
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repository;
    private readonly ICompanyDecisionProfileRepository _decisionProfileRepository;
    private readonly IPlanningTemplateRepository _planningTemplateRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository repository,
        ICompanyDecisionProfileRepository decisionProfileRepository,
        IPlanningTemplateRepository planningTemplateRepository,
        IAuditService auditService,
        ILogger<CompanyService> logger)
    {
        _repository = repository;
        _decisionProfileRepository = decisionProfileRepository;
        _planningTemplateRepository = planningTemplateRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync(
        CompanyQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var companies = await _repository.GetAllAsync(parameters, cancellationToken);
        return companies.Select(MapToResponse).ToList();
    }

    public async Task<CompanyResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(company);
    }

    public async Task<CompanyResponse> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueCodeAsync(request.CompanyCode, excludeCompanyId: null, cancellationToken);
        await EnsureUniqueNameAsync(request.CompanyName, excludeCompanyId: null, cancellationToken);

        var companyId = Guid.NewGuid();

        if (request.DecisionProfileId.HasValue)
        {
            await EnsureDecisionProfileAssignableAsync(companyId, request.DecisionProfileId.Value, cancellationToken);
        }

        if (request.DefaultPlanningTemplateId.HasValue)
        {
            await EnsurePlanningTemplateAssignableAsync(
                companyId,
                request.DefaultPlanningTemplateId.Value,
                cancellationToken);
        }

        Company company;
        try
        {
            company = Company.Create(
                companyId,
                request.CompanyCode,
                request.CompanyName,
                request.LegalName,
                request.Timezone,
                request.Country,
                request.Language,
                request.Currency,
                request.PlanningConfiguration,
                request.DecisionProfileId,
                request.DefaultCalendarId,
                request.DefaultPlanningTemplateId,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _repository.AddAsync(company, cancellationToken);

        await RecordAuditAsync(
            created,
            AuditEventTypes.Created,
            "Company.Create",
            previousState: null,
            cancellationToken);

        _logger.LogInformation(
            "Company Created: {CompanyId} ({CompanyCode} / {CompanyName})",
            created.Id,
            created.CompanyCode,
            created.CompanyName);

        return MapToResponse(created);
    }

    public async Task<CompanyResponse> UpdateAsync(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var company = await GetOrThrowAsync(id, cancellationToken);

        if (!string.Equals(request.CompanyName.Trim(), company.CompanyName, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureUniqueNameAsync(request.CompanyName, company.Id, cancellationToken);
        }

        if (request.DecisionProfileId.HasValue)
        {
            await EnsureDecisionProfileAssignableAsync(company.Id, request.DecisionProfileId.Value, cancellationToken);
        }

        if (request.DefaultPlanningTemplateId.HasValue)
        {
            await EnsurePlanningTemplateAssignableAsync(
                company.Id,
                request.DefaultPlanningTemplateId.Value,
                cancellationToken);
        }

        var previousState = AuditService.SerializeState(new
        {
            company.Id,
            company.CompanyName,
            company.DecisionProfileId,
            company.DefaultPlanningTemplateId
        });

        try
        {
            company.Update(
                request.CompanyName,
                request.LegalName,
                request.Timezone,
                request.Country,
                request.Language,
                request.Currency,
                request.PlanningConfiguration,
                request.DecisionProfileId,
                request.DefaultCalendarId,
                request.DefaultPlanningTemplateId,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _repository.UpdateAsync(company, cancellationToken);

        await RecordAuditAsync(
            updated,
            AuditEventTypes.StatusChanged,
            "Company.Update",
            previousState,
            cancellationToken);

        _logger.LogInformation("Company Updated: {CompanyId} ({CompanyName})", updated.Id, updated.CompanyName);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await GetOrThrowAsync(id, cancellationToken);

        if (company.IsActive)
        {
            return;
        }

        try
        {
            company.Activate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(company, cancellationToken);
        await RecordAuditAsync(
            company,
            AuditEventTypes.Activated,
            "Company.Activate",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Activated: {CompanyId}", company.Id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await GetOrThrowAsync(id, cancellationToken);

        if (company.IsInactive)
        {
            return;
        }

        try
        {
            company.Deactivate(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(company, cancellationToken);
        await RecordAuditAsync(
            company,
            AuditEventTypes.Deactivated,
            "Company.Deactivate",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Deactivated: {CompanyId}", company.Id);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            company.Archive(DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _repository.UpdateAsync(company, cancellationToken);
        await RecordAuditAsync(
            company,
            AuditEventTypes.Archived,
            "Company.Archive",
            previousState: null,
            cancellationToken);

        _logger.LogInformation("Company Archived: {CompanyId}", company.Id);
    }

    private async Task EnsureDecisionProfileAssignableAsync(
        Guid companyId,
        Guid decisionProfileId,
        CancellationToken cancellationToken)
    {
        var profile = await _decisionProfileRepository.GetByIdAsync(decisionProfileId, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException($"Company Decision Profile with id '{decisionProfileId}' was not found.");
        }

        if (profile.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                "DecisionProfileId must reference a Company Decision Profile belonging to this company (BR-2007).");
        }

        if (!profile.IsActive || !profile.DefaultProfile)
        {
            throw new BusinessRuleException(
                "DecisionProfileId must reference the company's Active default Company Decision Profile (BR-2007).");
        }
    }

    private async Task EnsurePlanningTemplateAssignableAsync(
        Guid companyId,
        Guid planningTemplateId,
        CancellationToken cancellationToken)
    {
        var template = await _planningTemplateRepository.GetByIdAsync(planningTemplateId, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Planning Template with id '{planningTemplateId}' was not found.");
        }

        if (template.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                "DefaultPlanningTemplateId must reference a Planning Template belonging to this company (BR-2008).");
        }
    }

    private async Task EnsureUniqueCodeAsync(
        string companyCode,
        Guid? excludeCompanyId,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsCodeAsync(companyCode, excludeCompanyId, cancellationToken))
        {
            throw new ConflictException(
                $"A Company with CompanyCode '{companyCode.Trim()}' already exists (BR-2002).");
        }
    }

    private async Task EnsureUniqueNameAsync(
        string companyName,
        Guid? excludeCompanyId,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsNameAsync(companyName, excludeCompanyId, cancellationToken))
        {
            throw new ConflictException(
                $"A Company with CompanyName '{companyName.Trim()}' already exists (BR-2001).");
        }
    }

    private async Task<Company> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{id}' was not found.");
        }

        return company;
    }

    private async Task RecordAuditAsync(
        Company company,
        string eventType,
        string action,
        string? previousState,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.Company,
                EntityId = company.Id,
                EventType = eventType,
                Action = action,
                CompanyId = company.Id,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                PreviousState = previousState,
                CurrentState = AuditService.SerializeState(new
                {
                    company.Id,
                    company.CompanyCode,
                    company.CompanyName,
                    company.Status,
                    company.DecisionProfileId,
                    company.DefaultPlanningTemplateId
                })
            },
            cancellationToken);
    }

    internal static CompanyResponse MapToResponse(Company company) =>
        new()
        {
            Id = company.Id,
            CompanyCode = company.CompanyCode,
            CompanyName = company.CompanyName,
            LegalName = company.LegalName,
            Status = company.Status,
            Timezone = company.Timezone,
            Country = company.Country,
            Language = company.Language,
            Currency = company.Currency,
            PlanningConfiguration = company.PlanningConfiguration,
            DecisionProfileId = company.DecisionProfileId,
            DefaultCalendarId = company.DefaultCalendarId,
            DefaultPlanningTemplateId = company.DefaultPlanningTemplateId,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            ArchivedAt = company.ArchivedAt
        };
}
