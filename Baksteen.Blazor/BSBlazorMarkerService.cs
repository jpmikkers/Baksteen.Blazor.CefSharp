namespace Baksteen.Blazor;

/// <summary>
/// Dummy class that should be registered in DI, allows us to detect that IServiceCollection.AddAvaloniaBlazorWebView() was called during startup
/// </summary>
public class BSBlazorMarkerService      // TODO JMIK: make this internal again
{
}
