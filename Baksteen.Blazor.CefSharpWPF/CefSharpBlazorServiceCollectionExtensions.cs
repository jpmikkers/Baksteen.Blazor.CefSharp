using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Blazor.CefSharpWPF;

public static class CefSharpBlazorServiceCollectionExtensions
{
    public static IServiceCollection AddCefSharpBlazorWebView(this IServiceCollection services)
    {
        //services.TryAddSingleton<BSBlazorMarkerService>();
        services.TryAddSingleton<CefSharpBlazorMarker>();
        services.AddBlazorWebView();
        services.TryAddSingleton(new WebViewDeveloperToolsMarker { Enabled = false });
        return services;
    }

    public static IServiceCollection AddCefSharpBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        return services.AddSingleton(new WebViewDeveloperToolsMarker { Enabled = true });
    }
}

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddCefSharpBlazorWebView() was called during startup
/// </summary>
internal class CefSharpBlazorMarker
{
}

internal class WebViewDeveloperToolsMarker
{
    public bool Enabled { get; set; } = false;
}
