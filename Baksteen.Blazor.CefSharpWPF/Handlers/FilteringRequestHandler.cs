using Baksteen.Blazor.CefSharpWPF.Glue;
using CefSharp;
using CefSharp.Handler;
using System.Collections.Specialized;

namespace Baksteen.Blazor.CefSharpWPF;

internal partial class FilteringRequestHandler : RequestHandler
{
    private static Dictionary<string, string> ConvertHeaders(NameValueCollection nvc)
    {
        return nvc.AllKeys.Where(k => k != null).ToDictionary(k => k!, k => nvc[k]!);
    }

    private readonly CancelingResourceRequestHandler _cancelingResourceRequestHandler;
    private readonly FilteredResourceRequestHandler _filteredResourceRequestHandler;
    private readonly List<(string url, BSCoreWebView2WebResourceContext context)> _filters = new();
    private readonly Action<BSNavigationStartingEventArgs> _onNavigationStarting;

    public FilteringRequestHandler(
        Action<BSNavigationStartingEventArgs> onNavigationStarting,
        Func<BSWebResourceRequestedEventArgs,Task> onWebResourceRequested)
    {
        _cancelingResourceRequestHandler = new();
        _filteredResourceRequestHandler = new(onWebResourceRequested);
        _onNavigationStarting = onNavigationStarting;
    }

    public void AddWebResourceRequestedFilter(string uri, BSCoreWebView2WebResourceContext ResourceContext)
    {
        _filters.Add((uri, ResourceContext));
    }

    private bool MatchesFilter(string url)
    {
        foreach(var filter in _filters)
        {
            var prefix = filter.url.Replace("*", "");
            if(url.StartsWith(prefix)) return true;
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

        if(isNavigation)
        {
            // this will ultimately result in an urlLoading event where the user can modify how the url is handled
            _onNavigationStarting(navigationStartingEventArgs);
    
            if(navigationStartingEventArgs.Cancel)
                return _cancelingResourceRequestHandler;
        }

        //Only intercept specific Url's
        if(MatchesFilter(request.Url))
        {
            disableDefaultHandling = true;
            return _filteredResourceRequestHandler;
        }

        //Default behaviour, url will be loaded normally.
        return null;
    }
}
