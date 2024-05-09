using Baksteen.Blazor.CefSharpWPF.Glue;
using Baksteen.Blazor.CefSharpWPF.Handlers;
using Baksteen.Blazor.CefSharpWPF.Tools;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;

namespace Baksteen.Blazor.CefSharpWPF;

public partial class CefSharpWebViewManager : WebViewManager
{
    // Using an IP address means that WebView2 doesn't wait for any DNS resolution,
    // making it substantially faster. Note that this isn't real HTTP traffic, since
    // we intercept all the requests within this origin.
    internal static readonly string AppHostAddress = "0.0.0.0";

    /// <summary>
    /// Gets the application's base URI. Defaults to <c>https://0.0.0.0/</c>
    /// </summary>
    protected static readonly string AppOrigin = $"https://{AppHostAddress}/";

    internal static readonly Uri AppOriginUri = new(AppOrigin);

    private readonly ChromiumWebBrowser _webview;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dispatcher _dispatcher;
    private readonly bool _developerToolsEnabled;

    //private readonly BSBlazorWebViewDeveloperTools _developerTools;
    private readonly string _contentRootRelativeToAppRoot;
    private readonly Action<BSUrlLoadingEventArgs> _onUrlLoadingAction;
    private readonly Task<bool> _webviewReadyTask;
    private readonly FilteringRequestHandler _filteringRequestHandler;
    private Task<JavascriptResponse>? _prevMessageTask;
    //private object _coreWebView2Environment;

    public CefSharpWebViewManager(
        ChromiumWebBrowser webView,                 // ok
        IServiceProvider serviceProvider,           // ok
        Dispatcher dispatcher,                      // ok
        IFileProvider fileProvider,                 // ?
        JSComponentConfigurationStore jsComponents, // ok
        string contentRootRelativeToAppRoot,        // ?
        string hostPagePathWithinFileProvider,
        Action<BSUrlLoadingEventArgs> onUrlLoadingAction
        ) :    // ok
        base(
            serviceProvider,
            dispatcher,
            AppOriginUri,
            fileProvider,
            jsComponents,
            hostPagePathWithinFileProvider)
    {
        _webview = webView;
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;

        _developerToolsEnabled = serviceProvider.GetRequiredService<WebViewDeveloperToolsMarker>().Enabled;

        _contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;
        _onUrlLoadingAction = onUrlLoadingAction;
        _filteringRequestHandler = new FilteringRequestHandler(CoreWebView2_NavigationStarting, HandleWebResourceRequest);

        _webview.RequestHandler = _filteringRequestHandler;
        _webview.LifeSpanHandler = new LifeSpanHandler(CoreWebView2_NewWindowRequested);
        _webview.FrameLoadEnd += _webview_FrameLoadEnd;
        _webview.JavascriptMessageReceived += _webview_JavascriptMessageReceived;

        // Unfortunately the CoreWebView2 can only be instantiated asynchronously.
        // We want the external API to behave as if initalization is synchronous,
        // so keep track of a task we can await during LoadUri.
        _webviewReadyTask = TryInitializeWebView2();
    }

    private async Task<bool> TryInitializeWebView2()
    {
        //BSBlazorWebViewInitializingEventArgs args = new();
        //_blazorWebviewInitializingAction(args);

        //var userDataFolder = args.UserDataFolder ?? GetWebView2UserDataFolder();
        //_coreWebView2Environment = await CoreWebView2Environment.CreateAsync(
        //    browserExecutableFolder: args.BrowserExecutableFolder,
        //    userDataFolder: userDataFolder,
        //    options: args.EnvironmentOptions)
        //.ConfigureAwait(true);

#if PARKEDSTUFF
        await _webview.EnsureCoreWebView2Async(_coreWebView2Environment);

        var developerTools = _developerTools;

        ApplyDefaultWebViewSettings(developerTools);
#endif

        //_blazorWebviewInitializedAction(new BSBlazorWebViewInitializedEventArgs
        //{
        //    //WebView = _webview,
        //});

        _filteringRequestHandler.AddWebResourceRequestedFilter($"{AppOrigin}*", BSCoreWebView2WebResourceContext.All);
        // originally _webview.CoreWebView2.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);

#if PARKEDSTUFF
        _webview.CoreWebView2.WebResourceRequested += async (s, eventArgs) =>
        {
            await HandleWebResourceRequest(eventArgs);
        };

        _webview.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        _webview.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

        // The code inside blazor.webview.js is meant to be agnostic to specific webview technologies,
        // so the following is an adaptor from blazor.webview.js conventions to WebView2 APIs
        await _webview.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
				window.external = {
                    // this means: send message from Javascript to C#
					sendMessage: message => {
                        // console.log(message);
						window.chrome.webview.postMessage(message);
					},
                    // this means: hook up callback to receive messages from C# to Javascript
					receiveMessage: callback => {
                        // console.log(callback);
						window.chrome.webview.addEventListener('message', e => callback(e.data));
					}
				};
			")
         .ConfigureAwait(true);

        QueueBlazorStart();

        _webview.CoreWebView2.WebMessageReceived += (s, e) =>
        {

            Debug.WriteLine($"MessageReceived(uri={e.Uri}, message={e.WebMessage})");

            MessageReceived(e.Uri!, e.WebMessage!);
        };
#endif
        await Task.CompletedTask;
        return true;
    }

