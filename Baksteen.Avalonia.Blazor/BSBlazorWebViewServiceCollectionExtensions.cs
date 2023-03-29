using Baksteen.Blazor.WinForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.Avalonia.Blazor;

public static class BSBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddBSAvaloniaBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSAvaloniaBlazorMarkerService>();
        return services.AddBSWinFormsBlazorWebView();
    }

    public static IServiceCollection AddBSAvaloniaBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        return services.AddBSWinFormsBlazorWebViewDeveloperTools();
    }
}

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddCefSharpBlazorWebView() was called during startup
/// </summary>
internal class BSAvaloniaBlazorMarkerService
{
}
