// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Avalonia.Platform;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using BaksteenBlazorWebViewInitializingEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializingEventArgs;
using BaksteenBlazorWebViewInitializedEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs;
using BaksteenUrlLoadingEventArgs = Baksteen.AspNetCore.Components.WebView.UrlLoadingEventArgs;

namespace Baksteen.Avalonia.Blazor;

public class ARootComponent
{
    /// <summary>
    /// Constructs an instance of <see cref="RootComponent"/>.
    /// </summary>
    /// <param name="selector">The CSS selector string that specifies where in the document the component should be placed. This must be unique among the root components within the <see cref="BlazorWebView"/>.</param>
    /// <param name="componentType">The type of the root component. This type must implement <see cref="IComponent"/>.</param>
    /// <param name="parameters">An optional dictionary of parameters to pass to the root component.</param>
    public ARootComponent(string selector, Type componentType, IDictionary<string, object?>? parameters)
    {
        if(string.IsNullOrWhiteSpace(selector))
        {
            throw new ArgumentException($"'{nameof(selector)}' cannot be null or whitespace.", nameof(selector));
        }

        Selector = selector;
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the CSS selector string that specifies where in the document the component should be placed.
    /// This must be unique among the root components within the <see cref="BlazorWebView"/>.
    /// </summary>
    public string Selector { get; }

    /// <summary>
    /// Gets the type of the root component. This type must implement <see cref="IComponent"/>.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Gets an optional dictionary of parameters to pass to the root component.
    /// </summary>
    public IDictionary<string, object?>? Parameters { get; }
}


public interface IBlazorWebView : IDisposable
{
    public IPlatformHandle Handle { get; }

    string? HostPage { get; set; }

    // AddRootComponents and JSComponents replace this one, for now:
    // RootComponentsCollection RootComponents { get; }

    void AddRootComponents(IEnumerable<ARootComponent> rootComponents);
    JSComponentConfigurationStore JSComponents { get; }

    IServiceProvider Services { get; set; }
    WebView2 WebView { get; }

    IFileProvider CreateFileProvider(string contentRootDir);

    /// <summary>
    /// Allows customizing how links are opened.
    /// By default, opens internal links in the webview and external links in an external app.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing how links are opened. By default, opens internal links in the webview and external links in an external app.")]
    public EventHandler<BaksteenUrlLoadingEventArgs>? UrlLoading { get; set; }

    /// <summary>
    /// Allows customizing the web view before it is created.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing the web view before it is created.")]
    public EventHandler<BaksteenBlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing { get; set; }

    /// <summary>
    /// Allows customizing the web view after it is created.
    /// </summary>
    [Category("Action")]
    [Description("Allows customizing the web view after it is created.")]
    public EventHandler<BaksteenBlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized { get; set; }
}
