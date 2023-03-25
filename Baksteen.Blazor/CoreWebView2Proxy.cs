namespace Baksteen.Avalonia.Blazor;

using Baksteen.Avalonia.Blazor.Contract;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal class CoreWebView2Proxy : IBSCoreWebView
{
    private readonly CoreWebView2 _coreWebView2;

    public bool AreDevToolsEnabled
    {
        get => _coreWebView2.Settings.AreDevToolsEnabled;
        set => _coreWebView2.Settings.AreDevToolsEnabled = value;
    }

    public bool AreDefaultContextMenusEnabled
    {
        get => _coreWebView2.Settings.AreDefaultContextMenusEnabled;
        set => _coreWebView2.Settings.AreDefaultContextMenusEnabled = value;
    }

    public bool IsStatusBarEnabled
    {
        get => _coreWebView2.Settings.IsStatusBarEnabled;
        set => _coreWebView2.Settings.IsStatusBarEnabled = value;
    }

    private bool _onWebResourceRequestedCoupled;
    private event EventHandler<BSWebResourceRequestedEventArgs>? _onWebResourceRequested;

    public event EventHandler<BSWebResourceRequestedEventArgs> WebResourceRequested
    {
        add
        {
            _onWebResourceRequested += value;
            if(!_onWebResourceRequestedCoupled)
            {
                _coreWebView2.WebResourceRequested += ConvertWebResourceRequested;
                _onWebResourceRequestedCoupled = true;
            }
        }
        remove
        {
            _onWebResourceRequested -= value;
            if(_onWebResourceRequested==null && _onWebResourceRequestedCoupled)
            {
                _coreWebView2.WebResourceRequested -= ConvertWebResourceRequested;
                _onWebResourceRequestedCoupled = false;
            }
        }
    }

    private protected static string GetHeaderString(IDictionary<string, string> headers) =>
    string.Join(Environment.NewLine, headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

    private void ConvertWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        var tmp = new BSWebResourceRequestedEventArgs
        {            
            ResourceContext = e.ResourceContext,
        };

        if(e.Request!=null)
        {
            tmp.Request = new()
            {
                Content = e.Request.Content,
                Headers = new System.Collections.Generic.Dictionary<string, string>(e.Request.Headers),
                Method = e.Request.Method,
                Uri = e.Request.Uri
            };
        }

        if(e.Response != null)
    {
            tmp.Response = new()
        {
                Content = e.Response.Content,
                Headers = new System.Collections.Generic.Dictionary<string, string>(e.Response.Headers),
                ReasonPhrase = e.Response.ReasonPhrase,
                StatusCode = e.Response.StatusCode,
            };
        }

        //if(e.GetDeferral() != null)
        //{
        //    tmp.Deferral = e.GetDeferral();
        //}

        _onWebResourceRequested?.Invoke(this, tmp);

        if(tmp.Response!=null)
        {
            e.Response = _coreWebView2.Environment.CreateWebResourceResponse(tmp.Response.Content, tmp.Response.StatusCode, tmp.Response.ReasonPhrase, GetHeaderString(tmp.Response.Headers));
        }
    }

    private bool _onNavigationStartingCoupled = false;
    private event EventHandler<BSNavigationStartingEventArgs>? _onNavigationStarting;

    public event EventHandler<BSNavigationStartingEventArgs> NavigationStarting
    {
        add
        {
            _onNavigationStarting += value;

            if(!_onNavigationStartingCoupled)
            {
                _coreWebView2.NavigationStarting += ConvertNavigationStarting;
                _onNavigationStartingCoupled = true;
            }
        }
        remove
        {
            _onNavigationStarting -= value;

            if(_onNavigationStarting == null && _onNavigationStartingCoupled)
            {
                _coreWebView2.NavigationStarting -= ConvertNavigationStarting;
                _onNavigationStartingCoupled = false;
            }
        }
    }

    private void ConvertNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        _onNavigationStarting?.Invoke(this, new BSNavigationStartingEventArgs
        {
            AdditionalAllowedFrameAncestors = e.AdditionalAllowedFrameAncestors,
            Cancel = e.Cancel,
            IsRedirected = e.IsRedirected,
            IsUserInitiated = e.IsUserInitiated,
            NavigationId = e.NavigationId,
            RequestHeaders = new System.Collections.Generic.Dictionary<string, string>(e.RequestHeaders),
            Uri = e.Uri,
        });
    }

    public event EventHandler<CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested
    {
        add
        {
            _coreWebView2.NewWindowRequested += value;
        }
        remove
        {
            _coreWebView2.NewWindowRequested -= value;
        }
    }

    public event EventHandler<BSWebMessageReceivedEventArgs>? WebMessageReceived;

    public CoreWebView2Proxy(CoreWebView2 coreWebView2)
    {
        _coreWebView2 = coreWebView2;
        _coreWebView2.WebMessageReceived += (s, e) => { WebMessageReceived?.Invoke(this, new BSWebMessageReceivedEventArgs {
            Uri = new Uri(e.Source), 
            WebMessage = e.TryGetWebMessageAsString()
        }
        ); };
    }

    public Task<string> AddScriptToExecuteOnDocumentCreatedAsync(string javaScript)
    {
        return _coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(javaScript);
    }

    public void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
    {
        _coreWebView2.AddWebResourceRequestedFilter(uri, ResourceContext);
    }

    public void PostWebMessageAsString(string webMessageAsString)
    {
        _coreWebView2.PostWebMessageAsString(webMessageAsString);
    }
}
