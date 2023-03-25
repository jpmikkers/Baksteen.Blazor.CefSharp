using System;

namespace Baksteen.Blazor.Contract;

//
// Summary:
//     Event args for the Microsoft.Web.WebView2.Core.CoreWebView2.WebMessageReceived
//     event.
public class BSWebMessageReceivedEventArgs : EventArgs
{
    public Uri? Uri { get; set; }
    public string? WebMessage { get; set; }
}
