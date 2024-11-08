using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Baksteen.Blazor.CefSharpWPF;
using CefSharp.BrowserSubprocess;

namespace DemoAppWpf
{
    public static class Program
    {
        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "8.0.1.0")]
        public static void Main(string[] args)
        {
            var appBuilder = Host.CreateApplicationBuilder(args);
            appBuilder.Logging.AddDebug();
            appBuilder.Services.AddCefSharpBlazorWebView();
            appBuilder.Services.AddCefSharpBlazorWebViewDeveloperTools();
            using var myApp = appBuilder.Build();

            myApp.Start();

            // CefSharp init taken from: https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.Wpf/Program.PublishSingleFile.cs

            //Cef.EnableHighDPISupport();

            var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(args);

            if(exitCode >= 0)
            {
                return;
            }

            App app = new App(myApp.Services);
            app.InitializeComponent();
            app.Run();

            Task.Run(async () => await myApp.StopAsync()).Wait();
        }
    }
}
