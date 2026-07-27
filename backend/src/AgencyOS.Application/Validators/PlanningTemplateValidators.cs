using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class PlanningTemplateQueryParametersValidator : AbstractValidator<PlanningTemplateQueryParameters>
{
    public PlanningTemplateQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Status)
            .Must(status => string.IsNullOrWhiteSpace(status)
                || PlanningTemplateStatus.IsActive(status)
                || PlanningTemplateStatus.IsInactive(status))
            .WithMessage("Status must be Active or Inactive when provided.");

        RuleFor(parameters => parameters.Name).MaximumLength(200);
        RuleFor(parameters => parameters.Search).MaximumLength(200);
    }
}

public class CreatePlanningTemplateRequestValidator : AbstractValidator<CreatePlanningTemplateRequest>
{
    public CreatePlanningTemplateRequestValidator()
    {
        RuleFor(request => request.CompanyId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(1000);
        RuleFor(request => request.WorkingCalendarId).NotEmpty();
        RuleFor(request => request.WorkingHoursId).NotEmpty();
        RuleFor(request => request.ResourceAvailabilityStrategy).NotEmpty().MaximumLength(80);
        RuleFor(request => request.DefaultPlanningWindowDays).GreaterThan(0);
        RuleFor(request => request.DefaultPeriodStartOffsetDays).GreaterThanOrEqualTo(0);
        RuleFor(request => request.UtilizationWarningPercentage)
            .InclusiveBetween(0, 100)
            .When(request => request.UtilizationWarningPercentage.HasValue);
        RuleFor(request => request.PlanningParametersJson).MaximumLength(8000);
    }
}

public class UpdatePlanningTemplateRequestValidator : AbstractValidator<UpdatePlanningTemplateRequest>
{
    public UpdatePlanningTemplateRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(1000);
        RuleFor(request => request.WorkingCalendarId).NotEmpty();
        RuleFor(request => request.WorkingHoursId).NotEmpty();
        RuleFor(request => request.ResourceAvailabilityStrategy).NotEmpty().MaximumLength(80);
        RuleFor(request => request.DefaultPlanningWindowDays).GreaterThan(0);
        RuleFor(request => request.DefaultPeriodStartOffsetDays).GreaterThanOrEqualTo(0);
        RuleFor(request => request.UtilizationWarningPercentage)
            .InclusiveBetween(0, 100)
            .When(request => request.UtilizationWarningPercentage.HasValue);
        RuleFor(request => request.PlanningParametersJson).MaximumLength(8000);
    }
}

public class ClonePlanningTemplateRequestValidator : AbstractValidator<ClonePlanningTemplateRequest>
{
    public ClonePlanningTemplateRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
    }
}

public class ApplyPlanningTemplateRequestValidator : AbstractValidator<ApplyPlanningTemplateRequest>
{
    public ApplyPlanningTemplateRequestValidator()
    {
        RuleFor(request => request)
            .Must(request =>
                !request.PeriodStartDate.HasValue
                || !request.PeriodEndDate.HasValue
                || request.PeriodEndDate >= request.PeriodStartDate)
            .WithMessage("PeriodEndDate cannot be earlier than PeriodStartDate.");
    }
}
