using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class CrossPortfolioPlanningQueryParametersValidator : AbstractValidator<CrossPortfolioPlanningQueryParameters>
{
    public CrossPortfolioPlanningQueryParametersValidator()
    {
        RuleFor(parameters => parameters)
            .Must(parameters => !parameters.From.HasValue || !parameters.To.HasValue || parameters.To >= parameters.From)
            .WithMessage("To cannot be earlier than From.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.PeriodStart.HasValue
                || !parameters.PeriodEnd.HasValue
                || parameters.PeriodEnd >= parameters.PeriodStart)
            .WithMessage("PeriodEnd cannot be earlier than PeriodStart.");
    }
}

public class CrossPortfolioSelectionQueryParametersValidator : AbstractValidator<CrossPortfolioSelectionQueryParameters>
{
    public CrossPortfolioSelectionQueryParametersValidator()
    {
        RuleFor(parameters => parameters.PortfolioIds)
            .NotEmpty()
            .WithMessage("At least one PortfolioId is required.");

        RuleFor(parameters => parameters)
            .Must(parameters => !parameters.From.HasValue || !parameters.To.HasValue || parameters.To >= parameters.From)
            .WithMessage("To cannot be earlier than From.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.PeriodStart.HasValue
                || !parameters.PeriodEnd.HasValue
                || parameters.PeriodEnd >= parameters.PeriodStart)
            .WithMessage("PeriodEnd cannot be earlier than PeriodStart.");
    }
}

public class SimulateCrossPortfolioPlanRequestValidator : AbstractValidator<SimulateCrossPortfolioPlanRequest>
{
    public SimulateCrossPortfolioPlanRequestValidator()
    {
        RuleFor(request => request.PortfolioIds)
            .Must(ids => ids is not null && ids.Distinct().Count() >= 2)
            .WithMessage("At least two distinct PortfolioIds are required to simulate a Cross-Portfolio Plan.");

        RuleFor(request => request)
            .Must(request => !request.PeriodStart.HasValue
                || !request.PeriodEnd.HasValue
                || request.PeriodEnd >= request.PeriodStart)
            .WithMessage("PeriodEnd cannot be earlier than PeriodStart.");

        RuleFor(request => request.ScenarioName)
            .MaximumLength(200)
            .When(request => request.ScenarioName is not null);
    }
}

public class CompareCrossPortfolioScenariosRequestValidator : AbstractValidator<CompareCrossPortfolioScenariosRequest>
{
    public CompareCrossPortfolioScenariosRequestValidator()
    {
        RuleFor(request => request.LeftScenarioId)
            .NotEmpty()
            .WithMessage("LeftScenarioId is required.");

        RuleFor(request => request.RightScenarioId)
            .NotEmpty()
            .WithMessage("RightScenarioId is required.");

        RuleFor(request => request)
            .Must(request =>
                request.LeftScenarioId == Guid.Empty
                || request.RightScenarioId == Guid.Empty
                || request.LeftScenarioId != request.RightScenarioId)
            .WithMessage("LeftScenarioId and RightScenarioId must be different.");
    }
}
