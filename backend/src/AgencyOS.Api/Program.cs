using AgencyOS.Application;
using AgencyOS.Api.Audit;
using AgencyOS.Api.ModelBinding;
using AgencyOS.Api.Swagger;
using AgencyOS.Application.Interfaces;
using AgencyOS.Infrastructure;
using AgencyOS.Shared.Exceptions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new GuidListModelBinderProvider());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "AgencyOS API",
        Version = "v1",
        Description = "AgencyOS MVP 1.0 REST API"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.OperationFilter<LeadOpenApiOperationFilter>();
    options.OperationFilter<ClientOpenApiOperationFilter>();
    options.OperationFilter<ContactOpenApiOperationFilter>();
    options.OperationFilter<ContractOpenApiOperationFilter>();
    options.OperationFilter<MissionOpenApiOperationFilter>();
    options.OperationFilter<TaskOpenApiOperationFilter>();
    options.OperationFilter<ExecutionResourceOpenApiOperationFilter>();
    options.OperationFilter<WorkingCalendarOpenApiOperationFilter>();
    options.OperationFilter<HolidayOpenApiOperationFilter>();
    options.OperationFilter<WorkingHoursOpenApiOperationFilter>();
    options.OperationFilter<ResourceAvailabilityOpenApiOperationFilter>();
    options.OperationFilter<PlanningTemplateOpenApiOperationFilter>();
    options.OperationFilter<PortfolioOpenApiOperationFilter>();
    options.OperationFilter<RecommendationWorkflowOpenApiOperationFilter>();
    options.OperationFilter<RecommendationOpenApiOperationFilter>();
    options.OperationFilter<RecommendationHistoryOpenApiOperationFilter>();
    options.OperationFilter<RecommendationComparisonOpenApiOperationFilter>();
    options.OperationFilter<DecisionOpenApiOperationFilter>();
    options.OperationFilter<AuditOpenApiOperationFilter>();
    options.OperationFilter<AIRecommendationOpenApiOperationFilter>();
    options.OperationFilter<ExplainabilityOpenApiOperationFilter>();
    options.OperationFilter<ExecutiveRecommendationSummaryOpenApiOperationFilter>();
    options.OperationFilter<AssignmentOpenApiOperationFilter>();
    options.OperationFilter<CapacityOpenApiOperationFilter>();
    options.OperationFilter<WorkloadOpenApiOperationFilter>();
    options.OperationFilter<AvailabilityOpenApiOperationFilter>();
    options.OperationFilter<AllocationConflictOpenApiOperationFilter>();
    options.OperationFilter<DeliveryStrategyOpenApiOperationFilter>();
    options.OperationFilter<DecisionProfileOpenApiOperationFilter>();
    options.OperationFilter<CompanyOpenApiOperationFilter>();
    options.OperationFilter<EnterpriseDashboardOpenApiOperationFilter>();
    options.OperationFilter<PortfolioAnalyticsOpenApiOperationFilter>();
    options.OperationFilter<CrossPortfolioPlanningOpenApiOperationFilter>();
    options.OperationFilter<MyWorkDashboardOpenApiOperationFilter>();
    options.OperationFilter<PlanningWorkspaceOpenApiOperationFilter>();
    options.OperationFilter<RecommendationWorkspaceOpenApiOperationFilter>();
    options.OperationFilter<DecisionWorkspaceOpenApiOperationFilter>();
    options.OperationFilter<ExecutiveWorkspaceOpenApiOperationFilter>();
    options.OperationFilter<NotificationOpenApiOperationFilter>();
    options.OperationFilter<PersonalProductivityDashboardOpenApiOperationFilter>();
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.RemoveAll<IAuditContext>();
builder.Services.AddScoped<IAuditContext, HttpAuditContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AgencyOS.Validation");

        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                $"{entry.Key}: {error.ErrorMessage}"))
            .ToArray();

        logger.LogWarning(
            "Validation failure for {Method} {Path}. Errors: {Errors}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value,
            errors);

        return new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
    };
});

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AgencyOS.ExceptionHandler");

        if (exception is NotFoundException notFoundException)
        {
            logger.LogWarning(
                notFoundException,
                "Resource not found for {Method} {Path}",
                context.Request.Method,
                context.Request.Path.Value);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Resource not found",
                Detail = notFoundException.Message,
                Status = StatusCodes.Status404NotFound
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        if (exception is ConflictException conflictException)
        {
            logger.LogWarning(
                conflictException,
                "Conflict for {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path.Value,
                conflictException.Message);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Conflict",
                Detail = conflictException.Message,
                Status = StatusCodes.Status409Conflict
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        if (exception is BusinessRuleException businessRuleException)
        {
            logger.LogWarning(
                businessRuleException,
                "Business rule violation for {Method} {Path}: {Message}",
                context.Request.Method,
                context.Request.Path.Value,
                businessRuleException.Message);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Business rule violation",
                Detail = businessRuleException.Message,
                Status = StatusCodes.Status400BadRequest
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        logger.LogError(
            exception,
            "Unexpected exception for {Method} {Path}",
            context.Request.Method,
            context.Request.Path.Value);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var internalProblem = new ProblemDetails
        {
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(internalProblem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendDev");
app.UseMiddleware<AuditCorrelationMiddleware>();
app.UseMiddleware<CompanyContextMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
