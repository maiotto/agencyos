using AgencyOS.Application.Interfaces;
using AgencyOS.Domain.Entities;
using AgencyOS.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AgencyOS.Infrastructure.Repositories;

public class CompanyDecisionProfileRepository : ICompanyDecisionProfileRepository
{
    private readonly IReadOnlyList<CompanyDecisionProfile> _profiles;

    public CompanyDecisionProfileRepository(IOptions<CompanyDecisionProfilesOptions> options)
    {
        _profiles = options.Value.Profiles
            .Select(MapProfile)
            .ToList();
    }

    public Task<CompanyDecisionProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = _profiles.FirstOrDefault(entry => entry.Id == id);
        return Task.FromResult(profile);
    }

    public Task<IReadOnlyList<CompanyDecisionProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_profiles);
    }

    private static CompanyDecisionProfile MapProfile(CompanyDecisionProfileOptions options)
    {
        return new CompanyDecisionProfile
        {
            Id = options.Id,
            Code = options.Code,
            Name = options.Name,
            Dimensions = options.Dimensions
                .Select(dimension => new DecisionProfileDimensionSetting
                {
                    Dimension = dimension.Dimension,
                    Weight = dimension.Weight,
                    PreferHigherValues = dimension.PreferHigherValues
                })
                .ToList()
        };
    }
}
