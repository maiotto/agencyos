using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AgencyOS.Api.ModelBinding;

/// <summary>
/// Binds a <c>List&lt;Guid&gt;</c> query/route parameter from either repeated keys
/// (<c>?portfolioIds=a&amp;portfolioIds=b</c>) or a single comma-separated value
/// (<c>?portfolioIds=a,b</c>) — used by Cross-Portfolio Planning's PortfolioIds selection (US-405).
/// </summary>
public sealed class GuidListModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(new List<Guid>());
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var guids = new List<Guid>();
        foreach (var rawValue in valueProviderResult.Values)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                continue;
            }

            foreach (var segment in rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(segment, out var guid))
                {
                    guids.Add(guid);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        $"The value '{segment}' is not a valid GUID for '{bindingContext.ModelName}'.");
                }
            }
        }

        bindingContext.Result = ModelBindingResult.Success(guids);
        return Task.CompletedTask;
    }
}

/// <summary>Applies <see cref="GuidListModelBinder"/> to every <c>List&lt;Guid&gt;</c> model in the app.</summary>
public sealed class GuidListModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType == typeof(List<Guid>) ? new GuidListModelBinder() : null;
    }
}
