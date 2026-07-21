using AgencyOS.Application.DTOs;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateMissionRequestValidator : AbstractValidator<UpdateMissionRequest>
{
    public UpdateMissionRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ClientContractId)
            .NotEmpty();

        RuleFor(x => x.MissionTypeId)
            .NotEmpty();

        RuleFor(x => x.MissionStatusId)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .MaximumLength(20);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description is not null);

        RuleFor(x => x.EndDate)
            .Must((request, endDate) => !request.StartDate.HasValue || !endDate.HasValue || endDate >= request.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate when both dates are provided.");
    }
}
