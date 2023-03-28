using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.WinForms;
using Microsoft.AspNetCore.Components;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Baksteen.Blazor.CefSharpWinForms;

internal class CefWebViewAdapter : IBSWebView
{
    private readonly ChromiumWebBrowser _webView;
    private readonly CefCoreWebView2Adapter _coreWebView;

    public CefWebViewAdapter(ChromiumWebBrowser chromiumWebBrowser, Dispatcher dispatcher)
    {
        _webView = chromiumWebBrowser;
        _coreWebView = new CefCoreWebView2Adapter(chromiumWebBrowser, dispatcher);
    }

    public Uri Source
    {
        get => _coreWebView.Source;
        set => _coreWebView.Source = value;
    }

    public double ZoomFactor
    {
        get => Task.Run(async () => { return await _webView.GetZoomLevelAsync(); }).Result;
        set => _webView.SetZoomLevel(value);
    }

    public IBSCoreWebView CoreWebView2 => _coreWebView;

    public Task EnsureCoreWebView2Async(CoreWebView2Environment environment)
    {
        return Task.CompletedTask;
    }
}
