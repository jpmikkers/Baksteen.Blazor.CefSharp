using System.Collections.Generic;
using System.IO;

namespace Baksteen.Blazor.Contract;

//
// Summary:
//     An HTTP response used with the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
//     event.
public class BSWebResourceResponse
{
    //
    // Summary:
    //     Gets HTTP response content as stream.
    //
    // Remarks:
    //     Stream must have all the content data available by the time the Microsoft.Web.WebView2.Core.CoreWebView2.WebResourceRequested
    //     event deferral of this response is completed. Stream should be agile or be created
    //     from a background thread to prevent performance impact to the UI thread. null
    //     means no content data.
    public Stream Content
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets the overridden HTTP response headers.
    public Dictionary<string, string> Headers
    {
        get; set;
    } = default!;

    //
    // Summary:
    //     Gets or sets the HTTP response status code.
    public int StatusCode
    {
        get; set;
    }

    //
    // Summary:
    //     Gets or sets the HTTP response reason phrase.
    public string ReasonPhrase
    {
        get; set;
    } = default!;
}
