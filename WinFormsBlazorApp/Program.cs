namespace WinFormsBlazorApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.PortableExecutable;
using WinFormsBlazorApp.Data;
using CefSharp.WinForms;
using CefSharp;
using Baksteen.Blazor;
using Baksteen.Blazor.Contract;
using Baksteen.Blazor.WinForms;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        var appBuilder = Host.CreateApplicationBuilder(args);
        appBuilder.Logging.AddDebug();
        appBuilder.Services.AddAvaloniaBlazorWebView();                 // this wraps: appBuilder.Services.AddWindowsFormsBlazorWebView();
        appBuilder.Services.AddAvaloniaBlazorWebViewDeveloperTools();   // this wraps: appBuilder.Services.AddBlazorWebViewDeveloperTools();
        appBuilder.Services.AddSingleton<WeatherForecastService>();
        using var myApp = appBuilder.Build();

        myApp.Start();

        // CefSharp init taken from: https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.Wpf/Program.PublishSingleFile.cs

        //Cef.EnableHighDPISupport();

        var exitCode = CefSharp.BrowserSubprocess.SelfHost.Main(args);

        if(exitCode >= 0)
        {
            return;
        }

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1(myApp.Services));

        Task.Run(async () => await myApp.StopAsync()).Wait();
    }
}