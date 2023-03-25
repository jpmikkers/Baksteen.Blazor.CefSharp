using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Baksteen.Blazor;
using Baksteen.Blazor.Contract;
using Baksteen.Blazor.WinForms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Baksteen.Avalonia.Blazor;

public class BlazorWebView : NativeControlHost
{
    private Uri _source = new Uri("http://localhost/");
    private IBSBlazorWebView? _blazorWebView;
    private double _zoomFactor = 1.0;
    private string? _hostPage;
    private IServiceProvider _serviceProvider = default!;
    private List<BSRootComponent> _rootComponents = new();

    /// <summary>
    /// The <see cref="AvaloniaProperty" /> which backs the <see cref="ZoomFactor" /> property.
    /// </summary>
    public static readonly DirectProperty<BlazorWebView, double> ZoomFactorProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, double>(
            nameof(ZoomFactor),
            x => x.ZoomFactor,
            (x, y) => x.ZoomFactor = y);

    public static readonly DirectProperty<BlazorWebView, IServiceProvider> ServicesProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, IServiceProvider>(
            nameof(Services),
            x => x.Services,
            (x, y) => x.Services = y);

    public static readonly DirectProperty<BlazorWebView, List<BSRootComponent>> RootComponentsProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, List<BSRootComponent>>(
            nameof(RootComponents),
            x => x.RootComponents,
            (x, y) => x.RootComponents = y);

    /*
		/// <summary>
		/// The <see cref="AvaloniaProperty" /> which backs the <see cref="Source" /> property.
		/// </summary>
		public static readonly DirectProperty<WebView, Uri> SourceProperty 
			= AvaloniaProperty.RegisterDirect<WebView, Uri>(
				nameof(Source), 
				x => x.Source, 
				(x, y) => x.Source = y);
	*/
    //public static readonly StyledProperty<string> LanguageProperty = AvaloniaProperty.Register<CoreWebView2CreationProperties, string>(nameof(Language));

    public string? HostPage
    {
        get
        {
            if(_blazorWebView != null)
            {
                _hostPage = _blazorWebView.HostPage;
            }
            return _hostPage;
        }

        set
        {
            if(_hostPage != value)
            {
                _hostPage = value;
                if(_blazorWebView != null)
                {
                    _blazorWebView.HostPage = value;
                }
            }
        }
    }

    public Uri Source
    {
        get
        {
            if(_blazorWebView != null)
            {
                _source = _blazorWebView.WebView.Source;
            }
            return _source;
        }

        set
        {
            if(_source != value)
            {
                _source = value;
                if(_blazorWebView != null)
                {
                    _blazorWebView.WebView.Source = value;
                }
            }
        }
    }

    public double ZoomFactor
    {
        get
        {
            if(_blazorWebView != null)
            {
                _zoomFactor = _blazorWebView.WebView.ZoomFactor;
            }
            return _zoomFactor;
        }

        set
        {
            if(_zoomFactor != value)
            {
                _zoomFactor = value;
                if(_blazorWebView != null)
                {
                    _blazorWebView.WebView.ZoomFactor = value;
                }
            }
        }
    }

    public IServiceProvider Services
    {
        get
        {
            return _serviceProvider;
        }
        set
        {
            _serviceProvider = value;
            if(_blazorWebView != null)
            {
                _blazorWebView.Services = _serviceProvider;
            }
        }
    }

    public List<BSRootComponent> RootComponents
    {
        get
        {
            return _rootComponents;
        }
        set
        {
            _rootComponents = value;
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if(OperatingSystem.IsWindows())
        {
            if(_serviceProvider != null)
            {
                if(_serviceProvider.GetService<BSAvaloniaBlazorMarkerService>() is null)
                {
                    throw new InvalidOperationException(
                        "Unable to find the required services. " +
                        $"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(AvaloniaBlazorWebViewServiceCollectionExtensions.AddAvaloniaBlazorWebView)}' in the application startup code.");
                }
            }

            // use PureBlazorWebView that operates on a WinForms WebView2
            _blazorWebView = new BlazorWebViewPure(new Microsoft.Web.WebView2.WinForms.WebView2())
            {
                HostPage = _hostPage,
                Services = _serviceProvider!,
            };

            // use Baksteen fork of BlazorWebView that operates on a WinForms WebView2
            //_blazorWebView = new ForkedWinFormsBlazorWebViewProxy(new Microsoft.Web.WebView2.WinForms.WebView2())
            //{
            //    HostPage = _hostPage,
            //    Services = _serviceProvider!,
            //};

            // use original WinForms BlazorWebView
            //_blazorWebView = new WinFormsBlazorWebViewProxy()
            //{
            //    HostPage = _hostPage,
            //    Services = _serviceProvider!,
            //};

            _blazorWebView.WebView.ZoomFactor = Math.Clamp(_zoomFactor, 0.1, 4.0);
            //_blazorWebView.RootComponents.AddRange(_rootComponents); // this was used for the original winforms BlazorWebView
            _blazorWebView.AddRootComponents(_rootComponents);
            _blazorWebView.BlazorWebViewInitialized = OnBlazorWebViewInitialized;
            _blazorWebView.BlazorWebViewInitializing = OnBlazorWebViewInitializing;
            return new PlatformHandle(((System.Windows.Forms.Control)_blazorWebView.PlatformSpecificComponent).Handle, "HWND"); ;
        }

        return base.CreateNativeControlCore(parent);
    }

    protected void OnBlazorWebViewInitializing(object? sender, BSBlazorWebViewInitializingEventArgs args)
    {
        // args.UserDataFolder
        // args.BrowserExecutableFolder
        // args.EnvironmentOptions
    }

    protected void OnBlazorWebViewInitialized(object? sender, BSBlazorWebViewInitializedEventArgs args)
    {
    }

    // TODO JMIK : this doesn't seem to be called :/
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if(OperatingSystem.IsWindows())
        {
            _blazorWebView?.Dispose();
            _blazorWebView = null;
        }

        base.DestroyNativeControlCore(control);
    }
}