    protected async virtual Task HandleWebResourceRequest(BSWebResourceRequestedEventArgs eventArgs)
    {
        // see https://github.com/dotnet/maui/blob/5a95d45fd6180fa52df731f240fa6884609bde42/src/BlazorWebView/src/SharedSource/WebView2WebViewManager.cs#L300

        // Unlike server-side code, we get told exactly why the browser is making the request,
        // so we can be smarter about fallback. We can ensure that 'fetch' requests never result
        // in fallback, for example.
        var allowFallbackOnHostPage =
            eventArgs.ResourceContext == BSCoreWebView2WebResourceContext.Document ||
            eventArgs.ResourceContext == BSCoreWebView2WebResourceContext.Other; // e.g., dev tools requesting page source

        var requestUri = QueryStringHelper.RemovePossibleQueryString(eventArgs.Request.Uri);
        Debug.WriteLine($"HandleWebResourceRequest(uri={eventArgs.Request.Uri})");

        if(TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
        {
            BSStaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, requestUri, ref statusCode, ref content, headers);

            var autoCloseStream = new AutoCloseOnReadCompleteStream(content);

            eventArgs.Response = new BSWebResourceResponse
            {
                Content = autoCloseStream,
                Headers = new Dictionary<string, string>(headers),  // new Dictionary<string, string>(tmpResponse.Headers),
                ReasonPhrase = statusMessage,                       // tmpResponse.ReasonPhrase,
                StatusCode = statusCode,                            // tmpResponse.StatusCode,
            };
        }

        await Task.CompletedTask;
    }

    private void _webview_JavascriptMessageReceived(object? sender, JavascriptMessageReceivedEventArgs e)
    {
        if(e.Message is not null)
        {
            // TODO, dispatch maybe?
            MessageReceived(new Uri(e.Browser.MainFrame.Url), e.Message.ToString() ?? "");
        }
    }

    private void _webview_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
    {
        //Wait for the MainFrame to finish loading
        if(e.Frame.IsMain)
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

    protected override void NavigateCore(Uri absoluteUri)
    {
        _ = Dispatcher.InvokeAsync(async () =>
        {
            var isWebviewInitialized = await _webviewReadyTask;

            if(isWebviewInitialized)
            {
                // orig _logger.NavigatingToUri(absoluteUri);
                // orig _webview.Source = absoluteUri;
                _webview.LoadUrl(absoluteUri.AbsoluteUri);
            }
        });
    }

    protected override void SendMessage(string message)
    {
        var messageJSStringLiteral = JavaScriptEncoder.Default.Encode(message);
        // seems like a good idea to wait on the previous message to be sent first?
        if(_prevMessageTask != null)
        {
            _prevMessageTask.GetAwaiter().GetResult();
        }
        _prevMessageTask = _webview.EvaluateScriptAsync($"__CSToJSMessageEventTarget.dispatchEvent(new CustomEvent('message', {{ detail : \"{messageJSStringLiteral}\" }}))");
    }

#if PARKEDSTUFF
    private static string? GetWebView2UserDataFolder()
    {
        if(Assembly.GetEntryAssembly() is { } mainAssembly)
        {
            // In case the application is running from a non-writable location (e.g., program files if you're not running
            // elevated), use our own convention of %LocalAppData%\YourApplicationName.WebView2.
            // We may be able to remove this if https://github.com/MicrosoftEdge/WebView2Feedback/issues/297 is fixed.
            var applicationName = mainAssembly.GetName().Name;
            var result = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                $"{applicationName}.WebView2");

            return result;
        }

        return null;
    }
#endif

    private void CoreWebView2_NavigationStarting(BSNavigationStartingEventArgs args)
    {
        if(Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
        {
            var callbackArgs = BSUrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, AppOriginUri);

            _onUrlLoadingAction(callbackArgs);

            if(callbackArgs.UrlLoadingStrategy == BSUrlLoadingStrategy.OpenExternally)
            {
                LaunchUriInExternalBrowser(uri);
            }

            args.Cancel = callbackArgs.UrlLoadingStrategy != BSUrlLoadingStrategy.OpenInWebView;
        }
    }

    private void CoreWebView2_NewWindowRequested(BSNewWindowRequestedEventArgs args)
    {
        // Intercept _blank target <a> tags to always open in device browser.
        // The ExternalLinkCallback is not invoked.
        if(Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
        {
            LaunchUriInExternalBrowser(uri);
            args.Handled = true;
        }
    }

    private void LaunchUriInExternalBrowser(Uri uri)
    {
        using(var launchBrowser = new Process())
        {
            launchBrowser.StartInfo.UseShellExecute = true;
            launchBrowser.StartInfo.FileName = uri.ToString();
            launchBrowser.Start();
        }
    }
}
