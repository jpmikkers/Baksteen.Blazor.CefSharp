// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Baksteen.Avalonia.Blazor.Contract
{
    /// <summary>
    /// Describes a root component that can be added to a <see cref="BlazorWebView"/>.
    /// </summary>
    public class BSRootComponent
    {
        /// <summary>
        /// Constructs an instance of <see cref="BSRootComponent"/>.
        /// </summary>
        /// <param name="selector">The CSS selector string that specifies where in the document the component should be placed. This must be unique among the root components within the <see cref="BlazorWebView"/>.</param>
        /// <param name="componentType">The type of the root component. This type must implement <see cref="IComponent"/>.</param>
        /// <param name="parameters">An optional dictionary of parameters to pass to the root component.</param>
        public BSRootComponent(string selector, Type componentType, IDictionary<string, object?>? parameters)
        {
            if (string.IsNullOrWhiteSpace(selector))
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

        internal Task AddToWebViewManagerAsync(Microsoft.AspNetCore.Components.WebView.WebViewManager webViewManager)
        {
            var parameterView = Parameters == null ? ParameterView.Empty : ParameterView.FromDictionary(Parameters);
            return webViewManager.AddRootComponentAsync(ComponentType, Selector, parameterView);
        }

        internal Task RemoveFromWebViewManagerAsync(Microsoft.AspNetCore.Components.WebView.WebViewManager webviewManager)
        {
            return webviewManager.RemoveRootComponentAsync(Selector);
        }
    }
}
