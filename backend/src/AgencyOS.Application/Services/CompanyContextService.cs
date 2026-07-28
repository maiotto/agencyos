using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Shared.Exceptions;

namespace AgencyOS.Application.Services;

/// <summary>
/// Resolves the active Company context for the current scope (US-402 / BR-2003).
/// </summary>
public class CompanyContextService : ICompanyContextService
{
    private readonly ICompanyRepository _repository;
    private readonly ICompanyContext _context;
    private readonly INotificationGenerationService _notificationGenerationService;
    private readonly IAuditContext _auditContext;

    public CompanyContextService(
        ICompanyRepository repository,
        ICompanyContext context,
        INotificationGenerationService notificationGenerationService,
        IAuditContext auditContext)
    {
        _repository = repository;
        _context = context;
        _notificationGenerationService = notificationGenerationService;
        _auditContext = auditContext;
    }

    public async Task SelectAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var company = await BindCompanyCoreAsync(companyId, cancellationToken);

        await _notificationGenerationService.GenerateSafeAsync(
            new NotificationGenerationRequest
            {
                CompanyId = company.Id,
                UserId = string.IsNullOrWhiteSpace(_auditContext.UserId) ? "system" : _auditContext.UserId,
                Title = "Company context selected",
                Message = $"Active company set to '{company.CompanyName}'.",
                Category = NotificationCategory.Company,
                Priority = NotificationPriority.Low,
                SourceEntity = NotificationSourceEntities.CompanyContext,
                SourceEntityId = company.Id
            },
            cancellationToken);
    }

    public Task BindContextAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        BindCompanyCoreAsync(companyId, cancellationToken);

    private async Task<Company> BindCompanyCoreAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await _repository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with id '{companyId}' was not found.");
        }

        try
        {
            company.Select();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        _context.CompanyId = company.Id;
        _context.CompanyCode = company.CompanyCode;
        _context.CompanyName = company.CompanyName;
        _context.IsSelected = true;

        return company;
    }

    public async Task<CompanyResponse> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        Company? company = null;

        if (_context.IsSelected && _context.CompanyId.HasValue)
        {
            company = await _repository.GetByIdAsync(_context.CompanyId.Value, cancellationToken);
        }

        company ??= await _repository.GetByIdAsync(AgencyOSCompanies.DefaultCompanyId, cancellationToken);

        if (company is null)
        {
            throw new NotFoundException(
                "No Company is selected and the default Company could not be found.");
        }

        return CompanyService.MapToResponse(company);
    }

    public void Clear()
    {
        _context.CompanyId = null;
        _context.CompanyCode = null;
        _context.CompanyName = null;
        _context.IsSelected = false;
    }
}
