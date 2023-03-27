namespace Baksteen.Blazor.CefSharpWinForms;

using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Baksteen.Blazor.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics.Tracing;
using System.Text.Encodings.Web;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using CefSharp.WinForms;
using CefSharp;

public partial class CefCoreWebView2Adapter : IBSCoreWebView
{
    private readonly ChromiumWebBrowser _webView;
    private readonly Dispatcher _dispatcher;
    private List<(string url, CoreWebView2WebResourceContext context)> _filters = new();

    private static Dictionary<string, string> ConvertHeaders(NameValueCollection nvc)
    {
        return nvc.AllKeys.Where(k => k != null).ToDictionary(k => k!, k => nvc[k]!);
    }

    public CefCoreWebView2Adapter(ChromiumWebBrowser chromiumWebBrowser, Dispatcher dispatcher)
    {
        _webView = chromiumWebBrowser;
        _dispatcher = dispatcher;
        _webView.LifeSpanHandler = new NewWindowLifeSpanHandler(this);
        _webView.RequestHandler = new CustomRequestHandler(this);
        _webView.ActivateBrowserOnCreation = true;
        _webView.JavascriptMessageReceived += _webView_JavascriptMessageReceived;
        _webView.FrameLoadEnd += _webview_FrameLoadEnd;
        _webView.AddressChanged += _webView_AddressChanged;
        //_webView.
        //_chromiumWebBrowser.AddressChanged += _chromiumWebBrowser_AddressChanged;
        //_chromiumWebBrowser.FrameLoadStart += _chromiumWebBrowser_FrameLoadStart;
    }

    private void _webView_AddressChanged(object? sender, AddressChangedEventArgs e)
    {
        //throw new NotImplementedException();
    }

    private void _webview_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
    {
        //Wait for the MainFrame to finish loading
        if(e.Frame.IsMain)
        {
            Trace.WriteLine("main frame finished loading");

            _ = e.Frame.EvaluateScriptAsync(@"

        		window.__receiveMessageCallbacks = [];
			    window.__dispatchMessageCallback = function(message) {
				    window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			    };

                window.external = {
                    // this means: send message from Javascript to C#
                    sendMessage: message => {
                        // console.log(message);
                        CefSharp.PostMessage(message);
                        // window.chrome.webview.postMessage(message);
                    },
                    // this means: hook up callback to receive messages from C# to Javascript
					receiveMessage: callback => {
                        // console.log('hooked up callback');
    					window.__receiveMessageCallbacks.push(callback);
                        // window.chrome.webview.addEventListener('message', e => callback(e.data));
                    }
                };
        
                Blazor.start();
			    (function () {
				    window.onpageshow = function(event) {
					    if (event.persisted) {
						    window.location.reload();
					    }
				    };
			    })();

                ");
        }
    }

    private void _webView_JavascriptMessageReceived(object? sender, JavascriptMessageReceivedEventArgs e)
    {
        var tmp = new BSWebMessageReceivedEventArgs
        {
            Uri = new Uri(e.Browser.MainFrame.Url),     // new Uri(e.Frame.Url), // this leads to ObjectDisposedException, frame already disposed
            WebMessage = e.Message.ToString()
        };

        _ = _dispatcher.InvokeAsync(async () =>
        {
            WebMessageReceived?.Invoke(this, tmp);
            await Task.CompletedTask;
        });
    }

    public bool AreDevToolsEnabled { get; set; }
    public bool AreDefaultContextMenusEnabled { get; set; }
    public bool IsStatusBarEnabled { get; set; }

    public event EventHandler<BSWebResourceRequestedEventArgs>? WebResourceRequested;
    public event EventHandler<BSNavigationStartingEventArgs>? NavigationStarting;
    public event EventHandler<BSNewWindowRequestedEventArgs>? NewWindowRequested;
    public event EventHandler<BSWebMessageReceivedEventArgs>? WebMessageReceived;

    public async Task<string> AddScriptToExecuteOnDocumentCreatedAsync(string javaScript)
    {
        await Task.CompletedTask;
        return "";
    }

    public void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
    {
        _filters.Add((uri,ResourceContext));
    }

    public void PostWebMessageAsString(string webMessageAsString)
    {
        var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(webMessageAsString);
        _webView.EvaluateScriptAsync($"__dispatchMessageCallback(\"{messageJSStringLiteral}\")");
    }

    public Uri Source
    {
        get => new Uri(_webView.Address);
        set
        {
            _webView.LoadUrl(value.AbsoluteUri);
        }
    }
}
