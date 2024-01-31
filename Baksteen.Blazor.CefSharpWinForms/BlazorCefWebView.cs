using Baksteen.Blazor.Contract;
using Baksteen.Blazor.WinForms;
using CefSharp;
using CefSharp.WinForms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Baksteen.Blazor.CefSharpWinForms;

/// <summary>
/// A Windows Forms control for hosting Razor components locally in Windows desktop applications.
/// </summary>
public class CefSharpBlazorWebView : ContainerControl, IBSBlazorWebView
{
    private readonly ChromiumWebBrowser _webview;
    private readonly IBSWebView _webViewProxy;
    private BSWebViewManager? _webviewManager;
    private string? _hostPage;
    private IServiceProvider? _services;

    /// <summary>
    /// Creates a new instance of <see cref="BlazorWebView"/>.
    /// </summary>
    public CefSharpBlazorWebView()
    {
        ComponentsDispatcher = new BSWindowsFormsDispatcher(this);

        RootComponents.CollectionChanged += HandleRootComponentsCollectionChanged;

        var settings = new CefSettings()
        {
            //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
            //CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            //BrowserSubprocessPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName
            //RootCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            //IgnoreCertificateErrors = true,
            //RemoteDebuggingPort = 8088,                
            //BrowserSubprocessPath = Path.GetFullPath("WinFormsBlazorApp.exe"),
            //WindowlessRenderingEnabled = false,
        };

        Cef.Initialize(settings);

        _webview = new ChromiumWebBrowser();
        // Register your Custom LifeSpanHandler

        //_webview.BrowserCore.

        //_webview.BrowserSettings = BrowserSettings.Create(true);
        //_webview.R
        //_webview.BrowserSettings.Javascript = CefState.Enabled;
        //_webview.BrowserSettings.JavascriptCloseWindows = CefState.Enabled;
        //_webview.BrowserSettings.JavascriptDomPaste = CefState.Enabled;


        //_webview.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;             // TODO JMIK: not sure what to pick here
        _webview.JavascriptObjectRepository.Settings.AlwaysInterceptAsynchronously = true;     // TODO JMIK: not sure what to pick here
        _webview.JavascriptObjectRepository.Settings.JavascriptBindingApiEnabled = true;        // TODO JMIK: not sure what to pick here

        _webview.ConsoleMessage += _webview_ConsoleMessage;

        _webview.IsBrowserInitializedChanged += _webview_IsBrowserInitializedChanged;
        // _webview.LoadError += _webview_LoadError;

        _webview.Dock = DockStyle.Fill;
        _webViewProxy = new CefWebViewAdapter(_webview, ComponentsDispatcher);

        Controls.Add(_webview);
    }

    private void _webview_IsBrowserInitializedChanged(object? sender, EventArgs e)
    {
        Trace.WriteLine("_webview_IsBrowserInitializedChanged");
        // TODO JMIK: debugging doesnt work
        //if(_webview.IsBrowserInitialized)
        //{
        //    using(var launchBrowser = new Process())
        //    {
        //        launchBrowser.StartInfo.UseShellExecute = true;
        //        launchBrowser.StartInfo.FileName = new Uri("http://localhost:8088/").ToString();
        //        launchBrowser.Start();
        //    }
        //}
    }

    private void _webview_ConsoleMessage(object? sender, ConsoleMessageEventArgs e)
    {
        Trace.WriteLine($"webview_consolemessage({e.Level} {e.Message})");
    }

    /// <summary>
    /// Returns the inner <see cref="IBSWebView"/> used by this control.
    /// </summary>
    /// <remarks>
    /// Directly using some functionality of the inner web view can cause unexpected results because its behavior
    /// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IBSWebView WebView => _webViewProxy;

    private Dispatcher ComponentsDispatcher { get; }

