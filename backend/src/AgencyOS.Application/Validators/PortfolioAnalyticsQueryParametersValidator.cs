using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class PortfolioAnalyticsQueryParametersValidator : AbstractValidator<PortfolioAnalyticsQueryParameters>
{
    public PortfolioAnalyticsQueryParametersValidator()
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

public class PortfolioCompareQueryParametersValidator : AbstractValidator<PortfolioCompareQueryParameters>
{
    public PortfolioCompareQueryParametersValidator()
    {
        RuleFor(parameters => parameters.LeftPortfolioId)
            .NotEmpty()
            .WithMessage("LeftPortfolioId is required.");

        RuleFor(parameters => parameters.RightPortfolioId)
            .NotEmpty()
            .WithMessage("RightPortfolioId is required.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                parameters.LeftPortfolioId == Guid.Empty
                || parameters.RightPortfolioId == Guid.Empty
                || parameters.LeftPortfolioId != parameters.RightPortfolioId)
            .WithMessage("LeftPortfolioId and RightPortfolioId must be different.");

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
