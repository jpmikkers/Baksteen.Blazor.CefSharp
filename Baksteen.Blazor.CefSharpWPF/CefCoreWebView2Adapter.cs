namespace Baksteen.Blazor.CefSharpWPF;

using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.AspNetCore.Components;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

internal partial class CefCoreWebView2Adapter : IBSCoreWebView
{
    private readonly ChromiumWebBrowser _webView;
    private readonly Dispatcher _dispatcher;
    private readonly FilteringRequestHandler _filteringRequestHandler;
    public bool AreDevToolsEnabled { get; set; }
    public bool AreDefaultContextMenusEnabled { get; set; }
    public bool IsStatusBarEnabled { get; set; }

    public event EventHandler<BSWebResourceRequestedEventArgs>? WebResourceRequested;
    public event EventHandler<BSNavigationStartingEventArgs>? NavigationStarting;
    public event EventHandler<BSNewWindowRequestedEventArgs>? NewWindowRequested;
    public event EventHandler<BSWebMessageReceivedEventArgs>? WebMessageReceived;

    private static Dictionary<string, string> ConvertHeaders(NameValueCollection nvc)
    {
        return nvc.AllKeys.Where(k => k != null).ToDictionary(k => k!, k => nvc[k]!);
    }

    public CefCoreWebView2Adapter(ChromiumWebBrowser chromiumWebBrowser, Dispatcher dispatcher)
    {
        _webView = chromiumWebBrowser;
        _dispatcher = dispatcher;
        _webView.LifeSpanHandler = new NewWindowLifeSpanHandler(this);
        _filteringRequestHandler = new FilteringRequestHandler(this);
        _webView.RequestHandler = _filteringRequestHandler;
        //_webView.ActivateBrowserOnCreation = true;
        _webView.JavascriptMessageReceived += _webView_JavascriptMessageReceived;
        _webView.FrameLoadEnd += _webview_FrameLoadEnd;
        _webView.AddressChanged += _webView_AddressChanged1;
        //_webView.
        //_chromiumWebBrowser.AddressChanged += _chromiumWebBrowser_AddressChanged;
        //_chromiumWebBrowser.FrameLoadStart += _chromiumWebBrowser_FrameLoadStart;
    }

    private void _webView_AddressChanged1(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        //throw new NotImplementedException();
    }

    private void _webview_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
    {
        //Wait for the MainFrame to finish loading
        if (e.Frame.IsMain)
        {
            Debug.WriteLine("main frame finished loading");

            _ = e.Frame.EvaluateScriptAsync(@"

                window.__CSToJSMessageEventTarget = new EventTarget();

                window.external = {
                    // this means: send message from Javascript to C#
                    sendMessage: message => {
                        CefSharp.PostMessage(message);
                    },
                    // this means: hook up callback to receive messages from C# to Javascript
                    receiveMessage: callback => {
                        // the 'e' argument in the listener is a CustomEvent() containing the message in its detail property.
                        // that CustomEvent() instance is created and dispatched in the PostWebMessageAsString() c# method 
                        window.__CSToJSMessageEventTarget.addEventListener('message', e => callback(e.detail));
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

    public Task<string> AddScriptToExecuteOnDocumentCreatedAsync(string javaScript) => Task.Run(() => string.Empty);

    public void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
    {
        _filteringRequestHandler.AddWebResourceRequestedFilter(uri, ResourceContext);
    }

    public void PostWebMessageAsString(string webMessageAsString)
    {
        var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(webMessageAsString);
        _webView.EvaluateScriptAsync($"__CSToJSMessageEventTarget.dispatchEvent(new CustomEvent('message', {{ detail : \"{messageJSStringLiteral}\" }}))");
    }

    public Uri Source
    {
        get => new Uri(_webView.Address);
        set => _webView.LoadUrl(value.AbsoluteUri);
    }
}
