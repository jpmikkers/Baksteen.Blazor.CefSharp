// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Baksteen.Avalonia.Blazor;
using Baksteen.Avalonia.Blazor.Contract;
using Baksteen.Avalonia.Blazor.WinForms;
using DynamicData;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;

namespace Baksteen.AspNetCore.Components.WebView.WindowsForms;

/// <summary>
/// A Windows Forms control for hosting Razor components locally in Windows desktop applications.
/// </summary>
public class BlazorWebViewPure : IBSBlazorWebView
{
    private class WrapperControl : ContainerControl
    {
        public Action? AfterCreateControl { get; set; }
        public Action? BeforeDisposeControl { get; set; }

        public WrapperControl() : base()
        {
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            AfterCreateControl?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                BeforeDisposeControl?.Invoke();
            }
            base.Dispose(disposing);
        }
    }

    private WrapperControl _wrapperControl;
    private readonly IBSWebView _webViewProxy;
    private BSWebViewManager? _webviewManager;
    private string? _hostPage;
    private IServiceProvider? _services;

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading { get; set; }

    /// <summary>
    /// Allows customizing the web view before it is created.
    /// </summary>
    public EventHandler<BSBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing { get; set; }

    /// <summary>
    /// Allows customizing the web view after it is created.
    /// </summary>
    public EventHandler<BSBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized { get; set; }

    /// <summary>
    /// Returns the inner <see cref="WebView2Control"/> used by this control.
    /// </summary>
    /// <remarks>
    /// Directly using some functionality of the inner web view can cause unexpected results because its behavior
    /// is controlled by the <see cref="BlazorWebView"/> that is hosting it.
    /// </remarks>
    public IBSWebView WebView => _webViewProxy;

    private Microsoft.AspNetCore.Components.Dispatcher ComponentsDispatcher { get; }

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
    //private void ResetHostPage() => HostPage = null;
    //private bool ShouldSerializeHostPage() => !string.IsNullOrEmpty(HostPage);

    /// <summary>
    /// A collection of <see cref="BSRootComponent"/> instances that specify the Blazor <see cref="IComponent"/> types
    /// to be used directly in the specified <see cref="HostPage"/>.
    /// </summary>
    public BSRootComponentsCollection RootComponents { get; } = new();

    /// <summary>
    /// Gets or sets an <see cref="IServiceProvider"/> containing services to be used by this control and also by application code.
    /// This property must be set to a valid value for the Razor components to start.
    /// </summary>
    public IServiceProvider Services
    {
        get => _services!;
        set
        {
            _services = value;
            StartWebViewCoreIfPossible();
        }
    }

    public object PlatformSpecificComponent => _wrapperControl;

    // TODO: wrap this? .. use AddRootComponents and JSComponents property for now
    //public RootComponentsCollection RootComponents => _original.RootComponents;
    public JSComponentConfigurationStore JSComponents => this.RootComponents.JSComponents;

    /// <summary>
    /// Creates a new instance of <see cref="BlazorWebView"/>.
    /// </summary>
    public BlazorWebViewPure(WebView2Control webView2Control)
    {
        _wrapperControl = new();

        _wrapperControl.AfterCreateControl = () => 
        { 
            StartWebViewCoreIfPossible(); 
        };

        _wrapperControl.BeforeDisposeControl = () => 
        {
            _webviewManager?
                .DisposeAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        };

        ComponentsDispatcher = new BSWindowsFormsDispatcher(_wrapperControl);

        RootComponents.CollectionChanged += HandleRootComponentsCollectionChanged;

        webView2Control.Dock = DockStyle.Fill;
        _webViewProxy = new WinFormsWebViewProxy(webView2Control);
        _wrapperControl.Controls.Add(webView2Control);
    }

    private bool RequiredStartupPropertiesSet =>
        _wrapperControl.Created &&
        HostPage != null &&
        Services != null;

    private void StartWebViewCoreIfPossible()
    {
        // We never start the Blazor code in design time because it doesn't make sense to run
        // a Razor component in the designer.
        if(_wrapperControl.IsAncestorSiteInDesignMode)
        {
            return;
        }

        // If we don't have all the required properties, or if there's already a WebViewManager, do nothing
        if(!RequiredStartupPropertiesSet || _webviewManager != null)
        {
            return;
        }

        // We assume the host page is always in the root of the content directory, because it's
        // unclear there's any other use case. We can add more options later if so.
        string appRootDir;
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        if(!string.IsNullOrEmpty(entryAssemblyLocation))
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

        foreach(var rootComponent in RootComponents)
        {
            // Since the page isn't loaded yet, this will always complete synchronously
            _ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
        }
        _webviewManager.Navigate("/");
    }

    private void HandleRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        // If we haven't initialized yet, this is a no-op
        if(_webviewManager != null)
        {
            // Dispatch because this is going to be async, and we want to catch any errors
            _ = ComponentsDispatcher.InvokeAsync(async () =>
            {
                var newItems = (eventArgs.NewItems ?? Array.Empty<object>()).Cast<BSRootComponent>();
                var oldItems = (eventArgs.OldItems ?? Array.Empty<object>()).Cast<BSRootComponent>();

                foreach(var item in newItems.Except(oldItems))
                {
                    await item.AddToWebViewManagerAsync(_webviewManager);
                }

                foreach(var item in oldItems.Except(newItems))
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
    public IFileProvider CreateFileProvider(string contentRootDir)
    {
        if(Directory.Exists(contentRootDir))
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

    public void AddRootComponents(IEnumerable<BSRootComponent> rootComponents)
    {
        this.RootComponents.AddRange(rootComponents);
    }

    public void Dispose()
    {
        var wc = Interlocked.Exchange(ref _wrapperControl!, null);

        if(wc != null)
        {
            wc.Dispose();
        }
    }
}
