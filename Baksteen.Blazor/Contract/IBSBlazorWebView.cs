// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using BaksteenBlazorWebViewInitializingEventArgs = Baksteen.Blazor.Contract.BSBlazorWebViewInitializingEventArgs;
using BaksteenBlazorWebViewInitializedEventArgs = Baksteen.Blazor.Contract.BSBlazorWebViewInitializedEventArgs;
using BaksteenUrlLoadingEventArgs = Baksteen.Blazor.Contract.BSUrlLoadingEventArgs;
using BaksteenRootComponent = Baksteen.Blazor.Contract.BSRootComponent;

namespace Baksteen.Blazor.Contract;

public interface IBSBlazorWebView : IDisposable
{
    /// <summary>
    /// returns a platform specific object that can be used to retrieve a platform specific handle, or the handle itself
    /// For now this is the convention:
    /// Windows & Winforms -> this should return a System.Windows.Forms.Control
    /// </summary>
    public object PlatformSpecificComponent { get; }

    string? HostPage { get; set; }

    // AddRootComponents and JSComponents replace this one, for now:
    // RootComponentsCollection RootComponents { get; }

    void AddRootComponents(IEnumerable<BaksteenRootComponent> rootComponents);
    JSComponentConfigurationStore JSComponents { get; }

    IServiceProvider Services { get; set; }
    IBSWebView WebView { get; }

    IFileProvider CreateFileProvider(string contentRootDir);

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    public EventHandler<BaksteenUrlLoadingEventArgs>? UrlLoading { get; set; }

    /// <summary>
    /// Allows customizing the web view before it is created.
    /// </summary>
    public EventHandler<BaksteenBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing { get; set; }

    /// <summary>
    /// Allows customizing the web view after it is created.
    /// </summary>
    public EventHandler<BaksteenBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized { get; set; }
}
