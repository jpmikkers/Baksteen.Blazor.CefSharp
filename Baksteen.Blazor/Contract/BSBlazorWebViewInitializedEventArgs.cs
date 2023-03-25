using System;

namespace Baksteen.Blazor.Contract;

/// <summary>
/// Allows configuring the underlying web view after it has been initialized.
/// </summary>
public class BSBlazorWebViewInitializedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="IBSWebView"/> instance that was initialized.
    /// </summary>
    public IBSWebView WebView { get; internal set; } = default!;
}
