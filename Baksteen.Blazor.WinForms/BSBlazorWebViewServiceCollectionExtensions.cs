using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Blazor.WinForms;

public static class BSBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddBSWinFormsBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSBlazorMarkerService>();
        services.TryAddSingleton<BSWinFormsBlazorMarkerService>();
        return services.AddWindowsFormsBlazorWebView().Services;
    }

    public static IServiceCollection AddBSWinFormsBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        services.AddBlazorWebViewDeveloperTools();
        return services.AddSingleton(new BSBlazorWebViewDeveloperTools { Enabled = true });
    }
}

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddCefSharpBlazorWebView() was called during startup
/// </summary>
internal class BSWinFormsBlazorMarkerService
{
}
