using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class HolidayRequestValidatorTests
{
    [Fact]
    public void CreateHolidayRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateHolidayRequestValidator();

        var result = validator.Validate(new CreateHolidayRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHolidayRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHolidayRequest.HolidayType));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHolidayRequest.HolidayDate));
    }

    [Fact]
    public void CreateHolidayRequestValidator_AcceptsValidNationalHoliday()
    {
        var validator = new CreateHolidayRequestValidator();

        var result = validator.Validate(new CreateHolidayRequest
        {
            Name = "Independence Day",
            HolidayType = HolidayTypes.National,
            HolidayDate = new DateOnly(2026, 9, 7),
            Recurring = true
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateHolidayRequestValidator_RejectsCompanyHolidayWithoutCompanyId()
    {
        var validator = new CreateHolidayRequestValidator();

        var result = validator.Validate(new CreateHolidayRequest
        {
            Name = "Foundation Day",
            HolidayType = HolidayTypes.Company,
            HolidayDate = new DateOnly(2026, 11, 15)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHolidayRequest.CompanyId));
    }

    [Fact]
    public void CreateHolidayRequestValidator_RejectsMunicipalHolidayWithoutCity()
    {
        var validator = new CreateHolidayRequestValidator();

        var result = validator.Validate(new CreateHolidayRequest
        {
            Name = "City Day",
            HolidayType = HolidayTypes.Municipal,
            HolidayDate = new DateOnly(2026, 1, 25),
            StateCode = "SP"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHolidayRequest.City));
    }

    [Fact]
    public void UpdateHolidayRequestValidator_AcceptsValidStateHoliday()
    {
        var validator = new UpdateHolidayRequestValidator();

        var result = validator.Validate(new UpdateHolidayRequest
        {
            Name = "State Holiday",
            HolidayType = HolidayTypes.State,
            HolidayDate = new DateOnly(2026, 7, 9),
            StateCode = "SP",
            Recurring = true
        });

        Assert.True(result.IsValid);
    }
}
