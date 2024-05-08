using System.Collections.Generic;
using System.IO;

namespace Baksteen.Blazor.CefSharpWPF.Glue;

//
// Summary:
//     An HTTP request used with the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
//     event.
public class BSWebResourceRequest
{
    //
    // Summary:
    //     Gets or sets the request URI.
    public string Uri
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets or sets the HTTP request method.
    public string Method
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets or sets the HTTP request message body as stream.
    //
    // Remarks:
    //     POST data should be here. If a stream is set, which overrides the message body,
    //     the stream must have all the content data available by the time the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event deferral of this request is completed. Stream should be agile or be created
    //     from a background STA to prevent performance impact to the UI thread. null means
    //     no content data.
    public Stream Content
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets the mutable HTTP request headers.
    public Dictionary<string, string> Headers
    {
        get; set;
    } = default!;
}
