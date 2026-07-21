using AgencyOS.Application.Interfaces;
using AgencyOS.Application.Services;
using AgencyOS.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgencyOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMissionService, MissionService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IClientContractService, ClientContractService>();
        services.AddValidatorsFromAssemblyContaining<CreateMissionRequestValidator>();

        return services;
    }
}
