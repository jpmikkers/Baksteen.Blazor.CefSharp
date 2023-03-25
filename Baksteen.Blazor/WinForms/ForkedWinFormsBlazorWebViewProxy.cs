using Baksteen.Avalonia.Blazor.Contract;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;

namespace Baksteen.Avalonia.Blazor.WinForms;

/// <summary>
/// Implementation of IBlazorWebView that uses a forked version of the WinForms BlazorWebView, 
/// which allows injection of the WebView2 component in its constructor.
/// </summary>
public class ForkedWinFormsBlazorWebViewProxy : IBSBlazorWebView
{
    public object PlatformSpecificComponent => _original;
    private readonly BlazorWebView _original;
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

    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading
    {
        get => _original.UrlLoading;
        set => _original.UrlLoading = value;
    }

    public EventHandler<BSBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing
    {
        get => _original.BlazorWebViewInitializing;
        set => _original.BlazorWebViewInitializing = value;
    }

    public EventHandler<BSBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized
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
        foreach(var component in rootComponents) { _original.RootComponents.Add(component); }
    }

    public JSComponentConfigurationStore JSComponents => _original.RootComponents.JSComponents;
}
