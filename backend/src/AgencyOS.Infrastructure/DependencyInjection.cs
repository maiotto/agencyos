using AgencyOS.Application.Interfaces;
using AgencyOS.Infrastructure.Persistence;
using AgencyOS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();

        return services;
    }
}
