using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;
using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Tests;

public class CompanyDecisionProfileRequestValidatorTests
{
    private static readonly Guid CompanyId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

    [Fact]
    public void CreateCompanyDecisionProfileRequestValidator_RejectsMissingRequiredFields()
    {
        var validator = new CreateCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(new CreateCompanyDecisionProfileRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyDecisionProfileRequest.CompanyId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyDecisionProfileRequest.Code));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyDecisionProfileRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyDecisionProfileRequest.PriorityWeights));
    }

    [Fact]
    public void CreateCompanyDecisionProfileRequestValidator_AcceptsValidRequest()
    {
        var validator = new CreateCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCompanyDecisionProfileRequestValidator_RejectsUnknownDimension()
    {
        var validator = new CreateCompanyDecisionProfileRequestValidator();
        var request = CreateValidRequest();
        request.PriorityWeights = [new DecisionProfileDimensionRequest { Dimension = "NotADimension", Weight = 1m }];

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateCompanyDecisionProfileRequestValidator_RejectsWeightOutOfRange()
    {
        var validator = new CreateCompanyDecisionProfileRequestValidator();
        var request = CreateValidRequest();
        request.CapacityWeight = 1.5m;

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCompanyDecisionProfileRequest.CapacityWeight));
    }

    [Fact]
    public void CreateCompanyDecisionProfileRequestValidator_RejectsPriorityWeightsWithNoPositiveWeight()
    {
        var validator = new CreateCompanyDecisionProfileRequestValidator();
        var request = CreateValidRequest();
        request.PriorityWeights = [new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0m }];

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateCompanyDecisionProfileRequestValidator_AcceptsValidRequest()
    {
        var validator = new UpdateCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(new UpdateCompanyDecisionProfileRequest
        {
            Name = "Updated",
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateCompanyDecisionProfileRequestValidator_RejectsMissingPriorityWeights()
    {
        var validator = new UpdateCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(new UpdateCompanyDecisionProfileRequest());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CloneCompanyDecisionProfileRequestValidator_RejectsMissingFields()
    {
        var validator = new CloneCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(new CloneCompanyDecisionProfileRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CloneCompanyDecisionProfileRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CloneCompanyDecisionProfileRequest.Code));
    }

    [Fact]
    public void CloneCompanyDecisionProfileRequestValidator_AcceptsValidRequest()
    {
        var validator = new CloneCompanyDecisionProfileRequestValidator();

        var result = validator.Validate(new CloneCompanyDecisionProfileRequest
        {
            Name = "Cloned",
            Code = "ClonedCode"
        });

        Assert.True(result.IsValid);
    }

    private static CreateCompanyDecisionProfileRequest CreateValidRequest() =>
        new()
        {
            CompanyId = CompanyId,
            Code = "TestCode",
            Name = "Test Profile",
            PriorityWeights =
            [
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.EstimatedCost, Weight = 0.5m },
                new DecisionProfileDimensionRequest { Dimension = RankingDimension.OperationalRisk, Weight = 0.5m }
            ],
            CapacityWeight = 0.2m,
            WorkloadWeight = 0.2m,
            CostWeight = 0.2m,
            RiskWeight = 0.2m,
            QualityWeight = 0.2m
        };
}
