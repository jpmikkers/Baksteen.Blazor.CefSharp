﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Baksteen.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaksteenAutoCloseOnReadCompleteStream = Baksteen.AspNetCore.Components.WebView.WebView2.AutoCloseOnReadCompleteStream;
using BaksteenAvaloniaBlazorMarkerService = Baksteen.Blazor.BaksteenAvaloniaBlazorMarkerService;
using BaksteenBlazorWebViewDeveloperTools = Baksteen.AspNetCore.Components.WebView.WindowsForms.BlazorWebViewDeveloperTools;
using BaksteenBlazorWebViewInitializedEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs;
using BaksteenBlazorWebViewInitializingEventArgs = Baksteen.AspNetCore.Components.WebView.BlazorWebViewInitializingEventArgs;
using BaksteenStaticContentHotReloadManager = Baksteen.AspNetCore.Components.WebView.StaticContentHotReloadManager;
using BaksteenUrlLoadingEventArgs = Baksteen.AspNetCore.Components.WebView.UrlLoadingEventArgs;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;

namespace Baksteen.AspNetCore.Components.WebView.WebView2
{
    /// <summary>
    /// An implementation of <see cref="WebViewManager"/> that uses the Edge WebView2 browser control
    /// to render web content.
    /// </summary>
    internal class WebView2WebViewManager : WebViewManager
    {
        // Using an IP address means that WebView2 doesn't wait for any DNS resolution,
        // making it substantially faster. Note that this isn't real HTTP traffic, since
        // we intercept all the requests within this origin.
        internal static readonly string AppHostAddress = "0.0.0.0";

        /// <summary>
        /// Gets the application's base URI. Defaults to <c>https://0.0.0.0/</c>
        /// </summary>
        protected static readonly string AppOrigin = $"https://{AppHostAddress}/";

        internal static readonly Uri AppOriginUri = new(AppOrigin);

        private readonly WebView2Control _webview;
        private readonly Task<bool> _webviewReadyTask;
        private readonly string _contentRootRelativeToAppRoot;

		private protected CoreWebView2Environment? _coreWebView2Environment;
		private readonly Action<BaksteenUrlLoadingEventArgs> _urlLoading;
		private readonly Action<BaksteenBlazorWebViewInitializingEventArgs> _blazorWebViewInitializing;
		private readonly Action<BaksteenBlazorWebViewInitializedEventArgs> _blazorWebViewInitialized;
		private readonly BaksteenBlazorWebViewDeveloperTools _developerTools;

        /// <summary>
        /// Constructs an instance of <see cref="Microsoft.AspNetCore.Components.WebView.WebView2.WebView2WebViewManager"/>.
        /// </summary>
        /// <param name="webview">A <see cref="WebView2Control"/> to access platform-specific WebView2 APIs.</param>
        /// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
        /// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
        /// <param name="fileProvider">Provides static content to the webview.</param>
        /// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
        /// <param name="contentRootRelativeToAppRoot">Path to the app's content root relative to the application root directory.</param>
        /// <param name="hostPagePathWithinFileProvider">Path to the host page within the <paramref name="fileProvider"/>.</param>
        /// <param name="urlLoading">Callback invoked when a url is about to load.</param>
        /// <param name="blazorWebViewInitializing">Callback invoked before the webview is initialized.</param>
        /// <param name="blazorWebViewInitialized">Callback invoked after the webview is initialized.</param>
        internal WebView2WebViewManager(
			WebView2Control webview,
			IServiceProvider services,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPagePathWithinFileProvider,
			Action<BaksteenUrlLoadingEventArgs> urlLoading,
			Action<BaksteenBlazorWebViewInitializingEventArgs> blazorWebViewInitializing,
			Action<BaksteenBlazorWebViewInitializedEventArgs> blazorWebViewInitialized)
			: base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPagePathWithinFileProvider)

		{
			ArgumentNullException.ThrowIfNull(webview);

			if (services.GetService<BaksteenAvaloniaBlazorMarkerService>() is null)
			{
				throw new InvalidOperationException(
					"Unable to find the required services. " +
					$"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BlazorWebViewServiceCollectionExtensions.AddWindowsFormsBlazorWebView)}' in the application startup code.");
			}

			_webview = webview;
			_urlLoading = urlLoading;
			_blazorWebViewInitializing = blazorWebViewInitializing;
			_blazorWebViewInitialized = blazorWebViewInitialized;
			_developerTools = services.GetRequiredService<BaksteenBlazorWebViewDeveloperTools>();
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

			// Unfortunately the CoreWebView2 can only be instantiated asynchronously.
			// We want the external API to behave as if initalization is synchronous,
			// so keep track of a task we can await during LoadUri.
			_webviewReadyTask = TryInitializeWebView2();
		}

        /// <inheritdoc />
        protected override void NavigateCore(Uri absoluteUri)
        {
            _ = Dispatcher.InvokeAsync(async () =>
            {
                var isWebviewInitialized = await _webviewReadyTask;

                if(isWebviewInitialized)
                {
                    _webview.Source = absoluteUri;
                }
            });
        }

        /// <inheritdoc />
        protected override void SendMessage(string message)
            => _webview.CoreWebView2.PostWebMessageAsString(message);

