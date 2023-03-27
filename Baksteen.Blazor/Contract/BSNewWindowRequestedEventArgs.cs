using System;

namespace Baksteen.Blazor.Contract;

//
// Summary:
//     Event args for the Microsoft.Web.WebView2.Core.CoreWebView2.NewWindowRequested
//     event.
public class BSNewWindowRequestedEventArgs : EventArgs
{
    //
    // Summary:
    //     Gets the target uri of the new window request.
    public string Uri
    {
        get; set;
    }

    //
    // Summary:
    //     Gets the new window or sets a WebView as a result of the new window requested.
    //
    // Remarks:
    //     Provides a WebView as the target for a window.open() from inside the requesting
    //     WebView. If this is set, the top-level window returns as the opened WindowProxy.
    //     If this is not set, then Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs.Handled
    //     is checked to determine behavior for the Microsoft.Web.WebView2.Core.CoreWebView2.NewWindowRequested.
    //     WebView provided in the NewWindow property must be on the same Microsoft.Web.WebView2.Core.CoreWebView2Environment
    //     as the opener WebView and cannot be navigated. Changes to settings should be
    //     made before setting NewWindow to ensure that those settings take effect for the
    //     newly setup WebView. The new WebView must have the same profile as the opener
    //     WebView.
    //public CoreWebView2 NewWindow
    //{
    //    get; set;
    //}

    //
    // Summary:
    //     Indicates whether the Microsoft.Web.WebView2.Core.CoreWebView2.NewWindowRequested
    //     event is handled by host.
    //
    // Remarks:
    //     If this is false and no Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs.NewWindow
    //     is set, the WebView opens a popup window and it returns as opened WindowProxy.
    //     If set to true and no Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs.NewWindow
    //     is set for window.open(), the opened WindowProxy is for a dummy window object
    //     and no window loads. The default value is false.
    public bool Handled
    {
        get; set;
    }

    //
    // Summary:
    //     true when the new window request was initiated through a user gesture such as
    //     selecting an anchor tag with target.
    //
    // Remarks:
    //     The Microsoft Edge popup blocker is disabled for WebView so the app is able to
    //     use this flag to block non-user initiated popups.
    public bool IsUserInitiated
    {
        get; set;
    }

    //
    // Summary:
    //     Gets the window features specified by the window.open() call. These features
    //     should be considered for positioning and sizing of new WebView windows.
    // public CoreWebView2WindowFeatures WindowFeatures{get;set;}

    //
    // Summary:
    //     Gets the name of the new window.
    //
    // Remarks:
    //     This window can be created via window.open(url, windowName), where the windowName
    //     parameter corresponds to Name property. If no windowName is passed to window.open,
    //     then the Name property will be set to an empty string. Additionally, if window
    //     is opened through other means, such as <a target="windowName"> or <iframe name="windowName">,
    //     then the Name property will be set accordingly. In the case of target=_blank,
    //     the Name property will be an empty string. Opening a window via Ctrl+clicking
    //     a link would result in the Name property being set to an empty string.
    public string Name
    {
        get; set;
    }
}
