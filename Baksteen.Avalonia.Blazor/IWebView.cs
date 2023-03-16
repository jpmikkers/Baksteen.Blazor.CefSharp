using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Baksteen.Avalonia.Blazor;

public interface IWebView
{
    Uri Source { get; set; }
    double ZoomFactor { get; set; }
/*
    bool AllowDrop { get; }
    bool AllowExternalDrop { get; set; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    ContextMenuStrip ContextMenuStrip { get; }
    CoreWebView2 CoreWebView2 { get; }
    CoreWebView2CreationProperties CreationProperties { get; set; }
    Color DefaultBackgroundColor { get; set; }
    Font Font { get; }
    string Text { get; }

    event EventHandler<CoreWebView2ContentLoadingEventArgs> ContentLoading;
    event EventHandler<CoreWebView2InitializationCompletedEventArgs> CoreWebView2InitializationCompleted;
    event EventHandler<CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
    event EventHandler<CoreWebView2NavigationStartingEventArgs> NavigationStarting;
    event EventHandler<CoreWebView2SourceChangedEventArgs> SourceChanged;
    event EventHandler<CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived;
    event EventHandler<EventArgs> ZoomFactorChanged;

    Task EnsureCoreWebView2Async(CoreWebView2Environment environment = null, CoreWebView2ControllerOptions controllerOptions = null);
    Task EnsureCoreWebView2Async(CoreWebView2Environment environment);
    Task<string> ExecuteScriptAsync(string script);
    void GoBack();
    void GoForward();
    void NavigateToString(string htmlContent);
    void Reload();
    void Stop();
*/
}
