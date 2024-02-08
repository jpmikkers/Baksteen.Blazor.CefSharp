namespace Baksteen.Blazor.CefSharpWinForms;

using Baksteen.Blazor;
using Baksteen.Blazor.Contract;
using CefSharp;
using CefSharp.Handler;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            var resourceType = request.ResourceType switch
            {
                ResourceType.Xhr => CoreWebView2WebResourceContext.Fetch,
                ResourceType.FontResource => CoreWebView2WebResourceContext.Font,
                _ => Enum.TryParse<CoreWebView2WebResourceContext>(request.ResourceType.ToString(), out var type) ? type : CoreWebView2WebResourceContext.Document
            };

            var webResourceRequestedEventArgs = new BSWebResourceRequestedEventArgs
            {
                Request = new BSWebResourceRequest
                {
                    Uri = request.Url,
                    Content = null!,
                    Headers = ConvertHeaders(request.Headers),
                    Method = request.Method,
                },
                ResourceContext = resourceType,
                Response = null!
            };

            var requestUri = BSQueryStringHelper.RemovePossibleQueryString(request.Url);
            Debug.WriteLine($"requestUri here is {requestUri}");

            var extension = Path.GetExtension(requestUri.Split('/').Last()).Trim('.');
            var mimeType = extension switch
            {
                "" => ResourceHandler.DefaultMimeType,
                "htm" => ResourceHandler.DefaultMimeType,
                "html" => ResourceHandler.DefaultMimeType,
                _ => Cef.GetMimeType(extension)
            };

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
