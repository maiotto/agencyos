using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class CapacityHistoryRepository : ICapacityHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public CapacityHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CapacityHistory> AddAsync(
        CapacityHistory history,
        CancellationToken cancellationToken = default)
    {
        _context.CapacityHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
        return history;
    }

    public async Task AddRangeAsync(
        IReadOnlyList<CapacityHistory> histories,
        CancellationToken cancellationToken = default)
    {
        _context.CapacityHistories.AddRange(histories);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CapacityHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CapacityHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(history => history.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CapacityHistory>> QueryAsync(
        CapacityHistoryQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return await BuildFilteredQuery(parameters)
            .OrderByDescending(history => history.CalculationDate)
            .ThenBy(history => history.ExecutionResourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CapacityHistory>> GetByExecutionResourceIdAsync(
        Guid executionResourceId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new CapacityHistoryQueryParameters();
        parameters.ExecutionResourceId = executionResourceId;

        return await QueryAsync(parameters, cancellationToken);
    }

    public async Task<IReadOnlyList<CapacityHistory>> GetByCompanyIdAsync(
        Guid companyId,
        CapacityHistoryQueryParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new CapacityHistoryQueryParameters();
        parameters.CompanyId = companyId;

        return await QueryAsync(parameters, cancellationToken);
    }

    private IQueryable<CapacityHistory> BuildFilteredQuery(CapacityHistoryQueryParameters parameters)
    {
        var query = _context.CapacityHistories.AsNoTracking().AsQueryable();

        if (parameters.ExecutionResourceId.HasValue)
        {
            query = query.Where(history => history.ExecutionResourceId == parameters.ExecutionResourceId.Value);
        }

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(history => history.CompanyId == parameters.CompanyId.Value);
        }

        if (parameters.PeriodStart.HasValue)
        {
            query = query.Where(history => history.PeriodEnd >= parameters.PeriodStart.Value);
        }

        if (parameters.PeriodEnd.HasValue)
        {
            query = query.Where(history => history.PeriodStart <= parameters.PeriodEnd.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.CalculationVersion))
        {
            var version = parameters.CalculationVersion.Trim();
            query = query.Where(history => history.CalculationVersion == version);
        }

        if (parameters.CalculatedFrom.HasValue)
        {
            query = query.Where(history => history.CalculationDate >= parameters.CalculatedFrom.Value);
        }

        if (parameters.CalculatedTo.HasValue)
        {
            query = query.Where(history => history.CalculationDate <= parameters.CalculatedTo.Value);
        }

        return query;
    }
}
