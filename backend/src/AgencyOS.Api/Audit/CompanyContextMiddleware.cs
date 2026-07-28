using AgencyOS.Application.Interfaces;
using AgencyOS.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace AgencyOS.Api.Audit;

/// <summary>
/// Resolves the active Company for the current request from the `X-Company-Id` header (US-402 / BR-2003).
/// When the header is absent the context is left empty; services fall back to the default Company where
/// applicable. When the header is present but references an unknown or non-selectable Company, the
/// request is rejected with 400 Bad Request.
/// </summary>
public sealed class CompanyContextMiddleware
{
    public const string CompanyIdHeader = "X-Company-Id";

    private readonly RequestDelegate _next;

    public CompanyContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICompanyContextService companyContextService)
    {
        var headerValue = context.Request.Headers[CompanyIdHeader].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            if (!Guid.TryParse(headerValue, out var companyId))
            {
                await WriteBadRequestAsync(context, $"'{CompanyIdHeader}' header is not a valid identifier.");
                return;
            }

            try
            {
                await companyContextService.BindContextAsync(companyId, context.RequestAborted);
            }
            catch (NotFoundException ex)
            {
                await WriteBadRequestAsync(context, ex.Message);
                return;
            }
            catch (BusinessRuleException ex)
            {
                await WriteBadRequestAsync(context, ex.Message);
                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteBadRequestAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title = "Invalid Company context",
            detail,
            status = StatusCodes.Status400BadRequest
        });
    }
}