    /// <inheritdoc cref="Control.OnCreateControl" />
    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        StartWebViewCoreIfPossible();
    }

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
    public BSRootComponentsCollection RootComponents { get; } = new();

    /// <summary>
    /// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [DisallowNull]
    public IServiceProvider Services
    {
        get => _services!;
        set
        {
            _services = value;
            StartWebViewCoreIfPossible();
        }
    }

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing how links are opened. By default, opens internal links in the webview and external links in an external app.")]
    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading;

    /// <summary>
    /// Allows customizing the web view before it is created.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing the web view before it is created.")]
    public EventHandler<BSBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing;

    /// <summary>
    /// Allows customizing the web view after it is created.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing the web view after it is created.")]
    public EventHandler<BSBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized;

    private bool RequiredStartupPropertiesSet =>
        Created &&
        _webview != null &&
        HostPage != null &&
        Services != null;

    object IBSBlazorWebView.PlatformSpecificComponent => _webview;

    JSComponentConfigurationStore IBSBlazorWebView.JSComponents => RootComponents.JSComponents;

    EventHandler<BSUrlLoadingEventArgs>? IBSBlazorWebView.UrlLoading { get; set; }
    EventHandler<BSBlazorWebViewInitializingEventArgs>? IBSBlazorWebView.BlazorWebViewInitializing { get; set; }
    EventHandler<BSBlazorWebViewInitializedEventArgs>? IBSBlazorWebView.BlazorWebViewInitialized { get; set; }

    private void StartWebViewCoreIfPossible()
    {
        // We never start the Blazor code in design time because it doesn't make sense to run
        // a Razor component in the designer.
        if (IsAncestorSiteInDesignMode)
        {
            return;
        }

        // If we don't have all the required properties, or if there's already a WebViewManager, do nothing
        if (!RequiredStartupPropertiesSet || _webviewManager != null)
        {
            return;
        }

        if (_services != null)
        {
            if (_services.GetService<BSCefSharpBlazorMarkerService>() is null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. " +
                    $"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BSBlazorWebViewServiceCollectionExtensions.AddCefSharpBlazorWebView)}' in the application startup code.");
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

        _webviewManager = new BSWebViewManager(
            _webViewProxy,
            Services,
            ComponentsDispatcher,
            fileProvider,
            RootComponents.JSComponents,
            contentRootRelativePath,
            hostPageRelativePath,
            (args) => UrlLoading?.Invoke(this, args),
            (args) => BlazorWebViewInitializing?.Invoke(this, args),
            (args) => BlazorWebViewInitialized?.Invoke(this, args));

        BSStaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

        foreach (var rootComponent in RootComponents)
        {
            // Since the page isn't loaded yet, this will always complete synchronously
            _ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
        }
        _webviewManager.Navigate("/");
    }

    private void HandleRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        // If we haven't initialized yet, this is a no-op
        if (_webviewManager != null)
        {
            // Dispatch because this is going to be async, and we want to catch any errors
            _ = ComponentsDispatcher.InvokeAsync(async () =>
            {
                var newItems = (eventArgs.NewItems ?? Array.Empty<object>()).Cast<BSRootComponent>();
                var oldItems = (eventArgs.OldItems ?? Array.Empty<object>()).Cast<BSRootComponent>();

                foreach (var item in newItems.Except(oldItems))
                {
                    await item.AddToWebViewManagerAsync(_webviewManager);
                }

                foreach (var item in oldItems.Except(newItems))
                {
                    await item.RemoveFromWebViewManagerAsync(_webviewManager);
                }
            });
        }
    }

    /// <summary>
    /// Creates a file provider for static assets used in the <see cref="BlazorWebView"/>. The default implementation
    /// serves files from disk. Override this method to return a custom <see cref="IFileProvider"/> to serve assets such
    /// as <c>wwwroot/index.html</c>. Call the base method and combine its return value with a <see cref="CompositeFileProvider"/>
    /// to use both custom assets and default assets.
    /// </summary>
    /// <param name="contentRootDir">The base directory to use for all requested assets, such as <c>wwwroot</c>.</param>
    /// <returns>Returns a <see cref="IFileProvider"/> for static assets.</returns>
    public virtual IFileProvider CreateFileProvider(string contentRootDir)
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

    /// <inheritdoc cref="Control.Dispose(bool)" />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose this component's contents and block on completion so that user-written disposal logic and
            // Razor component disposal logic will complete first. Then call base.Dispose(), which will dispose
            // the WebView2 control. This order is critical because once the WebView2 is disposed it will prevent
            // Razor component code from working because it requires the WebView to exist.
            _webviewManager?
                .DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }
        base.Dispose(disposing);
    }

    void IBSBlazorWebView.AddRootComponents(IEnumerable<BSRootComponent> rootComponents)
    {
        foreach (var component in rootComponents)
            RootComponents.Add(component);
    }
}
