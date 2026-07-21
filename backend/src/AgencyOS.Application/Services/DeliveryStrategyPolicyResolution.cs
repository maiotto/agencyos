using AgencyOS.Domain.Entities;

namespace AgencyOS.Application.Services;

public sealed class DeliveryStrategyPolicyContext
{
    public IReadOnlySet<string> AllowedResourceTypes { get; init; } =
        ExecutionResourceType.All;

    public IReadOnlySet<string> DisallowedResourceTypes { get; init; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public bool RequireHumanReviewForAiAutomation { get; init; } = true;

    public IReadOnlyList<string> AppliedCompanyPolicyRules { get; init; } = [];

    public IReadOnlyList<string> AppliedClientPolicyRules { get; init; } = [];
}

public static class DeliveryStrategyPolicyResolution
{
    private static readonly IReadOnlySet<string> HumanResourceTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ExecutionResourceType.InternalHuman,
            ExecutionResourceType.ExternalHuman
        };

    private static readonly IReadOnlySet<string> AiAutomationResourceTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ExecutionResourceType.AiAgent,
            ExecutionResourceType.AiService,
            ExecutionResourceType.Automation
        };

    public static DeliveryStrategyPolicyContext ResolveCompanyPolicy()
    {
        return new DeliveryStrategyPolicyContext
        {
            AllowedResourceTypes = ExecutionResourceType.All,
            RequireHumanReviewForAiAutomation = true,
            AppliedCompanyPolicyRules =
            [
                "All active execution resource types are eligible by default.",
                "AI and Automation resource types require human review coverage."
            ]
        };
    }

    public static DeliveryStrategyPolicyContext ResolveClientPolicy(ClientContract contract)
    {
        var disallowedResourceTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var appliedRules = new List<string>
        {
            "Client delivery constraints are derived from the contract billing model."
        };

        if (string.Equals(contract.BillingModel, ContractType.FixedPrice, StringComparison.OrdinalIgnoreCase))
        {
            disallowedResourceTypes.Add(ExecutionResourceType.ExternalHuman);
            appliedRules.Add("Fixed Price contracts exclude External Human resources.");
        }

        if (string.Equals(contract.BillingModel, ContractType.ManagedService, StringComparison.OrdinalIgnoreCase))
        {
            appliedRules.Add("Managed Service contracts require human review for AI and Automation.");
        }

        return new DeliveryStrategyPolicyContext
        {
            DisallowedResourceTypes = disallowedResourceTypes,
            AppliedClientPolicyRules = appliedRules
        };
    }

    public static DeliveryStrategyPolicyContext MergePolicies(
        DeliveryStrategyPolicyContext companyPolicy,
        DeliveryStrategyPolicyContext clientPolicy)
    {
        var allowedResourceTypes = companyPolicy.AllowedResourceTypes
            .Where(type => !clientPolicy.DisallowedResourceTypes.Contains(type))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new DeliveryStrategyPolicyContext
        {
            AllowedResourceTypes = allowedResourceTypes,
            DisallowedResourceTypes = clientPolicy.DisallowedResourceTypes,
            RequireHumanReviewForAiAutomation = companyPolicy.RequireHumanReviewForAiAutomation,
            AppliedCompanyPolicyRules = companyPolicy.AppliedCompanyPolicyRules,
            AppliedClientPolicyRules = clientPolicy.AppliedClientPolicyRules
        };
    }

    public static bool IsValidResourceMix(
        IReadOnlySet<string> resourceMix,
        DeliveryStrategyPolicyContext policies)
    {
        if (resourceMix.Count == 0)
        {
            return false;
        }

        if (resourceMix.Any(type => !policies.AllowedResourceTypes.Contains(type)))
        {
            return false;
        }

        if (resourceMix.Any(type => policies.DisallowedResourceTypes.Contains(type)))
        {
            return false;
        }

        if (!policies.RequireHumanReviewForAiAutomation)
        {
            return true;
        }

        var includesAiAutomation = resourceMix.Any(type => AiAutomationResourceTypes.Contains(type));

        if (!includesAiAutomation)
        {
            return true;
        }

        return resourceMix.Any(type => HumanResourceTypes.Contains(type));
    }

    public static IReadOnlyList<string> GetAvailableResourceTypes(
        IEnumerable<ExecutionResource> activeResources,
        DeliveryStrategyPolicyContext policies)
    {
        return activeResources
            .Select(resource => resource.ResourceType)
            .Where(type => policies.AllowedResourceTypes.Contains(type))
            .Where(type => !policies.DisallowedResourceTypes.Contains(type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(type => type, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
