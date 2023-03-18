namespace Baksteen.Avalonia.Blazor;

using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

internal class CoreWebView2Proxy : ICoreWebView2
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

    public event EventHandler<CoreWebView2WebResourceRequestedEventArgs> WebResourceRequested
    {
        add
        {
            _coreWebView2.WebResourceRequested += value;
        }
        remove
        {
            _coreWebView2.WebResourceRequested -= value;
        }
    }

    public event EventHandler<CoreWebView2NavigationStartingEventArgs> NavigationStarting
    {
        add
        {
            _coreWebView2.NavigationStarting += value;
        }
        remove
        {
            _coreWebView2.NavigationStarting -= value;
        }
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

    public event EventHandler<CoreWebView2WebMessageReceivedEventArgs> WebMessageReceived
    {
        add
        {
            _coreWebView2.WebMessageReceived += value;
        }
        remove
        {
            _coreWebView2.WebMessageReceived -= value;
        }
    }

    public CoreWebView2Proxy(CoreWebView2 coreWebView2)
    {
        _coreWebView2 = coreWebView2;
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
