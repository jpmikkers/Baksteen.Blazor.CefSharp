using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Baksteen.Blazor.Contract;

public interface IBSWebView : IDisposable
{
    bool CanGoBack { get; }
    bool CanGoForward { get; }

    Uri Source { get; set; }
    double ZoomFactor { get; set; }
    IBSCoreWebView CoreWebView2 { get; }

    void Reload();
    void GoBack();
    void GoForward();
    void Stop();

    //
    // Summary:
    //     Explicitly trigger initialization of the control's Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2.
    //
    // Parameters:
    //   environment:
    //     A pre-created Microsoft.Web.WebView2.Core.CoreWebView2Environment that should
    //     be used to create the Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2.
    //     Creating your own environment gives you control over several options that affect
    //     how the Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2 is initialized.
    //     If you pass null then a default environment will be created and used automatically.
    //
    // Returns:
    //     A Task that represents the background initialization process. When the task completes
    //     then the Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2 property will
    //     be available for use (i.e. non-null). Note that the control's Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2InitializationCompleted
    //     event will be invoked before the task completes or on exceptions.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     Thrown if this method is called with a different environment than when it was
    //     initialized. See Remarks for more info.
    //
    //   T:System.InvalidOperationException:
    //     Thrown if this instance of Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2
    //     is already disposed, or if the calling thread isn't the thread which created
    //     this object (usually the UI thread). See System.Windows.Forms.Control.InvokeRequired
    //     for more info. May also be thrown if the browser process has crashed unexpectedly
    //     and left the control in an invalid state. We are considering throwing a different
    //     type of exception for this case in the future.
    //
    // Remarks:
    //     Unless previous initialization has already failed, calling this method additional
    //     times with the same parameter will have no effect (any specified environment
    //     is ignored) and return the same Task as the first call. Unless previous initialization
    //     has already failed, calling this method after initialization has been implicitly
    //     triggered by setting the Microsoft.Web.WebView2.WinForms.WebView2.Source property
    //     will have no effect if no environment is given and simply return a Task representing
    //     that initialization already in progress. Unless previous initialization has already
    //     failed, calling this method with a different environment after initialization
    //     has begun will result in an System.ArgumentException. For example, this can happen
    //     if you begin initialization by setting the Microsoft.Web.WebView2.WinForms.WebView2.Source
    //     property and then call this method with a new environment, if you begin initialization
    //     with Microsoft.Web.WebView2.WinForms.WebView2.CreationProperties and then call
    //     this method with a new environment, or if you begin initialization with one environment
    //     and then call this method with no environment specified. When this method is
    //     called after previous initialization has failed, it will trigger initialization
    //     of the control's Microsoft.Web.WebView2.WinForms.WebView2.CoreWebView2 again.
    //     Note that even though this method is asynchronous and returns a Task, it still
    //     must be called on the UI thread like most public functionality of most UI controls.
    public Task EnsureCoreWebView2Async(CoreWebView2Environment environment);

    /*
        bool AllowDrop { get; }
        bool AllowExternalDrop { get; set; }
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
        void NavigateToString(string htmlContent);
    */
}
