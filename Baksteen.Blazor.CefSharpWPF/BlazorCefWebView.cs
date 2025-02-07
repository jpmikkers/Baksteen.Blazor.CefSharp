using Baksteen.Blazor.CefSharpWPF.Glue;
using CefSharp;
//using CefSharp.Wpf;
using CefSharp.Wpf.HwndHost;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Baksteen.Blazor.CefSharpWPF;

/// <summary>
/// A control for hosting Razor components locally in Windows desktop applications.
/// </summary>
public class CefSharpBlazorWebView : UserControl, ICefSharpBlazorWebView, IAsyncDisposable
{
    public static readonly DependencyProperty ServicesProperty = DependencyProperty.Register(nameof(Services), typeof(IServiceProvider), typeof(CefSharpBlazorWebView), new PropertyMetadata(default(IServiceProvider)));
    public static readonly DependencyProperty RootComponentsProperty = DependencyProperty.Register(nameof(RootComponents), typeof(BSRootComponentsCollection), typeof(CefSharpBlazorWebView), new PropertyMetadata(default(BSRootComponentsCollection), (sender, eventArgs) =>
    {
        if (sender is not CefSharpBlazorWebView blazorWebView)
            return;

        if (eventArgs.OldValue is BSRootComponentsCollection oldRootComponents)
            oldRootComponents.CollectionChanged -= blazorWebView.HandleRootComponentsCollectionChanged;

        var newValue = eventArgs.NewValue ?? new BSRootComponentsCollection();
        if (eventArgs.NewValue is BSRootComponentsCollection newRootComponents)
            newRootComponents.CollectionChanged += blazorWebView.HandleRootComponentsCollectionChanged;
    }));

    private readonly ChromiumWebBrowser _webview;

    //private BSWebViewManager? _webviewManager;
    private CefSharpWebViewManager? _webviewManager;

    private string? _hostPage;
    private bool _isDisposed;

    /// <summary>
    /// Creates a new instance of <see cref="BlazorWebView"/>.
    /// </summary>
    public CefSharpBlazorWebView()
    {
        ComponentsDispatcher = new BSWPFDispatcher(this);
        RootComponents = new BSRootComponentsCollection();
        Unloaded += CefSharpBlazorWebView_Unloaded;

        var settings = new CefSettings()
        {
            BrowserSubprocessPath = Environment.ProcessPath,
            CefCommandLineArgs =
            {
                 //Example of setting a command line argument
                //Enables WebRTC
                // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
                // - CEF Doesn't currently support displaying a UI for media access permissions
                //
                //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
                "enable-media-stream",
                //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
                "use-fake-ui-for-media-stream",
            }
        };

        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            // note to self: if the following crashes with 'missing locales en-us.pak' after upgrading cefsharp, check the following discussion:
            // https://github.com/cefsharp/CefSharp/discussions/4207
            Cef.Initialize(settings, performDependencyCheck: true);
        }

        _webview = new ChromiumWebBrowser();

#if PARKEDSTUFF
        //_webview.BrowserSettings = BrowserSettings.Create(true);
        //_webview.BrowserSettings.Javascript = CefState.Enabled;
        //_webview.BrowserSettings.JavascriptCloseWindows = CefState.Enabled;
        //_webview.BrowserSettings.JavascriptDomPaste = CefState.Enabled;

        if (_webview.IsBrowserInitialized)
        {
            //_webview.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;             // TODO JMIK: not sure what to pick here
            _webview.JavascriptObjectRepository.Settings.AlwaysInterceptAsynchronously = true;     // TODO JMIK: not sure what to pick here
            _webview.JavascriptObjectRepository.Settings.JavascriptBindingApiEnabled = true;        // TODO JMIK: not sure what to pick here
        }
#endif

#if DEBUG
       // _webview.IsBrowserInitializedChanged += _webview_IsBrowserInitializedChanged;
        _webview.ConsoleMessage += _webview_ConsoleMessage;
        _webview.LoadError += _webview_LoadError;
#endif

