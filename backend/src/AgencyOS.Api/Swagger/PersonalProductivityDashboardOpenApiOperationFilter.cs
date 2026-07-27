using AgencyOS.Api.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AgencyOS.Api.Swagger;

public sealed class PersonalProductivityDashboardOpenApiOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PersonalProductivityDashboardController))
        {
            return;
        }

        operation.Summary ??= "Personal Productivity Dashboard";
        operation.Description ??=
            "Read-only Personal Productivity Dashboard (US-507 / BR-3001..BR-3010). "
            + "Analytical projections over existing Assignment/Task/Mission/Recommendation/Decision/"
            + "Capacity/Workload/Audit data. Identity resolves via DEC-501-001. Usage is audited (BR-3010). "
            + "Never modifies operational behavior.";
    }
}
