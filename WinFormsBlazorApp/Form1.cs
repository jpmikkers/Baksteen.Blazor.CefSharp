namespace CefSharpWinFormsBlazorApp;
using Microsoft.Extensions.DependencyInjection;
using Baksteen.Blazor;
using Baksteen.Blazor.Contract;

public partial class Form1 : Form
{
    public Form1(IServiceProvider services)
    {
        InitializeComponent();

        blazorCefWebView1.Services = services;
        blazorCefWebView1.RootComponents.Add<Main>("#app");
        blazorCefWebView1.HostPage = "index.html";
    }
}
