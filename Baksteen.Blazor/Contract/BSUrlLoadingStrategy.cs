namespace Baksteen.Blazor.Contract;

public enum BSUrlLoadingStrategy
{
    OpenExternally, 	// 0 Allows loading URLs using an app determined by the system. This is the default strategy for URLs with an external host.
    OpenInWebView, 	    // 1 Allows loading URLs within the Blazor WebView. This is the default strategy for URLs with a host matching the ap
    CancelLoad,         // 2 Cancels the current URL loading attempt.
}