        private async Task<bool> TryInitializeWebView2()
        {
            var args = new BaksteenBlazorWebViewInitializingEventArgs();

			_blazorWebViewInitializing?.Invoke(args);
			var userDataFolder = args.UserDataFolder ?? GetWebView2UserDataFolder();
			_coreWebView2Environment = await CoreWebView2Environment.CreateAsync(
				browserExecutableFolder: args.BrowserExecutableFolder,
				userDataFolder: userDataFolder,
				options: args.EnvironmentOptions)
				.ConfigureAwait(true);

			await _webview.EnsureCoreWebView2Async(_coreWebView2Environment);

			var developerTools = _developerTools;

            ApplyDefaultWebViewSettings(developerTools);

			_blazorWebViewInitialized?.Invoke(new BaksteenBlazorWebViewInitializedEventArgs
			{
				WebView = new WinFormsWebViewProxy(_webview),
			});

            _webview.CoreWebView2.AddWebResourceRequestedFilter($"{AppOrigin}*", CoreWebView2WebResourceContext.All);

            _webview.CoreWebView2.WebResourceRequested += async (s, eventArgs) =>
            {
                await HandleWebResourceRequest(eventArgs);
            };

            _webview.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            _webview.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            // The code inside blazor.webview.js is meant to be agnostic to specific webview technologies,
            // so the following is an adaptor from blazor.webview.js conventions to WebView2 APIs
            await _webview.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
				window.external = {
					sendMessage: message => {
						window.chrome.webview.postMessage(message);
					},
					receiveMessage: callback => {
						window.chrome.webview.addEventListener('message', e => callback(e.data));
					}
				};
			")
             .ConfigureAwait(true);

            QueueBlazorStart();

            _webview.CoreWebView2.WebMessageReceived += (s, e) => MessageReceived(new Uri(e.Source), e.TryGetWebMessageAsString());

            return true;
        }

        /// <summary>
        /// Handles outbound URL requests.
        /// </summary>
        /// <param name="eventArgs">The <see cref="CoreWebView2WebResourceRequestedEventArgs"/>.</param>
        protected virtual Task HandleWebResourceRequest(CoreWebView2WebResourceRequestedEventArgs eventArgs)
        {
			// Unlike server-side code, we get told exactly why the browser is making the request,
			// so we can be smarter about fallback. We can ensure that 'fetch' requests never result
			// in fallback, for example.
			var allowFallbackOnHostPage =
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Document ||
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Other; // e.g., dev tools requesting page source

			var requestUri = Baksteen.AspNetCore.Components.WebView.QueryStringHelper.RemovePossibleQueryString(eventArgs.Request.Uri);

			if (TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
			{
				BaksteenStaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, requestUri, ref statusCode, ref content, headers);

				var headerString = GetHeaderString(headers);

				var autoCloseStream = new BaksteenAutoCloseOnReadCompleteStream(content);

				eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(autoCloseStream, statusCode, statusMessage, headerString);
			}
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to queue a call to Blazor.start(). Not all platforms require this.
        /// </summary>
        protected virtual void QueueBlazorStart()
        {
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if(Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
            {
				var callbackArgs = BaksteenUrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, AppOriginUri);

				_urlLoading?.Invoke(callbackArgs);

                if(callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
                {
                    LaunchUriInExternalBrowser(uri);
                }

                args.Cancel = callbackArgs.UrlLoadingStrategy != UrlLoadingStrategy.OpenInWebView;
            }
        }

        private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            // Intercept _blank target <a> tags to always open in device browser.
            // The ExternalLinkCallback is not invoked.
            if(Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
            {
                LaunchUriInExternalBrowser(uri);
                args.Handled = true;
            }
        }

        private void LaunchUriInExternalBrowser(Uri uri)
        {
			using (var launchBrowser = new Process())
			{
				launchBrowser.StartInfo.UseShellExecute = true;
				launchBrowser.StartInfo.FileName = uri.ToString();
				launchBrowser.Start();
			}
        }

        private protected static string GetHeaderString(IDictionary<string, string> headers) =>
            string.Join(Environment.NewLine, headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

        private void ApplyDefaultWebViewSettings(BaksteenBlazorWebViewDeveloperTools devTools)
        {
            _webview.CoreWebView2.Settings.AreDevToolsEnabled = devTools.Enabled;

            // Desktop applications typically don't want the default web browser context menu
            _webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

            // Desktop applications almost never want to show a URL preview when hovering over a link
            _webview.CoreWebView2.Settings.IsStatusBarEnabled = false;
        }

		private static string? GetWebView2UserDataFolder()
		{
			if (Assembly.GetEntryAssembly() is { } mainAssembly)
			{
				// In case the application is running from a non-writable location (e.g., program files if you're not running
				// elevated), use our own convention of %LocalAppData%\YourApplicationName.WebView2.
				// We may be able to remove this if https://github.com/MicrosoftEdge/WebView2Feedback/issues/297 is fixed.
				var applicationName = mainAssembly.GetName().Name;
				var result = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					$"{applicationName}.WebView2");

				return result;
			}

			return null;
		}
    }
}
