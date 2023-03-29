namespace CefSharpWinFormsBlazorApp;

using Baksteen.Blazor.CefSharpWinForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CefSharpWinFormsBlazorApp.Data;

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
        appBuilder.Services.AddCefSharpBlazorWebView();
        appBuilder.Services.AddCefSharpBlazorWebViewDeveloperTools();
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