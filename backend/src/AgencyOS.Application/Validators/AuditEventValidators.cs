using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class AuditEventQueryParametersValidator : AbstractValidator<AuditEventQueryParameters>
{
    public AuditEventQueryParametersValidator()
    {
        RuleFor(parameters => parameters.EntityType).MaximumLength(100);
        RuleFor(parameters => parameters.EventType).MaximumLength(80);
        RuleFor(parameters => parameters.Action).MaximumLength(120);
        RuleFor(parameters => parameters.UserId).MaximumLength(200);
        RuleFor(parameters => parameters.Search).MaximumLength(300);

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.OccurredFrom.HasValue
                || !parameters.OccurredTo.HasValue
                || parameters.OccurredTo >= parameters.OccurredFrom)
            .WithMessage("OccurredTo cannot be earlier than OccurredFrom.");
    }
}
