using Baksteen.Blazor.WinForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Baksteen.WPF.Blazor;

public static class BSBlazorWebViewServiceCollectionExtensions
{
    public static IServiceCollection AddBSWPFBlazorWebView(this IServiceCollection services)
    {
        services.TryAddSingleton<BSWPFBlazorMarkerService>();
        return services.AddBSWinFormsBlazorWebView();
    }

    public static IServiceCollection AddBSWPFBlazorWebViewDeveloperTools(this IServiceCollection services)
    {
        return services.AddBSWinFormsBlazorWebViewDeveloperTools();
    }
}

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddCefSharpBlazorWebView() was called during startup
/// </summary>
internal class BSWPFBlazorMarkerService
{
}
