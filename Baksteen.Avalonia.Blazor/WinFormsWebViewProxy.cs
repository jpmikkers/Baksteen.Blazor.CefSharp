namespace Baksteen.Avalonia.Blazor;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class WinFormsWebViewProxy : IWebView
{
    private readonly WebView2 _webView;

    public WinFormsWebViewProxy(WebView2 webView2)
    {
        _webView = webView2;
    }

    public Uri Source { get => _webView.Source; set => _webView.Source = value; }
    public double ZoomFactor { get => _webView.ZoomFactor; set => _webView.ZoomFactor = value; }

    public CoreWebView2 CoreWebView2 { get => _webView.CoreWebView2; }

    public Task EnsureCoreWebView2Async(CoreWebView2Environment environment)
    {
        return _webView.EnsureCoreWebView2Async(environment, null);
    }
}