        AddChild(_webview);
    }

    private void CefSharpBlazorWebView_Loaded(object sender, RoutedEventArgs e) => StartWebViewCoreIfPossible();

    private void CheckDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        //_webview.IsBrowserInitializedChanged -= _webview_IsBrowserInitializedChanged;
        _webview.ConsoleMessage -= _webview_ConsoleMessage;
        _webview.LoadError -= _webview_LoadError;

        // Dispose this component's contents that user-written disposal logic and Razor component disposal logic will
        // complete first. Then dispose the WebView2 control. This order is critical because once the WebView2 is
        // disposed it will prevent and Razor component code from working because it requires the WebView to exist.
        if (_webviewManager != null)
        {
            //In Microsoft's code this dispose has an ConfigureAwait(false). However, the webview can only be disposed on the Dispatcher thread
            //this means that it is better to just do a regular dispose and don't cause the rest of the code to run in some other random thread.
            await _webviewManager.DisposeAsync();
            _webviewManager = null;
        }

        _webview.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        // Perform async cleanup.
        await DisposeAsyncCore();

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    public void Dispose() => _ = DisposeAsync();

    private void CefSharpBlazorWebView_Unloaded(object sender, System.Windows.RoutedEventArgs e) => Dispose();

    private void _webview_IsBrowserInitializedChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        Debug.WriteLine("_webview_IsBrowserInitializedChanged");
    }

    private void _webview_ConsoleMessage(object? sender, ConsoleMessageEventArgs e)
    {
        Debug.WriteLine($"webview_consolemessage({e.Level} {e.Message})");
    }

    private void _webview_LoadError(object? sender, LoadErrorEventArgs e)
    {
        Debug.WriteLine($"webview_LoadError(ErrorCode='{e.ErrorCode}' ErrorText='{e.ErrorText}' FailedUrl='{e.FailedUrl}')");
    }

    /// <summary>
    /// Returns the inner webview used by this control.
    /// </summary>
    /// <remarks>
    /// Directly using some functionality of the inner web view can cause unexpected results because its behavior
    /// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ChromiumWebBrowser WebView => _webview;

    private Dispatcher ComponentsDispatcher { get; }

    /// <summary>
    /// Path to the host page within the application's static files. For example, <code>wwwroot\index.html</code>.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    [Category("Behavior")]
    [Description(@"Path to the host page within the application's static files. Example: wwwroot\index.html.")]
    public string? HostPage
    {
        get => _hostPage;
        set
        {
            _hostPage = value;
            StartWebViewCoreIfPossible();
        }
    }

    // Learn more about these methods here: https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/defining-default-values-with-the-shouldserialize-and-reset-methods?view=netframeworkdesktop-4.8
    private void ResetHostPage() => HostPage = null;
    private bool ShouldSerializeHostPage() => !string.IsNullOrEmpty(HostPage);

    /// <summary>
    /// A collection of <see cref="RootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
    /// to be used directly in the specified <see cref="HostPage"/>.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BSRootComponentsCollection RootComponents { get => (BSRootComponentsCollection)GetValue(RootComponentsProperty); set => SetValue(RootComponentsProperty, value); }

    /// <summary>
    /// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DisallowNull]
    public IServiceProvider Services
    {
        get => (IServiceProvider)GetValue(ServicesProperty);
        set => SetValue(ServicesProperty, value);
    }

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing how links are opened. By default, opens internal links in the webview and external links in an external app.")]
    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading { get; set; }

    ///// <summary>
    ///// Allows customizing the web view before it is created.
    ///// </summary>
    //[Category("Action")]
    //[Description("Allows customizing the web view before it is created.")]
    //public EventHandler<BSBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing { get; set; }

    ///// <summary>
    ///// Allows customizing the web view after it is created.
    ///// </summary>
    //[Category("Action")]
    //[Description("Allows customizing the web view after it is created.")]
    //public EventHandler<BSBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized { get; set; }

    private bool RequiredStartupPropertiesSet =>
        _webview != null &&
        HostPage != null &&
        Services != null;

    object ICefSharpBlazorWebView.PlatformSpecificComponent => _webview;

    JSComponentConfigurationStore ICefSharpBlazorWebView.JSComponents => RootComponents.JSComponents;

    protected override void OnInitialized(EventArgs e)
    {
        // Called when BeginInit/EndInit are used, such as when creating the control from XAML
        base.OnInitialized(e);
        StartWebViewCoreIfPossible();
    }

    public override void OnApplyTemplate()
    {
        // Called when the control is created after its child control (the WebView2) is created from the Template property
        base.OnApplyTemplate();
        if (_webviewManager == null)
            StartWebViewCoreIfPossible();
    }

    private void StartWebViewCoreIfPossible()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        // If we don't have all the required properties, or if there's already a WebViewManager, do nothing
        if (!RequiredStartupPropertiesSet || _webviewManager != null)
        {
            return;
        }

        if (Services != null)
        {
            if (Services.GetService<CefSharpBlazorMarker>() is null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. " +
                    $"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(CefSharpBlazorServiceCollectionExtensions.AddCefSharpBlazorWebView)}' in the application startup code.");
            }
        }

        // We assume the host page is always in the root of the content directory, because it's
        // unclear there's any other use case. We can add more options later if so.
        string appRootDir;
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        if (!string.IsNullOrEmpty(entryAssemblyLocation))
        {
            appRootDir = Path.GetDirectoryName(entryAssemblyLocation)!;
        }
        else
        {
            appRootDir = Environment.CurrentDirectory;
        }

        var hostPageFullPath = Path.GetFullPath(Path.Combine(appRootDir, HostPage!)); // HostPage is nonnull because RequiredStartupPropertiesSet is checked above
        var contentRootDirFullPath = Path.GetDirectoryName(hostPageFullPath)!;
        var contentRootRelativePath = Path.GetRelativePath(appRootDir, contentRootDirFullPath);
        var hostPageRelativePath = Path.GetRelativePath(contentRootDirFullPath, hostPageFullPath);

        var fileProvider = CreateFileProvider(contentRootDirFullPath);

        _webviewManager = new CefSharpWebViewManager(
            _webview,
            Services!,
            ComponentsDispatcher,
            fileProvider,
            RootComponents.JSComponents,
            contentRootRelativePath,
            hostPageRelativePath,
            onUrlLoadingAction: args => UrlLoading?.Invoke(this,args)
        );

        BSStaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

        Dispatcher.InvokeAsync(async () =>
        {
            foreach (var rootComponent in RootComponents)
            {
                // Since the page isn't loaded yet, this will always complete synchronously
                await rootComponent.AddToWebViewManagerAsync(_webviewManager);
            }
            _webviewManager.Navigate("/");
        });
    }

    private void HandleRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        // If we haven't initialized yet, this is a no-op
        if (_webviewManager == null)
            return;

        // Dispatch because this is going to be async, and we want to catch any errors
        _ = ComponentsDispatcher.InvokeAsync(async () =>
        {
            var newItems = (eventArgs.NewItems ?? Array.Empty<object>()).Cast<BSRootComponent>();
            var oldItems = (eventArgs.OldItems ?? Array.Empty<object>()).Cast<BSRootComponent>();

            foreach (var item in newItems.Except(oldItems))
                await item.AddToWebViewManagerAsync(_webviewManager);

            foreach (var item in oldItems.Except(newItems))
                await item.RemoveFromWebViewManagerAsync(_webviewManager);
        });
    }

    /// <summary>
    /// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. The default implementation
    /// serves files from disk. Override this method to return a custom <see cref="IFileProvider"/> to serve assets such
    /// as <c>wwwroot/index.html</c>. Call the base method and combine its return value with a <see cref="CompositeFileProvider"/>
    /// to use both custom assets and default assets.
    /// </summary>
    /// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
    /// <returns>Returns a <see cref="IFileProvider"/> for static assets.</returns>
    private IFileProvider CreateFileProvider(string contentRootDir)
    {
        if (Directory.Exists(contentRootDir))
        {
            // Typical case after publishing, or if you're copying content to the bin dir in development for some nonstandard reason
            return new PhysicalFileProvider(contentRootDir);
        }
        else
        {
            // Typical case in development, as the files come from Microsoft.AspNetCore.Components.WebView.StaticContentProvider
            // instead and aren't copied to the bin dir
            return new NullFileProvider();
        }
    }

    void ICefSharpBlazorWebView.AddRootComponents(IEnumerable<BSRootComponent> rootComponents)
    {
        foreach (var component in rootComponents)
            RootComponents.Add(component);
    }
}