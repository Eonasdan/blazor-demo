using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using BlazorDemo.Masl;
using BlazorDemo.Masl.MSALClient;
using BlazorDemo.Shared;
using BlazorDemo.Shared.Downstream;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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
            
            var executingAssembly = Assembly.GetExecutingAssembly();

            using var stream = executingAssembly.GetManifestResourceStream("BlazorDemo.Wpf.appsettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream!)
                .Build();
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
            //builder.Logging.AddDebug();
#endif
            
            serviceCollection.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();
            serviceCollection.AddSingleton<IExternalAuthService, ExternalAuthService>();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
        
            serviceCollection.AddAuthorizationCore();
            serviceCollection.AddScoped<AuthenticationStateProvider, ExternalAuthStateProvider>();
            serviceCollection.AddScoped<IAccessTokenProvider, AccessTokenProvider>();
            
            SharedConfigurationBuilder.Service(serviceCollection, configuration);
            
            Resources.Add("services", serviceCollection.BuildServiceProvider());
            
            PlatformConfig.Instance.ParentWindow = new WindowInteropHelper(this).Handle;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            PlatformConfig.Instance.ParentWindow = new WindowInteropHelper(this).Handle;
        }
    }
}