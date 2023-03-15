using DynamicData;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform;

namespace Baksteen.Avalonia.Blazor;

public class WinFormsBlazorWebViewProxy : IBlazorWebView
{
    public IPlatformHandle Handle => new PlatformHandle(_original.Handle, "HWND");
    private readonly Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView _original;

    public WinFormsBlazorWebViewProxy()
    {
        _original = new();
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

    public WebView2 WebView => _original.WebView;

    public EventHandler<UrlLoadingEventArgs>? UrlLoading
    {
        get => _original.UrlLoading;
        set => _original.UrlLoading = value;
    }

    public EventHandler<BlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing
    {
        get => _original.BlazorWebViewInitializing;
        set => _original.BlazorWebViewInitializing = value;
    }

    public EventHandler<BlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized
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

    public void AddRootComponents(IEnumerable<ARootComponent> rootComponents)
    {
        _original.RootComponents.AddRange(rootComponents.Select(x => new RootComponent(x.Selector, x.ComponentType, x.Parameters)));
    }

    public JSComponentConfigurationStore JSComponents => _original.RootComponents.JSComponents;
}
