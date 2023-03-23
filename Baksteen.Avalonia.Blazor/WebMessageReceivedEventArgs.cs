using System;

namespace Baksteen.Avalonia.Blazor;

//
// Summary:
//     Event args for the Microsoft.Web.WebView2.Core.CoreWebView2.WebMessageReceived
//     event.
public class WebMessageReceivedEventArgs : EventArgs
{
    public Uri? Uri { get; set; }
    public string? WebMessage { get; set; }
}
