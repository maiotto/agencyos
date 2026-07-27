using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class DecisionQueryParametersValidator : AbstractValidator<DecisionQueryParameters>
{
    public DecisionQueryParametersValidator()
    {
        RuleFor(parameters => parameters.DecisionStatus)
            .Must(status => string.IsNullOrWhiteSpace(status) || DecisionStatus.IsKnown(status))
            .WithMessage("DecisionStatus filter is not a known decision status.");

        RuleFor(parameters => parameters.ImplementationStatus)
            .Must(status => string.IsNullOrWhiteSpace(status) || DecisionImplementationStatus.IsKnown(status))
            .WithMessage("ImplementationStatus filter is not a known implementation status.");

        RuleFor(parameters => parameters.Search).MaximumLength(300);

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.DecisionFrom.HasValue
                || !parameters.DecisionTo.HasValue
                || parameters.DecisionTo >= parameters.DecisionFrom)
            .WithMessage("DecisionTo cannot be earlier than DecisionFrom.");
    }
}

public class CreateDecisionRequestValidator : AbstractValidator<CreateDecisionRequest>
{
    public CreateDecisionRequestValidator()
    {
        RuleFor(request => request.RecommendationId).NotEmpty();
        RuleFor(request => request.CreatedBy).NotEmpty().MaximumLength(200);
    }
}

public class DecisionActionRequestValidator : AbstractValidator<DecisionActionRequest>
{
    public DecisionActionRequestValidator()
    {
        RuleFor(request => request.Actor).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Comment).MaximumLength(2000);
    }
}

public class RecordDecisionOutcomeRequestValidator : AbstractValidator<RecordDecisionOutcomeRequest>
{
    public RecordDecisionOutcomeRequestValidator()
    {
        RuleFor(request => request.Outcome).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.BusinessValue).MaximumLength(2000);
        RuleFor(request => request.Actor).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Comment).MaximumLength(2000);
    }
}
