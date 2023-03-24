using DynamicData;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using BaksteenBlazorWebViewInitializingEventArgs = Baksteen.Avalonia.Blazor.Contract.BSBlazorWebViewInitializingEventArgs;
using BaksteenBlazorWebViewInitializedEventArgs = Baksteen.Avalonia.Blazor.Contract.BSBlazorWebViewInitializedEventArgs;
using BaksteenUrlLoadingEventArgs = Baksteen.Avalonia.Blazor.Contract.BSUrlLoadingEventArgs;
using Baksteen.Avalonia.Blazor.Contract;

namespace Baksteen.Avalonia.Blazor.WinForms;

/// <summary>
/// Implementation of IBlazorWebView that uses a forked version of the WinForms BlazorWebView, 
/// which allows injection of the WebView2 component in its constructor.
/// </summary>
public class ForkedWinFormsBlazorWebViewProxy : IBSBlazorWebView
{
    public object PlatformSpecificComponent => _original;
    private readonly AspNetCore.Components.WebView.WindowsForms.BlazorWebView _original;
    private readonly WinFormsWebViewProxy _webViewProxy;
    public ForkedWinFormsBlazorWebViewProxy(WebView2 webView)
    {
        _original = new(webView);
        _webViewProxy = new(webView);
    }

    public string? HostPage
    {
        get => _original.HostPage;
        set => _original.HostPage = value;
    }

    public IServiceProvider Services
    {
        get => _original.Services;
        set => _original.Services = value;
    }

    public IBSWebView WebView => _webViewProxy;

    public EventHandler<BaksteenUrlLoadingEventArgs>? UrlLoading
    {
        get => _original.UrlLoading;
        set => _original.UrlLoading = value;
    }

    public EventHandler<BaksteenBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing
    {
        get => _original.BlazorWebViewInitializing;
        set => _original.BlazorWebViewInitializing = value;
    }

    public EventHandler<BaksteenBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized
    {
        get => _original.BlazorWebViewInitialized;
        set => _original.BlazorWebViewInitialized = value;
    }

    public IFileProvider CreateFileProvider(string contentRootDir)
    {
        return _original.CreateFileProvider(contentRootDir);
    }

    public void Dispose()
    {
        _original.Dispose();
    }

    // TODO: wrap this? .. use AddRootComponents and JSComponents property for now
    //public RootComponentsCollection RootComponents => _original.RootComponents;

    public void AddRootComponents(IEnumerable<BSRootComponent> rootComponents)
    {
        _original.RootComponents.AddRange(rootComponents);
    }

    public JSComponentConfigurationStore JSComponents => _original.RootComponents.JSComponents;
}
