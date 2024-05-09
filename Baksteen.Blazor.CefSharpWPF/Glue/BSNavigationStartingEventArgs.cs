using System;
using System.Collections.Generic;

namespace Baksteen.Blazor.CefSharpWPF.Glue;

//
// Summary:
//     Event args for the Microsoft.Web.WebView2.Core.CoreWebView2.NavigationStarting
//     event.
public class BSNavigationStartingEventArgs : EventArgs
{
    //
    // Summary:
    //     Gets the uri of the requested navigation.
    public string Uri
    {
        get; set;
    } = string.Empty;

    //
    // Summary:
    //     true when the new window request was initiated through a user gesture.
    //
    // Remarks:
    //     Examples of user initiated requests are: - Selecting an anchor tag with target
    //     - Programmatic window open from a script that directly run as a result of user
    //     interaction such as via onclick handlers. Non-user initiated requests are programmatic
    //     window opens from a script that are not directly triggered by user interaction,
    //     such as those that run while loading a new page or via timers. The Microsoft
    //     Edge popup blocker is disabled for WebView so the app is able to use this flag
    //     to block non-user initiated popups.
    public bool IsUserInitiated
    {
        get; set;
    }

    //
    // Summary:
    //     true when the navigation is redirected.
    public bool IsRedirected
    {
        get; set;
    }

    //
    // Summary:
    //     Gets the HTTP request headers for the navigation.
    //
    // Remarks:
    //     Note, you are not able to modify the HTTP request headers in a Microsoft.Web.WebView2.Core.CoreWebView2.NavigationStarting
    //     event.
    public Dictionary<string, string> RequestHeaders
    {
        get; set;
    } = new();

    //
    // Summary:
    //     Determines whether to cancel the navigation.
    //
    // Remarks:
    //     If set to true, the navigation is no longer present and the content of the current
    //     page is intact. For performance reasons, GET HTTP requests may happen, while
    //     the host is responding. You may set cookies and use part of a request for the
    //     navigation. Cancellation for navigation to about:blank or frame navigation to
    //     srcdoc is not supported and ignored.
    public bool Cancel
    {
        get; set;
    }

    //
    // Summary:
    //     Gets the ID of the navigation.
    public ulong NavigationId
    {
        get; set;
    }

    //
    // Summary:
    //     Additional allowed frame ancestors set by the host app.
    //
    // Remarks:
    //     The app may set this property to allow a frame to be embedded by additional ancestors
    //     besides what is allowed by http header [X-Frame-Options](https://developer.mozilla.org/docs/Web/HTTP/Headers/X-Frame-Options)
    //     and [Content-Security-Policy frame-ancestors directive](https://developer.mozilla.org/docs/Web/HTTP/Headers/Content-Security-Policy/frame-ancestors).
    //     If set, a frame ancestor is allowed if it is allowed by the additional allowed
    //     frame ancestors or original http header from the site. Whether an ancestor is
    //     allowed by the additional allowed frame ancestors is done the same way as if
    //     the site provided it as the source list of the Content-Security-Policy frame-ancestors
    //     directive. For example, if https://example.com and https://www.example.com are
    //     the origins of the top page and intermediate iframes that embed a nested site-embedding
    //     iframe, and you fully trust those origins, you should set this property to https://example.com
    //     https://www.example.com. This property gives the app the ability to use iframe
    //     to embed sites that otherwise could not be embedded in an iframe in trusted app
    //     pages. This could potentially subject the embedded sites to [Clickjacking](https://wikipedia.org/wiki/Clickjacking)
    //     attack from the code running in the embedding web page. Therefore, you should
    //     only set this property with origins of fully trusted embedding page and any intermediate
    //     iframes. Whenever possible, you should use the list of specific origins of the
    //     top and intermediate frames instead of wildcard characters for this property.
    //     This API is to provide limited support for app scenarios that used to be supported
    //     by <webview> element in other solutions like JavaScript UWP apps and Electron.
    //     You should limit the usage of this property to trusted pages, and specific navigation
    //     target url, by checking the Microsoft.Web.WebView2.Core.CoreWebView2.Source,
    //     and Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs.Uri.
    //     This property is ignored for top level document navigation.
    public string AdditionalAllowedFrameAncestors
    {
        get; set;
    } = string.Empty;
}
