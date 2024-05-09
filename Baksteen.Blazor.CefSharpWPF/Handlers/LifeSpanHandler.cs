using Baksteen.Blazor.CefSharpWPF.Glue;
using CefSharp;

namespace Baksteen.Blazor.CefSharpWPF.Handlers;

internal class LifeSpanHandler(Action<BSNewWindowRequestedEventArgs> onNewWindowRequestedAction) : ILifeSpanHandler
{
    public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
        return true;
    }

    public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
    }

    public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
    {
    }

    public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
    {
        BSNewWindowRequestedEventArgs args = new()
        {
            Name = targetUrl,
            Uri = targetUrl,
        };

        onNewWindowRequestedAction(args);

        if (!args.Handled)
        {
            browser.MainFrame.LoadUrl(targetUrl);
        }
        newBrowser = null!;
        return true;
    }
}
