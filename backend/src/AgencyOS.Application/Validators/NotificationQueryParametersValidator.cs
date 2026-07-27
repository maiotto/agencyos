using AgencyOS.Application.DTOs;
using AgencyOS.Domain.Entities;
using FluentValidation;

namespace AgencyOS.Application.Validators;

public class NotificationQueryParametersValidator : AbstractValidator<NotificationQueryParameters>
{
    public NotificationQueryParametersValidator()
    {
        RuleFor(parameters => parameters.Category)
            .Must(value => string.IsNullOrWhiteSpace(value) || NotificationCategory.IsKnown(value))
            .WithMessage($"Category must be one of: {string.Join(", ", NotificationCategory.All)}.");

        RuleFor(parameters => parameters.Priority)
            .Must(value => string.IsNullOrWhiteSpace(value) || NotificationPriority.IsKnown(value))
            .WithMessage($"Priority must be one of: {string.Join(", ", NotificationPriority.All)}.");

        RuleFor(parameters => parameters.Status)
            .Must(value => string.IsNullOrWhiteSpace(value) || NotificationStatus.IsKnown(value))
            .WithMessage($"Status must be one of: {string.Join(", ", NotificationStatus.All)}.");

        RuleFor(parameters => parameters.SourceEntity)
            .Must(value => string.IsNullOrWhiteSpace(value) || NotificationSourceEntities.IsKnown(value))
            .WithMessage($"SourceEntity must be one of: {string.Join(", ", NotificationSourceEntities.All)}.");

        RuleFor(parameters => parameters)
            .Must(parameters =>
                !parameters.CreatedFrom.HasValue
                || !parameters.CreatedTo.HasValue
                || parameters.CreatedTo >= parameters.CreatedFrom)
            .WithMessage("CreatedTo cannot be earlier than CreatedFrom.");

        RuleFor(parameters => parameters.OrderBy)
            .Must(value =>
                string.IsNullOrWhiteSpace(value)
                || new[] { "createdAt", "title", "priority", "status", "category" }
                    .Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("OrderBy must be one of: createdAt, title, priority, status, category.");

        RuleFor(parameters => parameters.OrderDirection)
            .Must(value =>
                string.IsNullOrWhiteSpace(value)
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("OrderDirection must be asc or desc.");
    }
}
