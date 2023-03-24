namespace Baksteen.Avalonia.Blazor;

using Baksteen.Avalonia.Blazor.Contract;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class WinFormsWebViewProxy : IBSWebView
{
    private readonly WebView2 _webView;
    private readonly Lazy<CoreWebView2Proxy> _coreWebView2Proxy;

    public WinFormsWebViewProxy(WebView2 webView2)
    {
        _webView = webView2;
        _coreWebView2Proxy = new(() => new CoreWebView2Proxy(_webView.CoreWebView2));
    }

    public Uri Source { get => _webView.Source; set => _webView.Source = value; }
    public double ZoomFactor { get => _webView.ZoomFactor; set => _webView.ZoomFactor = value; }

    public IBSCoreWebView CoreWebView2 { get => _coreWebView2Proxy.Value; }

    public Task EnsureCoreWebView2Async(CoreWebView2Environment environment)
    {
        return _webView.EnsureCoreWebView2Async(environment, null);
    }
}
