using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Notification>> QueryAsync(
        NotificationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(parameters);
        query = ApplyOrdering(query, parameters);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<int> CountUnreadAsync(
        Guid companyId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(
                item => item.CompanyId == companyId
                    && item.UserId == userId
                    && item.Status == NotificationStatus.Unread
                    && !item.Archived,
                cancellationToken);
    }

    public async Task<Notification> AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    public async Task<Notification> UpdateAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(notification);
        if (entry.State == EntityState.Detached)
        {
            _context.Notifications.Attach(notification);
            entry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return notification;
    }

    private IQueryable<Notification> BuildFilteredQuery(NotificationQueryParameters parameters)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (parameters.CompanyId.HasValue)
        {
            query = query.Where(item => item.CompanyId == parameters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.UserId))
        {
            var userId = parameters.UserId.Trim();
            query = query.Where(item => item.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var category = parameters.Category.Trim().ToLowerInvariant();
            query = query.Where(item => item.Category.ToLower() == category);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Priority))
        {
            var priority = parameters.Priority.Trim().ToLowerInvariant();
            query = query.Where(item => item.Priority.ToLower() == priority);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim().ToLowerInvariant();
            query = query.Where(item => item.Status.ToLower() == status);
        }

        if (parameters.Archived.HasValue)
        {
            query = query.Where(item => item.Archived == parameters.Archived.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SourceEntity))
        {
            var sourceEntity = parameters.SourceEntity.Trim().ToLowerInvariant();
            query = query.Where(item => item.SourceEntity.ToLower() == sourceEntity);
        }

        if (parameters.SourceEntityId.HasValue)
        {
            query = query.Where(item => item.SourceEntityId == parameters.SourceEntityId.Value);
        }

        if (parameters.CreatedFrom.HasValue)
        {
            query = query.Where(item => item.CreatedAt >= parameters.CreatedFrom.Value);
        }

        if (parameters.CreatedTo.HasValue)
        {
            query = query.Where(item => item.CreatedAt <= parameters.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(item =>
                item.Title.ToLower().Contains(search)
                || item.Message.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<Notification> ApplyOrdering(
        IQueryable<Notification> query,
        NotificationQueryParameters parameters)
    {
        var descending = !string.Equals(
            parameters.OrderDirection,
            "asc",
            StringComparison.OrdinalIgnoreCase);

        return (parameters.OrderBy?.Trim().ToLowerInvariant()) switch
        {
            "title" => descending
                ? query.OrderByDescending(item => item.Title)
                : query.OrderBy(item => item.Title),
            "priority" => descending
                ? query.OrderByDescending(item => item.Priority)
                : query.OrderBy(item => item.Priority),
            "status" => descending
                ? query.OrderByDescending(item => item.Status)
                : query.OrderBy(item => item.Status),
            "category" => descending
                ? query.OrderByDescending(item => item.Category)
                : query.OrderBy(item => item.Category),
            _ => descending
                ? query.OrderByDescending(item => item.CreatedAt)
                : query.OrderBy(item => item.CreatedAt)
        };
    }
}
