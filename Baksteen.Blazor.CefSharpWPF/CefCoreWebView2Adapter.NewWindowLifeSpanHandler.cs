using Baksteen.Blazor.Contract;
using CefSharp;

namespace Baksteen.Blazor.CefSharpWPF;

internal partial class CefCoreWebView2Adapter
{
    // thanks to: https://ourcodeworld.com/articles/read/1444/how-to-prevent-target-blank-links-from-opening-in-a-new-window-popups-in-cefsharp
    public class NewWindowLifeSpanHandler : ILifeSpanHandler
    {
        private readonly CefCoreWebView2Adapter _parent;

        public NewWindowLifeSpanHandler(CefCoreWebView2Adapter parent)
        {
            _parent = parent;
        }

        // Load new URL (when clicking a link with target=_blank) in the same frame
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            BSNewWindowRequestedEventArgs args = new()
            {
                Name = targetUrl,
                Uri = targetUrl,
            };

            _parent.NewWindowRequested?.Invoke(_parent, args);

            //if(!args.Handled)
            //{
            //    browser.MainFrame.LoadUrl(targetUrl);
            //}
            newBrowser = null!;
            return true;
        }

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
    }
}
