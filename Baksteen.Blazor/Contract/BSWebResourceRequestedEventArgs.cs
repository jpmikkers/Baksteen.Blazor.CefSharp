using Microsoft.Web.WebView2.Core;
using System;

namespace Baksteen.Blazor.Contract;

//
// Summary:
//     Event args for the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
//     event.
public class BSWebResourceRequestedEventArgs : EventArgs
{
    //
    // Summary:
    //     Gets the web resource request.
    //
    // Remarks:
    //     The request object may be missing some headers that are added by network stack
    //     at a later time.
    public BSWebResourceRequest Request
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets or sets the Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponse
    //     object.
    //
    // Remarks:
    //     If this object is set, the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event will be completed with this Response. An empty Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponse
    //     object can be created with Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateWebResourceResponse(System.IO.Stream,System.Int32,System.String,System.String)
    //     and then modified to construct the Response.
    public BSWebResourceResponse Response
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets the web resource request context.
    public CoreWebView2WebResourceContext ResourceContext
    {
        get; set;
    }
}
