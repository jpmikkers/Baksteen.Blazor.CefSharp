using Baksteen.Blazor.CefSharpWPF.Glue;
using System.Windows;

namespace DemoAppWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IServiceProvider Services { get; set; }
        public BSRootComponentsCollection RootComponents { get; set; } = new BSRootComponentsCollection();

        public MainWindow()
        {
            DataContext = this;
            Services = (Application.Current as App)!.Services!;
            //InitializeComponent();
            RootComponents.Add<BlazorLayout>("#app");

            //RootComponents =
            //[
            //new BSRootComponent("#app", typeof(BlazorLayout), null)
            //];
        }
    }
}