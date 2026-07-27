using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class RecommendationWorkflowQueryParametersValidator
    : AbstractValidator<RecommendationWorkflowQueryParameters>
{
    public RecommendationWorkflowQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || RecommendationWorkflowStatus.IsKnown(status))
            .WithMessage("Status filter is not a known recommendation workflow status.");

        RuleFor(parameters => parameters.Search).MaximumLength(300);
        RuleFor(parameters => parameters.CreatedBy).MaximumLength(200);
    }
}

public class CreateRecommendationWorkflowRequestValidator
    : AbstractValidator<CreateRecommendationWorkflowRequest>
{
    public CreateRecommendationWorkflowRequestValidator()
    {
        RuleFor(request => request.RecommendationId).NotEmpty();
        RuleFor(request => request.CreatedBy).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Title).MaximumLength(300).When(request => request.Title is not null);
        RuleFor(request => request.Summary).MaximumLength(2000).When(request => request.Summary is not null);
    }
}

public class RecommendationWorkflowActionRequestValidator
    : AbstractValidator<RecommendationWorkflowActionRequest>
{
    public RecommendationWorkflowActionRequestValidator()
    {
        RuleFor(request => request.Actor).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Comment).MaximumLength(2000);
    }
}

public class ApproveRecommendationWorkflowRequestValidator
    : AbstractValidator<ApproveRecommendationWorkflowRequest>
{
    public ApproveRecommendationWorkflowRequestValidator()
    {
        RuleFor(request => request.Approver).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Comment).MaximumLength(2000);
    }
}
