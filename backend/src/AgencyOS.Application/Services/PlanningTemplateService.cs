using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace AgencyOS.Application.Services;

public class PlanningTemplateService : IPlanningTemplateService
{
    private readonly IPlanningTemplateRepository _templateRepository;
    private readonly IWorkingCalendarRepository _workingCalendarRepository;
    private readonly IWorkingHoursRepository _workingHoursRepository;
    private readonly ICapacityCalculatorService _capacityCalculatorService;
    private readonly IWorkloadCalculatorService _workloadCalculatorService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PlanningTemplateService> _logger;

    public PlanningTemplateService(
        IPlanningTemplateRepository templateRepository,
        IWorkingCalendarRepository workingCalendarRepository,
        IWorkingHoursRepository workingHoursRepository,
        ICapacityCalculatorService capacityCalculatorService,
        IWorkloadCalculatorService workloadCalculatorService,
        IAuditService auditService,
        ILogger<PlanningTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _workingCalendarRepository = workingCalendarRepository;
        _workingHoursRepository = workingHoursRepository;
        _capacityCalculatorService = capacityCalculatorService;
        _workloadCalculatorService = workloadCalculatorService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlanningTemplateResponse>> GetAllAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var templates = await _templateRepository.GetAllAsync(parameters, cancellationToken);
        return templates.Select(MapToResponse).ToList();
    }

    public Task<IReadOnlyList<PlanningTemplateResponse>> FilterAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(parameters, cancellationToken);

