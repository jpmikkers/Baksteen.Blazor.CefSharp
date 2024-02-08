
using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.Handler;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;

namespace Baksteen.Blazor.CefSharpWinForms;

internal partial class CefCoreWebView2Adapter
{
    private class FilteringRequestHandler : RequestHandler
    {
        private readonly CefCoreWebView2Adapter _parent;
        private readonly CancelingResourceRequestHandler _cancelingResourceRequestHandler;
        private readonly FilteredResourceRequestHandler _filteredResourceRequestHandler;
        private readonly List<(string url, CoreWebView2WebResourceContext context)> _filters = new();

        public FilteringRequestHandler(CefCoreWebView2Adapter parent)
        {
            _parent = parent;
            _cancelingResourceRequestHandler = new();
            _filteredResourceRequestHandler = new(parent);
        }

        public void AddWebResourceRequestedFilter(string uri, CoreWebView2WebResourceContext ResourceContext)
        {
            _filters.Add((uri, ResourceContext));
        }

        private bool MatchesFilter(string url)
        {
            foreach (var filter in _filters)
            {
                var prefix = filter.url.Replace("*", "");
                if (url.StartsWith(prefix)) return true;
            }
            return false;
        }

        protected override IResourceRequestHandler? GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            var navigationStartingEventArgs = new BSNavigationStartingEventArgs
            {
                Uri = request.Url,
                Cancel = false,
                IsRedirected = false,
                IsUserInitiated = false,
                NavigationId = request.Identifier,
                RequestHeaders = ConvertHeaders(request.Headers)
            };

            if (isNavigation)
            {
                _parent.NavigationStarting?.Invoke(_parent, navigationStartingEventArgs);
                if (navigationStartingEventArgs.Cancel)
                    return _cancelingResourceRequestHandler;
            }

            //Only intercept specific Url's
            if (MatchesFilter(request.Url))
            {
                disableDefaultHandling = true;
                return _filteredResourceRequestHandler;
            }

            //Default behaviour, url will be loaded normally.
            return null;
        }
    }
}
