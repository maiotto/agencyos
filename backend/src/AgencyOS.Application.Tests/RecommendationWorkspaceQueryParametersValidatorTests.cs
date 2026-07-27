using AgencyOS.Application.DTOs;
using AgencyOS.Application.Validators;

namespace AgencyOS.Application.Tests;

public class RecommendationWorkspaceQueryParametersValidatorTests
{
    private readonly RecommendationWorkspaceQueryParametersValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenEmpty()
    {
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenToBeforeFrom()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters
        {
            From = now,
            To = now.AddDays(-1)
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenFromEqualsTo()
    {
        var now = DateTimeOffset.UtcNow;
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters { From = now, To = now });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenOnlyLeftRecommendationIdSet()
    {
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters
        {
            LeftRecommendationId = Guid.NewGuid()
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenOnlyRightRecommendationIdSet()
    {
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters
        {
            RightRecommendationId = Guid.NewGuid()
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenLeftAndRightRecommendationIdsAreEqual()
    {
        var id = Guid.NewGuid();
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters
        {
            LeftRecommendationId = id,
            RightRecommendationId = id
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Succeeds_WhenLeftAndRightRecommendationIdsAreDistinct()
    {
        var result = _validator.Validate(new RecommendationWorkspaceQueryParameters
        {
            LeftRecommendationId = Guid.NewGuid(),
            RightRecommendationId = Guid.NewGuid()
        });
        Assert.True(result.IsValid);
    }
}
