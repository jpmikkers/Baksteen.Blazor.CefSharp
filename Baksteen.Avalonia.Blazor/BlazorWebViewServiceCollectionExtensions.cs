using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Avalonia.Blazor;

public static class AvaloniaBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddAvaloniaBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BaksteenAvaloniaBlazorMarkerService>();
        return services.AddWindowsFormsBlazorWebView().Services;
    }

    public static IServiceCollection AddAvaloniaBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        return services.AddBlazorWebViewDeveloperTools();
    }
}