    public async Task<PlanningTemplateResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);
        return MapToResponse(template);
    }

    public async Task<PlanningTemplateResponse> CreateAsync(
        CreatePlanningTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(request.CompanyId, request.Name, excludeTemplateId: null, cancellationToken);
        await EnsureReferencedConfigurationAsync(
            request.CompanyId,
            request.WorkingCalendarId,
            request.WorkingHoursId,
            requireActive: false,
            cancellationToken);

        PlanningTemplate template;
        try
        {
            template = PlanningTemplate.Create(
                request.CompanyId,
                request.Name,
                request.Description,
                request.WorkingCalendarId,
                request.WorkingHoursId,
                request.ResourceAvailabilityStrategy,
                request.DefaultPlanningWindowDays,
                request.DefaultPeriodStartOffsetDays,
                new PlanningTemplateCapacityRules(
                    request.UtilizationWarningPercentage,
                    request.IncludeAssignmentDistribution),
                request.PlanningParametersJson,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _templateRepository.AddAsync(template, cancellationToken);

        _logger.LogInformation(
            "Planning Template Created: {TemplateId} ({Name}) Company={CompanyId}",
            created.Id,
            created.Name,
            created.CompanyId);

        return MapToResponse(created);
    }

    public async Task<PlanningTemplateResponse> UpdateAsync(
        Guid id,
        UpdatePlanningTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);

        await EnsureUniqueNameAsync(template.CompanyId, request.Name, template.Id, cancellationToken);
        await EnsureReferencedConfigurationAsync(
            template.CompanyId,
            request.WorkingCalendarId,
            request.WorkingHoursId,
            requireActive: template.IsActive,
            cancellationToken);

        try
        {
            template.Reconfigure(
                request.Name,
                request.Description,
                request.WorkingCalendarId,
                request.WorkingHoursId,
                request.ResourceAvailabilityStrategy,
                request.DefaultPlanningWindowDays,
                request.DefaultPeriodStartOffsetDays,
                new PlanningTemplateCapacityRules(
                    request.UtilizationWarningPercentage,
                    request.IncludeAssignmentDistribution),
                request.PlanningParametersJson,
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var updated = await _templateRepository.UpdateAsync(template, cancellationToken);

        _logger.LogInformation("Planning Template Updated: {TemplateId} ({Name})", updated.Id, updated.Name);

        return MapToResponse(updated);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);

        if (template.IsActive)
        {
            return;
        }

        await EnsureReferencedConfigurationAsync(
            template.CompanyId,
            template.WorkingCalendarId,
            template.WorkingHoursId,
            requireActive: true,
            cancellationToken);

        template.Activate(DateTimeOffset.UtcNow);
        await _templateRepository.UpdateAsync(template, cancellationToken);
        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.PlanningTemplate,
                EntityId = template.Id,
                EventType = AuditEventTypes.Activated,
                Action = "PlanningTemplate.Activate",
                CompanyId = template.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new { template.Id, template.Name, isActive = true })
            },
            cancellationToken);

        _logger.LogInformation("Planning Template Activated: {TemplateId}", template.Id);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);

        if (template.IsInactive)
        {
            return;
        }

        template.Deactivate(DateTimeOffset.UtcNow);
        await _templateRepository.UpdateAsync(template, cancellationToken);

        _logger.LogInformation("Planning Template Deactivated: {TemplateId}", template.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            template.EnsureCanDelete();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await _templateRepository.DeleteAsync(template, cancellationToken);

        _logger.LogInformation("Planning Template Deleted: {TemplateId}", template.Id);
    }

    public async Task<PlanningTemplateResponse> CloneAsync(
        Guid id,
        ClonePlanningTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await GetOrThrowAsync(id, cancellationToken);

        await EnsureUniqueNameAsync(source.CompanyId, request.Name, excludeTemplateId: null, cancellationToken);

        PlanningTemplate clone;
        try
        {
            clone = source.Clone(request.Name, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        var created = await _templateRepository.AddAsync(clone, cancellationToken);

        _logger.LogInformation(
            "Planning Template Cloned: Source={SourceTemplateId} Clone={CloneTemplateId} ({Name})",
            source.Id,
            created.Id,
            created.Name);

        return MapToResponse(created);
    }

    public async Task<AppliedPlanningConfigurationResponse> ApplyAsync(
        Guid id,
        ApplyPlanningTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await GetOrThrowAsync(id, cancellationToken);

        try
        {
            template.EnsureCanApply();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        await EnsureReferencedConfigurationAsync(
            template.CompanyId,
            template.WorkingCalendarId,
            template.WorkingHoursId,
            requireActive: true,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        DateOnly periodStart;
        DateOnly periodEnd;

        if (request.PeriodStartDate.HasValue && request.PeriodEndDate.HasValue)
        {
            periodStart = request.PeriodStartDate.Value;
            periodEnd = request.PeriodEndDate.Value;
        }
        else if (request.PeriodStartDate.HasValue)
        {
            periodStart = request.PeriodStartDate.Value;
            periodEnd = periodStart.AddDays(template.DefaultPlanningWindowDays - 1);
        }
        else
        {
            (periodStart, periodEnd) = template.ResolveDefaultPlanningWindow(today);
        }

        if (periodEnd < periodStart)
        {
            throw new BusinessRuleException("PeriodEndDate cannot be earlier than PeriodStartDate.");
        }

        // BR-807: template is not modified. BR-808: produce a new planning configuration.
        var applied = new AppliedPlanningConfigurationResponse
        {
            SourceTemplateId = template.Id,
            SourceTemplateName = template.Name,
            CompanyId = template.CompanyId,
            WorkingCalendarId = template.WorkingCalendarId,
            WorkingHoursId = template.WorkingHoursId,
            ResourceAvailabilityStrategy = template.ResourceAvailabilityStrategy,
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            UtilizationWarningPercentage = template.UtilizationWarningPercentage,
            IncludeAssignmentDistribution = template.IncludeAssignmentDistribution,
            PlanningParametersJson = template.PlanningParametersJson,
            ExecutionResourceId = request.ExecutionResourceId,
            ExcludeMissionId = request.ExcludeMissionId
        };

        var capacityParameters = new CapacityQueryParameters
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            ExcludeMissionId = request.ExcludeMissionId
        };

        var workloadParameters = new WorkloadQueryParameters
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            ExcludeMissionId = request.ExcludeMissionId
        };

        if (request.CalculateCapacity)
        {
            if (request.ExecutionResourceId.HasValue)
            {
                var capacity = await _capacityCalculatorService.GetByResourceIdAsync(
                    request.ExecutionResourceId.Value,
                    capacityParameters,
                    cancellationToken);
                applied.CapacityResults = [capacity];
            }
            else
            {
                applied.CapacityResults = await _capacityCalculatorService.GetAllAsync(
                    capacityParameters,
                    cancellationToken);
                applied.CapacitySummary = await _capacityCalculatorService.GetSummaryAsync(
                    capacityParameters,
                    cancellationToken);
            }
        }

        if (request.CalculateWorkload)
        {
            if (request.ExecutionResourceId.HasValue)
            {
                var workload = await _workloadCalculatorService.GetByResourceIdAsync(
                    request.ExecutionResourceId.Value,
                    workloadParameters,
                    cancellationToken);
                applied.WorkloadResults = [workload];
            }
            else
            {
                applied.WorkloadResults = await _workloadCalculatorService.GetAllAsync(
                    workloadParameters,
                    cancellationToken);
                applied.WorkloadSummary = await _workloadCalculatorService.GetSummaryAsync(
                    workloadParameters,
                    cancellationToken);
            }
        }

        _logger.LogInformation(
            "Planning Template Applied: {TemplateId} Period={PeriodStart}-{PeriodEnd} Capacity={CalculateCapacity} Workload={CalculateWorkload}",
            template.Id,
            periodStart,
            periodEnd,
            request.CalculateCapacity,
            request.CalculateWorkload);

        await _auditService.RecordSafeAsync(
            new AuditEventWriteRequest
            {
                EntityType = AuditEntityTypes.PlanningTemplate,
                EntityId = template.Id,
                EventType = AuditEventTypes.Applied,
                Action = "PlanningTemplate.Apply",
                CompanyId = template.CompanyId,
                UserId = "system",
                UserName = "system",
                Source = AuditSources.Api,
                CurrentState = AuditService.SerializeState(new
                {
                    template.Id,
                    template.Name,
                    periodStart,
                    periodEnd,
                    request.CalculateCapacity,
                    request.CalculateWorkload
                })
            },
            cancellationToken);

        return applied;
    }

    private async Task EnsureUniqueNameAsync(
        Guid companyId,
        string name,
        Guid? excludeTemplateId,
        CancellationToken cancellationToken)
    {
        if (await _templateRepository.ExistsByCompanyAndNameAsync(
                companyId,
                name,
                excludeTemplateId,
                cancellationToken))
        {
            throw new ConflictException(
                $"A Planning Template named '{name.Trim()}' already exists for this company.");
        }
    }

    private async Task EnsureReferencedConfigurationAsync(
        Guid companyId,
        Guid workingCalendarId,
        Guid workingHoursId,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var calendar = await _workingCalendarRepository.GetByIdAsync(workingCalendarId, cancellationToken);
        if (calendar is null)
        {
            throw new NotFoundException($"Working Calendar with id '{workingCalendarId}' was not found.");
        }

        if (calendar.CompanyId != companyId)
        {
            throw new BusinessRuleException(
                "Working Calendar must belong to the same company as the Planning Template.");
        }

        if (requireActive && !calendar.IsActive)
        {
            throw new BusinessRuleException(
                "Template must reference an Active Working Calendar.");
        }

        var workingHours = await _workingHoursRepository.GetByIdAsync(workingHoursId, cancellationToken);
        if (workingHours is null)
        {
            throw new NotFoundException($"Working Hours with id '{workingHoursId}' was not found.");
        }

        if (workingHours.WorkingCalendarId != workingCalendarId)
        {
            throw new BusinessRuleException(
                "Working Hours must belong to the referenced Working Calendar.");
        }

        if (requireActive && !workingHours.IsActive)
        {
            throw new BusinessRuleException(
                "Template must reference Active Working Hours.");
        }
    }

    private async Task<PlanningTemplate> GetOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            throw new NotFoundException($"Planning Template with id '{id}' was not found.");
        }

        return template;
    }

    internal static PlanningTemplateResponse MapToResponse(PlanningTemplate template) =>
        new()
        {
            Id = template.Id,
            CompanyId = template.CompanyId,
            Name = template.Name,
            Description = template.Description,
            Status = template.Status,
            WorkingCalendarId = template.WorkingCalendarId,
            WorkingHoursId = template.WorkingHoursId,
            ResourceAvailabilityStrategy = template.ResourceAvailabilityStrategy,
            DefaultPlanningWindowDays = template.DefaultPlanningWindowDays,
            DefaultPeriodStartOffsetDays = template.DefaultPeriodStartOffsetDays,
            UtilizationWarningPercentage = template.UtilizationWarningPercentage,
            IncludeAssignmentDistribution = template.IncludeAssignmentDistribution,
            PlanningParametersJson = template.PlanningParametersJson,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
}
