namespace Baksteen.Blazor.CefSharpWPF;

using Baksteen.Blazor;
using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.Handler;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

internal partial class CefCoreWebView2Adapter
{
    private class FilteredResourceRequestHandler : ResourceRequestHandler
    {
        private readonly CefCoreWebView2Adapter _parent;

        public FilteredResourceRequestHandler(CefCoreWebView2Adapter parent)
        {
            _parent = parent;
        }

        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            var webResourceRequestedEventArgs = new BSWebResourceRequestedEventArgs
            {
                Request = new BSWebResourceRequest
                {
                    Uri = request.Url,
                    Content = null!,
                    Headers = ConvertHeaders(request.Headers),
                    Method = request.Method,
                },
                ResourceContext = CoreWebView2WebResourceContext.Document,
                Response = null!
            };

            var requestUri = BSQueryStringHelper.RemovePossibleQueryString(request.Url);
            var mimeType = ResourceHandler.DefaultMimeType;

            Debug.WriteLine($"requestUri here is {requestUri}");

            if (requestUri.EndsWith(".html"))
            {
                mimeType = Cef.GetMimeType("html");
            }

            if (requestUri.EndsWith(".css"))
            {
                mimeType = Cef.GetMimeType("css");
                webResourceRequestedEventArgs.ResourceContext = CoreWebView2WebResourceContext.Stylesheet;
            }

            if (requestUri.EndsWith(".js"))
            {
                mimeType = Cef.GetMimeType("js");
                webResourceRequestedEventArgs.ResourceContext = CoreWebView2WebResourceContext.Script;
            }

            if (requestUri.EndsWith(".json"))
            {
                mimeType = Cef.GetMimeType("json");
                webResourceRequestedEventArgs.ResourceContext = CoreWebView2WebResourceContext.Fetch;
            }

            _parent.WebResourceRequested?.Invoke(_parent, webResourceRequestedEventArgs);

            if (webResourceRequestedEventArgs.Response.StatusCode != (int)System.Net.HttpStatusCode.OK)
            {
                return ResourceHandler.ForErrorMessage(webResourceRequestedEventArgs.Response.ReasonPhrase, (System.Net.HttpStatusCode)webResourceRequestedEventArgs.Response.StatusCode);
            }

            var rs = ResourceHandler.FromStream(webResourceRequestedEventArgs.Response.Content, mimeType);
            //rs.StatusCode = webResourceRequestedEventArgs.Response.StatusCode;
            //rs.StatusText = webResourceRequestedEventArgs.Response.ReasonPhrase;
            //rs.MimeType = mimeType;
            return rs;

            //ResourceHandler has many static methods for dealing with Streams, 
            // byte[], files on disk, strings
            // Alternatively ou can inheir from IResourceHandler and implement
            // a custom behaviour that suites your requirements.
            //return ResourceHandler.FromString("Welcome to CefSharp!", mimeType: Cef.GetMimeType("html"));
        }
    }
}
