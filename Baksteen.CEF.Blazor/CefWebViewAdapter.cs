using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baksteen.Blazor.Contract;
using Baksteen.Blazor.WinForms;
using Microsoft.AspNetCore.Components;
using CefSharp.WinForms;
using CefSharp;

namespace Baksteen.Blazor.CefSharpWinForms;

public class CefWebViewAdapter : IBSWebView
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
