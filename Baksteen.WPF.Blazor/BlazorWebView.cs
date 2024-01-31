using Baksteen.Blazor;
using Baksteen.Blazor.CefSharpWinForms;
using Baksteen.Blazor.Contract;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Baksteen.WPF.Blazor;

public class BlazorWebView : WindowsFormsHost
{
    private IBSBlazorWebView? _blazorWebView;
    private string? _hostPage;

    public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(nameof(ZoomFactor), typeof(double), typeof(BlazorWebView), new PropertyMetadata(1.0));
    public static readonly DependencyProperty ServicesProperty = DependencyProperty.Register(nameof(Services), typeof(IServiceProvider), typeof(BlazorWebView), new PropertyMetadata(default(IServiceProvider)));
    public static readonly DependencyProperty RootComponentsProperty = DependencyProperty.Register(nameof(RootComponents), typeof(List<BSRootComponent>), typeof(BlazorWebView), new PropertyMetadata(null));
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(Uri), typeof(BlazorWebView), new PropertyMetadata(new Uri("http://localhost/")));

    //public static readonly StyledProperty<string> LanguageProperty = AvaloniaProperty.Register<CoreWebView2CreationProperties, string>(nameof(Language));

    public string? HostPage
    {
        get
        {
            if (_blazorWebView != null)
            {
                _hostPage = _blazorWebView.HostPage;
            }
            return _hostPage;
        }

        set
        {
            if (_hostPage != value)
            {
                _hostPage = value;
                if (_blazorWebView != null)
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
            var _source = (Uri)GetValue(SourceProperty);
            var _sourceWebView = _blazorWebView?.WebView.Source;
            if (_sourceWebView != null && _sourceWebView != _source)
                SetValue(SourceProperty, _source);

            return _sourceWebView ?? _source;
        }

        set
        {
            SetValue(SourceProperty, value);
            if (_blazorWebView != null)
                _blazorWebView.WebView.Source = value;
        }
    }

    public double ZoomFactor
    {
        get
        {
            var _zoomFactorprop = (double)GetValue(ZoomFactorProperty);
            var _zoomFactorWebView = _blazorWebView?.WebView.ZoomFactor;
            if (_zoomFactorWebView != null && _zoomFactorWebView != _zoomFactorprop)
                SetValue(ZoomFactorProperty, _zoomFactorWebView);

            return _zoomFactorWebView ?? _zoomFactorprop;
        }

        set
        {
            SetValue(ZoomFactorProperty, value);
            if (_blazorWebView != null)
                _blazorWebView.WebView.ZoomFactor = value;
        }
    }

    public IServiceProvider Services
    {
        get => (IServiceProvider)GetValue(ServicesProperty);
        set
        {
            SetValue(ServicesProperty, value);
            if (_blazorWebView != null)
                _blazorWebView.Services = value;
        }
    }

    public List<BSRootComponent> RootComponents
    {
        get => (List<BSRootComponent>)GetValue(RootComponentsProperty);
        set => SetValue(RootComponentsProperty, value);
    }

    public BlazorWebView()
    {
        Unloaded += BlazorWebView_Unloaded;
        Loaded += BlazorWebView_Loaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RootComponents = new List<BSRootComponent>();
        if (Services != null)
        {
            if (Services.GetService<BSBlazorMarkerService>() is null)
            {
                throw new InvalidOperationException(
                    "Unable to find the required services. " +
                    $"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(WPF.Blazor.BSBlazorWebViewServiceCollectionExtensions.AddBSWPFBlazorWebView)}' in the application startup code.");
            }
        }

        // use PureBlazorWebView that operates on a WinForms WebView2
        _blazorWebView = new CefSharpBlazorWebView()//BlazorWebViewPure(new Microsoft.Web.WebView2.WinForms.WebView2())
        {
            HostPage = _hostPage,
            Services = Services!,
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

        _blazorWebView.WebView.ZoomFactor = Math.Clamp(ZoomFactor, 0.1, 4.0);
        //_blazorWebView.RootComponents.AddRange(_rootComponents); // this was used for the original winforms BlazorWebView
        _blazorWebView.AddRootComponents(RootComponents);
        _blazorWebView.BlazorWebViewInitialized = OnBlazorWebViewInitialized;
        _blazorWebView.BlazorWebViewInitializing = OnBlazorWebViewInitializing;

        Child = (System.Windows.Forms.Control)_blazorWebView.PlatformSpecificComponent;
    }

    private void BlazorWebView_Loaded(object sender, RoutedEventArgs e)
    {
        OnApplyTemplate();
    }

    private void BlazorWebView_Unloaded(object sender, RoutedEventArgs e)
    {
        _blazorWebView?.Dispose();
        _blazorWebView = null;
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
}
