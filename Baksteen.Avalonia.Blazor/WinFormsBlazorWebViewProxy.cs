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
using System.ComponentModel.DataAnnotations;
using BaksteenBlazorWebViewInitializingEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializingEventArgs;
using BaksteenBlazorWebViewInitializedEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs;
using BaksteenUrlLoadingEventArgs = Baksteen.AspNetCore.Components.WebView.UrlLoadingEventArgs;

namespace Baksteen.Avalonia.Blazor;

/// <summary>
/// Implementation of IBlazorWebView that uses the regular, unmodified BlazorWebView from Microsoft
/// </summary>
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

    private EventHandler<UrlLoadingEventArgs>? _urlLoadingOriginal;
    private EventHandler<BaksteenUrlLoadingEventArgs>? _urlLoading;

    public EventHandler<BaksteenUrlLoadingEventArgs>? UrlLoading
    {
        get
        {
            return _urlLoading;
        }

        set
        {
            if(value != null)
            {
                _urlLoading = value;
                _urlLoadingOriginal = new EventHandler<UrlLoadingEventArgs>(
                    (sender, args) => _urlLoading(sender, 
                    new BaksteenUrlLoadingEventArgs(args.Url, args.UrlLoadingStrategy)));
                _original.UrlLoading = _urlLoadingOriginal;
            }
            else
            {
                _original.UrlLoading = null;
                _urlLoadingOriginal = null;
                _urlLoading = null;
            }
        }
    }

    private EventHandler<BlazorWebViewInitializingEventArgs>? _blazorWebViewInitializingOriginal;
    private EventHandler<BaksteenBlazorWebViewInitializingEventArgs>? _blazorWebViewInitializing;

    public EventHandler<BaksteenBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing
    {
        get
        {
            return _blazorWebViewInitializing;
        }

        set
        {
            if(value != null)
            {
                _blazorWebViewInitializing = value;
                _blazorWebViewInitializingOriginal = new EventHandler<BlazorWebViewInitializingEventArgs>(
                    (sender, args) => _blazorWebViewInitializing(sender,
                    new BaksteenBlazorWebViewInitializingEventArgs
                    {
                        BrowserExecutableFolder = args.BrowserExecutableFolder,
                        EnvironmentOptions = args.EnvironmentOptions,
                        UserDataFolder = args.UserDataFolder
                    }));
                _original.BlazorWebViewInitializing = _blazorWebViewInitializingOriginal;
            }
            else
            {
                _original.BlazorWebViewInitializing = null;
                _blazorWebViewInitializingOriginal = null;
                _blazorWebViewInitializing = null;
            }
        }
    }

    private EventHandler<BlazorWebViewInitializedEventArgs>? _blazorWebViewInitializedOriginal;
    private EventHandler<BaksteenBlazorWebViewInitializedEventArgs>? _blazorWebViewInitialized;

    public EventHandler<BaksteenBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized
    {
        get
        {
            return _blazorWebViewInitialized;
        }
        set 
        {
            if(value != null)
            {
                _blazorWebViewInitialized = value;
                _blazorWebViewInitializedOriginal = new EventHandler<BlazorWebViewInitializedEventArgs>(
                    (sender, args) => _blazorWebViewInitialized(sender,
                    new BaksteenBlazorWebViewInitializedEventArgs { WebView = args.WebView }));
                _original.BlazorWebViewInitialized = _blazorWebViewInitializedOriginal;
            }
            else
            {
                _original.BlazorWebViewInitializing = null;
                _blazorWebViewInitializingOriginal = null;
                _blazorWebViewInitializing = null;
            }
        }
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
