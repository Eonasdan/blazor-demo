using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlazorDemo.Masl;
using BlazorDemo.Models.DownstreamApi;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDemo.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();
            
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
            //builder.Logging.AddDebug();
#endif
            
            var executingAssembly = Assembly.GetExecutingAssembly();

            using var stream = executingAssembly.GetManifestResourceStream("BlazorDemo.Wpf.appsettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            serviceCollection.AddSingleton<IExternalAuthService, ExternalAuthService>();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
        
            serviceCollection.AddAuthorizationCore();
            serviceCollection.AddScoped<AuthenticationStateProvider, ExternalAuthStateProvider>();
            serviceCollection.AddSingleton<ExternalAuthService>();
            
            
            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}