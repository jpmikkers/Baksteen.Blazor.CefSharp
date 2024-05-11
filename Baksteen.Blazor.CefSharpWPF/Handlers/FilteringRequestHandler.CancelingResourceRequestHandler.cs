namespace Baksteen.Blazor.CefSharpWPF;
using CefSharp;
using CefSharp.Handler;

internal partial class FilteringRequestHandler
{
    private class CancelingResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }
}
