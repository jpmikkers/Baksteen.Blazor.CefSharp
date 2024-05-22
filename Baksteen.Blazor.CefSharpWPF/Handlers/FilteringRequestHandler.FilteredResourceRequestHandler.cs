namespace Baksteen.Blazor.CefSharpWPF;
using Baksteen.Blazor.CefSharpWPF.Glue;
using Baksteen.Blazor.CefSharpWPF.Tools;
using CefSharp;
using CefSharp.Handler;
using System.Diagnostics;
using System.IO;

internal partial class FilteringRequestHandler
{
    private class FilteredResourceRequestHandler(
        Func<BSWebResourceRequestedEventArgs, Task> onWebResourceRequested) : ResourceRequestHandler
    {
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
                ResourceContext = request.ResourceType,
                Response = null!
            };

            var requestUri = QueryStringHelper.RemovePossibleQueryString(request.Url);

            Debug.WriteLine($"requestUri here is {requestUri}");

            var extension = Path.GetExtension(requestUri.Split('/').Last()).Trim('.');
            var mimeType = extension switch
            {
                "" => ResourceHandler.DefaultMimeType,
                "htm" => ResourceHandler.DefaultMimeType,
                "html" => ResourceHandler.DefaultMimeType,
                _ => Cef.GetMimeType(extension)
            };

            // origally _parent.WebResourceRequested?.Invoke(_parent, webResourceRequestedEventArgs);
            onWebResourceRequested(webResourceRequestedEventArgs).GetAwaiter().GetResult();

            if(webResourceRequestedEventArgs.Response.StatusCode != (int)System.Net.HttpStatusCode.OK)
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
