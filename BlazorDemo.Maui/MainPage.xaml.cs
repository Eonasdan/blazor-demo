using Microsoft.Identity.Client;

namespace BlazorDemo.Maui;

public partial class MainPage : ContentPage
{
    public IPublicClientApplication IdentityClient { get; set; }
    
    public MainPage()
    {
        InitializeComponent();
    }
}