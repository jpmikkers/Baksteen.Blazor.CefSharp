using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Avalonia.Blazor;

public static class AvaloniaBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSAvaloniaBlazorMarkerService>();
        return services.AddWindowsFormsBlazorWebView().Services;
    }

    public static IServiceCollection AddAvaloniaBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        services.AddBlazorWebViewDeveloperTools();
        return services.AddSingleton<BSBlazorWebViewDeveloperTools>(new BSBlazorWebViewDeveloperTools { Enabled = true });
    }
}
