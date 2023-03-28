using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Blazor;

public static class BSBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSBlazorMarkerService>();
        return services.AddWindowsFormsBlazorWebView().Services;
    }

    public static IServiceCollection AddAvaloniaBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        services.AddBlazorWebViewDeveloperTools();
        return services.AddSingleton(new BSBlazorWebViewDeveloperTools { Enabled = true });
    }
}
