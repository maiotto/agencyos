using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class PlanningTemplateRepository : IPlanningTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public PlanningTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlanningTemplate>> GetAllAsync(
        PlanningTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PlanningTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlanningTemplates
            .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCompanyAndNameAsync(
        Guid companyId,
        string name,
        Guid? excludeTemplateId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var query = _context.PlanningTemplates.AsNoTracking()
            .Where(template => template.CompanyId == companyId)
            .Where(template => template.Name.ToLower() == normalized);

        if (excludeTemplateId.HasValue)
        {
            query = query.Where(template => template.Id != excludeTemplateId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<PlanningTemplate> AddAsync(
        PlanningTemplate template,
        CancellationToken cancellationToken = default)
    {
        _context.PlanningTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<PlanningTemplate> UpdateAsync(
        PlanningTemplate template,
        CancellationToken cancellationToken = default)
    {
        _context.PlanningTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task DeleteAsync(PlanningTemplate template, CancellationToken cancellationToken = default)
    {
        _context.PlanningTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<PlanningTemplate> BuildFilteredQuery(PlanningTemplateQueryParameters parameters)
    {
        var query = _context.PlanningTemplates.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(template => template.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim().ToLowerInvariant();
            query = query.Where(template => template.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(template =>
                template.Name.ToLower().Contains(search)
                || (template.Description != null && template.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(template => template.Status.ToLower() == status);
        }

        if (parameters.WorkingCalendarId.HasValue)
        {
            query = query.Where(template => template.WorkingCalendarId == parameters.WorkingCalendarId.Value);
        }

        if (parameters.WorkingHoursId.HasValue)
        {
            query = query.Where(template => template.WorkingHoursId == parameters.WorkingHoursId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ResourceAvailabilityStrategy))
        {
            var strategy = parameters.ResourceAvailabilityStrategy.Trim().ToLowerInvariant();
            query = query.Where(template =>
                template.ResourceAvailabilityStrategy.ToLower() == strategy);
        }

        return query;
    }

    private static IQueryable<PlanningTemplate> ApplyOrdering(
        IQueryable<PlanningTemplate> query,
        PlanningTemplateQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var orderBy = parameters.OrderBy?.Trim().ToLowerInvariant();

        return orderBy switch
        {
            "name" => descending
                ? query.OrderByDescending(template => template.Name)
                : query.OrderBy(template => template.Name),
            "status" => descending
                ? query.OrderByDescending(template => template.Status)
                : query.OrderBy(template => template.Status),
            "updatedat" => descending
                ? query.OrderByDescending(template => template.UpdatedAt)
                : query.OrderBy(template => template.UpdatedAt),
            _ => descending
                ? query.OrderByDescending(template => template.CreatedAt)
                : query.OrderBy(template => template.CreatedAt)
        };
    }
}
