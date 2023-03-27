
using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.Handler;

namespace Baksteen.Blazor.CefSharpWinForms;

public partial class CefCoreWebView2Adapter
{
    private class CustomRequestHandler : RequestHandler
    {
        private readonly CefCoreWebView2Adapter _parent;
        private readonly CancelingResourceRequestHandler _cancelingResourceRequestHandler;
        private readonly CustomResourceRequestHandler _customResourceRequestHandler;

        public CustomRequestHandler(CefCoreWebView2Adapter parent)
        {
            _parent = parent;
            _cancelingResourceRequestHandler = new();
            _customResourceRequestHandler = new(parent);
            //this.
        }

        private bool MatchesFilter(string url)
        {
            foreach(var filter in _parent._filters)
            {
                var prefix = filter.url.Replace("*", "");
                if(url.StartsWith(prefix)) return true;
            }
            return false;
        }

        protected override bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return base.OnOpenUrlFromTab(chromiumWebBrowser, browser, frame, targetUrl, targetDisposition, userGesture);
        }

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            var navigationStartingEventArgs = new BSNavigationStartingEventArgs {
                Uri = request.Url,
                Cancel = false,
                IsRedirected = false,
                IsUserInitiated = false,
                NavigationId = request.Identifier,
                RequestHeaders = ConvertHeaders(request.Headers)
            };

            _parent.NavigationStarting?.Invoke(_parent, navigationStartingEventArgs);

            if(navigationStartingEventArgs.Cancel)
            {
                return _cancelingResourceRequestHandler;
            }

            //Only intercept specific Url's
            if(MatchesFilter(request.Url))
            {
                disableDefaultHandling = true;
                return _customResourceRequestHandler;
            }

            //Default behaviour, url will be loaded normally.
            return null!;
        }
    }
}
