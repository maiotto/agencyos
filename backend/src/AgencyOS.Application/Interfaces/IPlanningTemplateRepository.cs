using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Interfaces;

public interface IPlanningTemplateRepository
{
    Task<IReadOnlyList<PlanningTemplate>> GetAllAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PlanningTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCompanyAndNameAsync(
        Guid companyId,
        string name,
        Guid? excludeTemplateId = null,
        CancellationToken cancellationToken = default);

    Task<PlanningTemplate> AddAsync(PlanningTemplate template, CancellationToken cancellationToken = default);

    Task<PlanningTemplate> UpdateAsync(PlanningTemplate template, CancellationToken cancellationToken = default);

    Task DeleteAsync(PlanningTemplate template, CancellationToken cancellationToken = default);
}
