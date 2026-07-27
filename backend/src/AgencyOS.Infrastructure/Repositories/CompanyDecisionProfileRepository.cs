using AgencyOS.Application.DTOs;
using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyOS.Infrastructure.Repositories;

public class CompanyDecisionProfileRepository : ICompanyDecisionProfileRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyDecisionProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyDecisionProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyDecisionProfiles
            .FirstOrDefaultAsync(profile => profile.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompanyDecisionProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompanyDecisionProfile>> QueryAsync(
        CompanyDecisionProfileQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var all = await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var latestPerFamily = LatestVersionPerFamily(all);

        IEnumerable<CompanyDecisionProfile> query = latestPerFamily;

        if (parameters.CompanyId.HasValue)
        {
            var companyId = parameters.CompanyId.Value;
            query = query.Where(profile => profile.CompanyId == companyId);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            var status = parameters.Status.Trim();
            query = query.Where(profile => string.Equals(profile.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Name))
        {
            var name = parameters.Name.Trim();
            query = query.Where(profile =>
                profile.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Code))
        {
            var code = parameters.Code.Trim();
            query = query.Where(profile =>
                string.Equals(profile.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.DefaultProfile.HasValue)
        {
            var defaultProfile = parameters.DefaultProfile.Value;
            query = query.Where(profile => profile.DefaultProfile == defaultProfile);
        }

        return ApplyOrdering(query, parameters).ToList();
    }

    public async Task<IReadOnlyList<CompanyDecisionProfile>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsync(
            new CompanyDecisionProfileQueryParameters { CompanyId = companyId },
            cancellationToken);
    }

    public async Task<CompanyDecisionProfile?> GetDefaultActiveAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyId == companyId)
            .Where(profile => profile.DefaultProfile)
            .Where(profile => profile.Status == CompanyDecisionProfileStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanyDecisionProfile?> GetLatestByFamilyAsync(
        Guid profileFamilyId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .Where(profile => profile.ProfileFamilyId == profileFamilyId)
            .OrderByDescending(profile => profile.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveNameAsync(
        Guid companyId,
        string name,
        Guid? excludeProfileFamilyId = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        var latestPerFamily = LatestVersionPerFamily(all);
        var normalizedName = name.Trim();

        return latestPerFamily.Any(profile =>
            profile.IsActive
            && (!excludeProfileFamilyId.HasValue || profile.ProfileFamilyId != excludeProfileFamilyId.Value)
            && string.Equals(profile.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsCodeAsync(
        Guid companyId,
        string code,
        Guid? excludeProfileFamilyId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();

        return await _context.CompanyDecisionProfiles
            .AsNoTracking()
            .Where(profile => profile.CompanyId == companyId)
            .Where(profile => !excludeProfileFamilyId.HasValue || profile.ProfileFamilyId != excludeProfileFamilyId.Value)
            .AnyAsync(
                profile => profile.Code.ToLower() == normalizedCode.ToLower(),
                cancellationToken);
    }

    public async Task<CompanyDecisionProfile> AddAsync(
        CompanyDecisionProfile profile,
        CancellationToken cancellationToken = default)
    {
        _context.CompanyDecisionProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<CompanyDecisionProfile> UpdateAsync(
        CompanyDecisionProfile profile,
        CancellationToken cancellationToken = default)
    {
        _context.CompanyDecisionProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private static IReadOnlyList<CompanyDecisionProfile> LatestVersionPerFamily(
        IReadOnlyList<CompanyDecisionProfile> profiles)
    {
        return profiles
            .GroupBy(profile => profile.ProfileFamilyId)
            .Select(group => group.OrderByDescending(profile => profile.Version).First())
            .ToList();
    }

    private static IEnumerable<CompanyDecisionProfile> ApplyOrdering(
        IEnumerable<CompanyDecisionProfile> query,
        CompanyDecisionProfileQueryParameters parameters)
    {
        var descending = string.Equals(parameters.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(parameters.OrderBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(profile => profile.Status).ThenBy(profile => profile.Name)
                : query.OrderBy(profile => profile.Status).ThenBy(profile => profile.Name);
        }

        if (string.Equals(parameters.OrderBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return descending
                ? query.OrderByDescending(profile => profile.CreatedAt)
                : query.OrderBy(profile => profile.CreatedAt);
        }

        return descending
            ? query.OrderByDescending(profile => profile.Name)
            : query.OrderBy(profile => profile.Name);
    }
}
