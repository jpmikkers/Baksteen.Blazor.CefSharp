using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Baksteen.Blazor.Contract;

public interface IBSCoreWebView
{
    //
    // Summary:
    //     Determines whether the user is able to use the context menu or keyboard shortcuts
    //     to open the DevTools window.
    //
    // Remarks:
    //     The default value is true.
    public bool AreDevToolsEnabled { get; set; }

    //
    // Summary:
    //     Determines whether the default context menus are shown to the user in WebView.
    //
    // Remarks:
    //     The default value is true.
    public bool AreDefaultContextMenusEnabled { get; set; }

    //
    // Summary:
    //     Determines whether the status bar is displayed.
    //
    // Remarks:
    //     The status bar is usually displayed in the lower left of the WebView and shows
    //     things such as the URI of a link when the user hovers over it and other information.
    //     The default value is true. The status bar UI can be altered by web content and
    //     should not be considered secure.
    public bool IsStatusBarEnabled { get; set; }

    //
    // Summary:
    //     Posts a message that is a simple string rather than a JSON string representation
    //     of a JavaScript object.
    //
    // Parameters:
    //   webMessageAsString:
    //     The web message to be posted to the top level document in this WebView.
    //
    // Remarks:
    //     This behaves in exactly the same manner as Microsoft.Web.WebView2.Core.CoreWebView2.PostWebMessageAsJson(System.String),
    //     but the data property of the event arg of the window.chrome.webview message is
    //     a string with the same value as webMessageAsString. Use this instead of Microsoft.Web.WebView2.Core.CoreWebView2.PostWebMessageAsJson(System.String)
    //     if you want to communicate using simple strings rather than JSON objects.
    void PostWebMessageAsString(string webMessageAsString);

