using Baksteen.Blazor.CefSharpWPF.Glue;
using System.Windows;

namespace DemoAppWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public IServiceProvider Services { get; set; }
    public MainWindow()
    {
        DataContext = this;
        Services = (Application.Current as App)!.Services!;

        InitializeComponent();
    }
}