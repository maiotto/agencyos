using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class UpdateHolidayRequestValidator : AbstractValidator<UpdateHolidayRequest>
{
    public UpdateHolidayRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Holiday Name is mandatory.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.HolidayType)
            .NotEmpty()
            .Must(type => HolidayTypes.All.Contains(type))
            .WithMessage("Holiday Type must be a valid holiday type.");

        RuleFor(x => x.HolidayDate)
            .NotEmpty()
            .WithMessage("Holiday Date is mandatory.");

        When(x => HolidayTypes.IsCompany(x.HolidayType), () =>
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .WithMessage("Company Holiday requires CompanyId.");
        });

        When(x => HolidayRules.RequiresStateCode(x.HolidayType), () =>
        {
            RuleFor(x => x.StateCode)
                .NotEmpty()
                .WithMessage("StateCode is required for State and Municipal holidays.")
                .MaximumLength(10);
        });

        When(x => HolidayRules.RequiresCity(x.HolidayType), () =>
        {
            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Municipal Holiday requires StateCode and City.")
                .MaximumLength(120);
        });

        RuleFor(x => x.StateCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.StateCode) && !HolidayRules.RequiresStateCode(x.HolidayType));

        RuleFor(x => x.City)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.City) && !HolidayRules.RequiresCity(x.HolidayType));
    }
}
