using Baksteen.Blazor.CefSharpWPF;
using Microsoft.Extensions.DependencyInjection;

namespace DemoAppWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; }

    public App(IServiceProvider services) : base()
    {
        Services = services;

        //add an global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
        {
            if (eventArgs?.ExceptionObject is Exception exception)
            {
                GlobalExceptionHandler.ProcessException(sender, exception);
            }
        };

        DispatcherUnhandledException += (sender, eventArgs) =>
        {
            GlobalExceptionHandler.ProcessException(sender, eventArgs.Exception);
            eventArgs.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
        {
            GlobalExceptionHandler.ProcessException(sender, eventArgs.Exception);
            eventArgs.SetObserved();
        };
    }
}
