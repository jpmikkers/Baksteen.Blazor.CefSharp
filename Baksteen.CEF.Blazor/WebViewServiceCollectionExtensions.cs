using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Blazor.CefSharpWinForms;

public static class BSBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddCefSharpBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSBlazorMarkerService>();
        services.TryAddSingleton<BSCefSharpBlazorMarkerService>();
        services.AddBlazorWebView();
        services.TryAddSingleton(new BSBlazorWebViewDeveloperTools { Enabled = false });
        return services;
    }

    public static IServiceCollection AddCefSharpBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        return services.AddSingleton(new BSBlazorWebViewDeveloperTools { Enabled = true });
    }
}

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddCefSharpBlazorWebView() was called during startup
/// </summary>
internal class BSCefSharpBlazorMarkerService
{
}
