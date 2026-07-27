using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class WorkloadHistoryQueryParametersValidator : AbstractValidator<WorkloadHistoryQueryParameters>
{
    public WorkloadHistoryQueryParametersValidator()
    {
        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.PeriodStart.HasValue
                || !parameters.PeriodEnd.HasValue
                || parameters.PeriodEnd >= parameters.PeriodStart)
            .WithMessage("PeriodEnd cannot be earlier than PeriodStart.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.CalculatedFrom.HasValue
                || !parameters.CalculatedTo.HasValue
                || parameters.CalculatedTo >= parameters.CalculatedFrom)
            .WithMessage("CalculatedTo cannot be earlier than CalculatedFrom.");

        RuleFor(parameters => parameters.CalculationVersion)
            .MaximumLength(50)
            .When(parameters => !string.IsNullOrWhiteSpace(parameters.CalculationVersion));
    }
}

public class WorkloadHistoryCompareQueryParametersValidator
    : AbstractValidator<WorkloadHistoryCompareQueryParameters>
{
    public WorkloadHistoryCompareQueryParametersValidator()
    {
        RuleFor(parameters => parameters.LeftHistoryId)
            .NotEmpty()
            .WithMessage("LeftHistoryId is required.");

        RuleFor(parameters => parameters.RightHistoryId)
            .NotEmpty()
            .WithMessage("RightHistoryId is required.");

        RuleFor(parameters => parameters)
            .Must(parameters => parameters.LeftHistoryId != parameters.RightHistoryId)
            .WithMessage("LeftHistoryId and RightHistoryId must be different.");
    }
}
