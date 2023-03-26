using CefSharp;
using CefSharp.Handler;

namespace Baksteen.Blazor.CefSharpWinForms;

public partial class CefCoreWebView2Adapter
{
    public class CancelingResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }
}
