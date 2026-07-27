using AgencyOS.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AgencyOS.Api.Audit;

/// <summary>
/// Resolves correlation/session/request identifiers from HTTP headers (BR-1508).
/// </summary>
public sealed class HttpAuditContext : IAuditContext
{
    public const string CorrelationHeader = "X-Correlation-Id";
    public const string SessionHeader = "X-Session-Id";
    public const string RequestHeader = "X-Request-Id";
    public const string UserIdHeader = "X-User-Id";
    public const string UserNameHeader = "X-User-Name";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpAuditContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CorrelationId => ParseGuid(GetHeader(CorrelationHeader));

    public string? SessionId => GetHeader(SessionHeader);

    public string? RequestId => GetHeader(RequestHeader);

    public string? UserId => GetHeader(UserIdHeader);

    public string? UserName => GetHeader(UserNameHeader);

    private string? GetHeader(string name)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        if (context.Items.TryGetValue(name, out var item) && item is string itemValue)
        {
            return itemValue;
        }

        return context.Request.Headers.TryGetValue(name, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var parsed) ? parsed : null;
}

/// <summary>
/// Ensures every request has a CorrelationId for audit correlation (BR-1508).
/// </summary>
public sealed class AuditCorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public AuditCorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlation = context.Request.Headers[HttpAuditContext.CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlation) || !Guid.TryParse(correlation, out var correlationId))
        {
            correlationId = Guid.NewGuid();
            correlation = correlationId.ToString();
        }

        context.Items[HttpAuditContext.CorrelationHeader] = correlation;
        context.Response.Headers[HttpAuditContext.CorrelationHeader] = correlation;

        if (!context.Request.Headers.ContainsKey(HttpAuditContext.RequestHeader))
        {
            var requestId = Guid.NewGuid().ToString("N");
            context.Items[HttpAuditContext.RequestHeader] = requestId;
            context.Response.Headers[HttpAuditContext.RequestHeader] = requestId;
        }

        await _next(context);
    }
}
