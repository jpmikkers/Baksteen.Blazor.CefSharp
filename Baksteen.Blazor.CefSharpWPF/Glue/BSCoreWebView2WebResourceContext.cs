namespace Baksteen.Blazor.CefSharpWPF.Glue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
// Summary:
//     Specifies the web resource request contexts.
public enum BSCoreWebView2WebResourceContext
{
    //
    // Summary:
    //     Specifies all resources.
    All,
    //
    // Summary:
    //     Specifies a document resources.
    Document,
    //
    // Summary:
    //     Specifies a CSS resources.
    Stylesheet,
    //
    // Summary:
    //     Specifies an image resources.
    Image,
    //
    // Summary:
    //     Specifies another media resource such as a video.
    Media,
    //
    // Summary:
    //     Specifies a font resource.
    Font,
    //
    // Summary:
    //     Specifies a script resource.
    Script,
    //
    // Summary:
    //     Specifies an XML HTTP request, Fetch and EventSource HTTP communication.
    XmlHttpRequest,
    //
    // Summary:
    //     Specifies a Fetch API communication.
    Fetch,
    //
    // Summary:
    //     Specifies a TextTrack resource.
    TextTrack,
    //
    // Summary:
    //     Specifies an EventSource API communication.
    EventSource,
    //
    // Summary:
    //     Specifies a WebSocket API communication.
    Websocket,
    //
    // Summary:
    //     Specifies a Web App Manifest.
    Manifest,
    //
    // Summary:
    //     Specifies a Signed HTTP Exchange.
    SignedExchange,
    //
    // Summary:
    //     Specifies a Ping request.
    Ping,
    //
    // Summary:
    //     Specifies a CSP Violation Report.
    CspViolationReport,
    //
    // Summary:
    //     Specifies an other resource.
    Other
}
