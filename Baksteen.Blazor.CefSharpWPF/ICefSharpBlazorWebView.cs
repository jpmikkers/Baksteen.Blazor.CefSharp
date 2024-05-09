using Microsoft.AspNetCore.Components.Web;
using CefSharp.Wpf;
using Baksteen.Blazor.CefSharpWPF.Glue;

namespace Baksteen.Blazor.CefSharpWPF;

public interface ICefSharpBlazorWebView : IDisposable
{
    /// <summary>
    /// returns a platform specific object that can be used to retrieve a platform specific handle, or the handle itself
    /// For now this is the convention:
    /// Windows & Winforms -> this should return a System.Windows.Forms.Control
    /// </summary>
    public object PlatformSpecificComponent { get; }

    string? HostPage { get; set; }

    // AddRootComponents and JSComponents replace this one, for now:
    // RootComponentsCollection RootComponents { get; }

    void AddRootComponents(IEnumerable<BSRootComponent> rootComponents);
    JSComponentConfigurationStore JSComponents { get; }

    IServiceProvider Services { get; set; }
    ChromiumWebBrowser WebView { get; }

    // not public for CefSharp:
    // IFileProvider CreateFileProvider(string contentRootDir);

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading { get; set; }
}
