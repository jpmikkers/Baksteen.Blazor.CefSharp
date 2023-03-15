// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#define WEBVIEW2_WINFORMS

#if !WEBVIEW2_WINFORMS && !WEBVIEW2_WPF && !WEBVIEW2_MAUI
#error Must specify which WebView2 is targeted
#endif

#if WINDOWS

#if WEBVIEW2_WINFORMS
//using Microsoft.Web.WebView2.Core;
#elif WEBVIEW2_WPF
using System.Diagnostics;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;
using System.Reflection;
#elif WEBVIEW2_MAUI
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Launcher = Windows.System.Launcher;
#endif

namespace Baksteen.Avalonia.Blazor
{
    internal class BaksteenBlazorWebViewDeveloperTools
    {
        public bool Enabled { get; set; } = false;
    }
}

#endif
