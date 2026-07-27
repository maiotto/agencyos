using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class PortfolioQueryParametersValidator : AbstractValidator<PortfolioQueryParameters>
{
    public PortfolioQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Search).MaximumLength(300);
        RuleFor(parameters => parameters.OrderDirection)
            .Must(value => value is null
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderDirection must be 'asc' or 'desc'.");
    }
}

public class CreatePortfolioRequestValidator : AbstractValidator<CreatePortfolioRequest>
{
    public CreatePortfolioRequestValidator()
    {
        RuleFor(request => request.CompanyId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000).When(request => request.Description is not null);
        RuleFor(request => request.PlanningPeriodEnd)
            .GreaterThanOrEqualTo(request => request.PlanningPeriodStart)
            .WithMessage("PlanningPeriodEnd cannot be earlier than PlanningPeriodStart.");
        RuleFor(request => request.Missions)
            .NotEmpty()
            .WithMessage("Portfolio must contain at least one Mission.");
        RuleForEach(request => request.Missions).ChildRules(mission =>
        {
            mission.RuleFor(item => item.MissionId).NotEmpty();
            mission.RuleFor(item => item.Priority).GreaterThanOrEqualTo(1);
        });
    }
}

public class UpdatePortfolioRequestValidator : AbstractValidator<UpdatePortfolioRequest>
{
    public UpdatePortfolioRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000).When(request => request.Description is not null);
        RuleFor(request => request.PlanningPeriodEnd)
            .GreaterThanOrEqualTo(request => request.PlanningPeriodStart)
            .WithMessage("PlanningPeriodEnd cannot be earlier than PlanningPeriodStart.");
    }
}

public class PortfolioMissionRequestValidator : AbstractValidator<PortfolioMissionRequest>
{
    public PortfolioMissionRequestValidator()
    {
        RuleFor(request => request.MissionId).NotEmpty();
        RuleFor(request => request.Priority).GreaterThanOrEqualTo(1);
    }
}
