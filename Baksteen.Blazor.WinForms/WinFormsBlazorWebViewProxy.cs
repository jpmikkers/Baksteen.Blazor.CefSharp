using Baksteen.Blazor.Contract;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baksteen.Blazor.WinForms;

/// <summary>
/// Implementation of IBlazorWebView that uses the regular, unmodified BlazorWebView from Microsoft
/// </summary>
public class WinFormsBlazorWebViewProxy : IBSBlazorWebView
{
    public object PlatformSpecificComponent => _original;
    private readonly Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView _original;
    private readonly WinFormsWebViewProxy _webViewProxy;
    public WinFormsBlazorWebViewProxy()
    {
        _original = new();
        _webViewProxy = new(_original.WebView);
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

    private EventHandler<UrlLoadingEventArgs>? _urlLoadingOriginal;
    private EventHandler<BSUrlLoadingEventArgs>? _urlLoading;


    private static BSUrlLoadingStrategy FromNativeUrlLoadingStrategy(UrlLoadingStrategy urlLoadingStrategy)
    {
        switch(urlLoadingStrategy)
        {
            case UrlLoadingStrategy.OpenExternally:
                return BSUrlLoadingStrategy.OpenExternally;
            case UrlLoadingStrategy.OpenInWebView:
                return BSUrlLoadingStrategy.OpenInWebView;
            default:
            case UrlLoadingStrategy.CancelLoad:
                return BSUrlLoadingStrategy.CancelLoad;
        }
    }

    private static UrlLoadingStrategy ToNativeUrlLoadingStrategy(BSUrlLoadingStrategy urlLoadingStrategy)
    {
        switch(urlLoadingStrategy)
        {
            case BSUrlLoadingStrategy.OpenExternally:
                return UrlLoadingStrategy.OpenExternally;
            case BSUrlLoadingStrategy.OpenInWebView:
                return UrlLoadingStrategy.OpenInWebView;
            default:
            case BSUrlLoadingStrategy.CancelLoad:
                return UrlLoadingStrategy.CancelLoad;
        }
    }

    public EventHandler<BSUrlLoadingEventArgs>? UrlLoading
    {
        get
        {
            return _urlLoading;
        }

        set
        {
            if (value != null)
            {
                _urlLoading = value;
                _urlLoadingOriginal = new EventHandler<UrlLoadingEventArgs>(
                    (sender, args) =>
                    {
                        var bsArgs = new BSUrlLoadingEventArgs(args.Url, FromNativeUrlLoadingStrategy(args.UrlLoadingStrategy));
                        _urlLoading(sender, bsArgs);
                        args.UrlLoadingStrategy = ToNativeUrlLoadingStrategy(bsArgs.UrlLoadingStrategy);
                    });
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
    private EventHandler<BSBlazorWebViewInitializingEventArgs>? _blazorWebViewInitializing;

    public EventHandler<BSBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing
    {
        get
        {
            return _blazorWebViewInitializing;
        }

        set
        {
            if (value != null)
            {
                _blazorWebViewInitializing = value;
                _blazorWebViewInitializingOriginal = new EventHandler<BlazorWebViewInitializingEventArgs>(
                    (sender, args) => _blazorWebViewInitializing(sender,
                    new BSBlazorWebViewInitializingEventArgs
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
    private EventHandler<BSBlazorWebViewInitializedEventArgs>? _blazorWebViewInitialized;

    public EventHandler<BSBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized
    {
        get
        {
            return _blazorWebViewInitialized;
        }
        set
        {
            if (value != null)
            {
                _blazorWebViewInitialized = value;
                _blazorWebViewInitializedOriginal = new EventHandler<BlazorWebViewInitializedEventArgs>(
                    (sender, args) => _blazorWebViewInitialized(sender,
                    new BSBlazorWebViewInitializedEventArgs { WebView = new WinFormsWebViewProxy(args.WebView) }));
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

    public void AddRootComponents(IEnumerable<BSRootComponent> rootComponents)
    {
        foreach(var rc in rootComponents.Select(x => new RootComponent(x.Selector, x.ComponentType, x.Parameters)))
        {
            _original.RootComponents.Add(rc);
        }
    }

    public JSComponentConfigurationStore JSComponents => _original.RootComponents.JSComponents;
}
