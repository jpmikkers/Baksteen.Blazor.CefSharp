namespace Baksteen.Avalonia.Blazor;

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddAvaloniaBlazorWebView() was called during startup
/// </summary>
internal class BSAvaloniaBlazorMarkerService
{
}