    //
    // Summary:
    //     Adds a URI and resource context filter for the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event.
    //
    // Parameters:
    //   uri:
    //     An URI to be added to the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event.
    //
    //   ResourceContext:
    //     A resource context filter to be added to the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event.
    //
    // Remarks:
    //     A web resource request with a resource context that matches this filter's resource
    //     context and a URI that matches this filter's URI wildcard string will be raised
    //     via the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested event.
    //     The uri parameter value is a wildcard string matched against the URI of the web
    //     resource request. This is a glob style wildcard string in which a * matches zero
    //     or more characters and a ? matches exactly one character. These wildcard characters
    //     can be escaped using a backslash just before the wildcard character in order
    //     to represent the literal * or ?. The matching occurs over the URI as a whole
    //     string and not limiting wildcard matches to particular parts of the URI. The
    //     wildcard filter is compared to the URI after the URI has been normalized, any
    //     URI fragment has been removed, and non-ASCII hostnames have been converted to
    //     punycode. Specifying a nullptr for the uri is equivalent to an empty string which
    //     matches no URIs. For more information about resource context filters, navigate
    //     to Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.
    //     URI Filter String Request URI Match Notes
    //     * https://contoso.com/a/b/c Yes A single * will match all URIs
    //     *://contoso.com/* https://contoso.com/a/b/c Yes Matches everything in contoso.com
    //     across all schemes
    //     *://contoso.com/* https://example.com/?https://contoso.com/ Yes But also matches
    //     a URI with just the same text anywhere in the URI
    //     example https://contoso.com/example No The filter does not perform partial matches
    //     *example https://contoso.com/example Yes The filter matches across URI parts
    //     *example https://contoso.com/path/?example Yes The filter matches across URI
    //     parts
    //     *example https://contoso.com/path/?query#example No The filter is matched against
    //     the URI with no fragment
    //     *example https://example No The URI is normalized before filter matching so the
    //     actual URI used for comparison is https://example.com/
    //     *example/ https://example Yes Just like above, but this time the filter ends
    //     with a / just like the normalized URI
    //     https://xn--qei.example/ https://❤.example/ Yes Non-ASCII hostnames are normalized
    //     to punycode before wildcard comparison
    //     https://❤.example/ https://xn--qei.example/ No Non-ASCII hostnames are normalized
    //     to punycode before wildcard comparison
    void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext);

    //
    // Summary:
    //     WebResourceRequested is raised when the WebView is performing a URL request to
    //     a matching URL and resource context filter that was added with Microsoft.Web.WebView2.Core.CoreWebView2.AddWebResourceRequestedFilter(System.String,Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext).
    //
    // Remarks:
    //     At least one filter must be added for the event to be raised. The web resource
    //     requested may be blocked until the event handler returns if a deferral is not
    //     taken on the event args. If a deferral is taken, then the web resource requested
    //     is blocked until the deferral is completed. If this event is subscribed in the
    //     Microsoft.Web.WebView2.Core.CoreWebView2.NewWindowRequested handler it should
    //     be called after the new window is set. For more details see Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs.NewWindow.
    //     Currently this only supports file, http, and https URI schemes.
    event EventHandler<BSWebResourceRequestedEventArgs> WebResourceRequested;

    //
    // Summary:
    //     NavigationStarting is raised when the WebView main frame is requesting permission
    //     to navigate to a different URI.
    //
    // Remarks:
    //     Redirects raise this event as well, and the navigation id is the same as the
    //     original one. You may block corresponding navigations until the event handler
    //     returns.
    event EventHandler<BSNavigationStartingEventArgs> NavigationStarting;

    //
    // Summary:
    //     NewWindowRequested is raised when content inside the WebView requests to open
    //     a new window, such as through window.open().
    //
    // Remarks:
    //     The app passes a target WebView that is considered the opened window. If a deferral
    //     is not taken on the event args, scripts that resulted in the new window that
    //     are requested are blocked until the event handler returns. If a deferral is taken,
    //     then scripts are blocked until the deferral is completed.
    event EventHandler<CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

    //
    // Summary:
    //     Adds the provided JavaScript to a list of scripts that should be run after the
    //     global object has been created, but before the HTML document has been parsed
    //     and before any other script included by the HTML document is run.
    //
    // Parameters:
    //   javaScript:
    //     The JavaScript code to be run.
    //
    // Returns:
    //     A script ID that may be passed when calling Microsoft.Web.WebView2.Core.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(System.String).
    //
    // Remarks:
    //     The injected script will apply to all future top level document and child frame
    //     navigations until removed with Microsoft.Web.WebView2.Core.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(System.String).
    //     This is applied asynchronously and you must wait for the returned System.Threading.Tasks.Task`1
    //     to complete before you can be sure that the script is ready to execute on future
    //     navigations. Note that if an HTML document has sandboxing of some kind via [sandbox](https://developer.mozilla.org/docs/Web/HTML/Element/iframe#attr-sandbox)
    //     properties or the [Content-Security-Policy HTTP header](https://developer.mozilla.org/docs/Web/HTTP/Headers/Content-Security-Policy)
    //     this will affect the script run here. So, for example, if the allow-modals keyword
    //     is not set then calls to the alert function will be ignored.
    Task<string> AddScriptToExecuteOnDocumentCreatedAsync(string javaScript);

    //
    // Summary:
    //     WebMessageReceived is raised when the Microsoft.Web.WebView2.Core.CoreWebView2Settings.IsWebMessageEnabled
    //     setting is set and the top-level document of the WebView runs window.chrome.webview.postMessage.
    //
    // Remarks:
    //     The postMessage function is void postMessage(object) where object is any object
    //     supported by JSON conversion. When postMessage is called, the handler's Invoke
    //     method will be called with the object parameter postMessage converted to a JSON
    //     string. If the same page calls postMessage multiple times, the corresponding
    //     WebMessageReceived events are guaranteed to be fired in the same order. However,
    //     if multiple frames call postMessage, there is no guaranteed order. In addition,
    //     WebMessageReceived events caused by calls to postMessage are not guaranteed to
    //     be sequenced with events caused by DOM APIs. For example, if the page runs
    //     chrome.webview.postMessage("message");
    //     window.open();
    //     then the Microsoft.Web.WebView2.Core.CoreWebView2.NewWindowRequested event might
    //     be fired before the WebMessageReceived event. If you need the WebMessageReceived
    //     event to happen before anything else, then in the WebMessageReceived handler
    //     you can post a message back to the page and have the page wait until it receives
    //     that message before continuing.
    public event EventHandler<BSWebMessageReceivedEventArgs> WebMessageReceived;

}
