Author: Jonathan Peterson

# Introduction 
This project was a showcase of the different ways that Blazor can be used. You will need the Maui workload in VS in order to open the Maui project.

The app is a very simple SignalR "chat" application. There's a single channel that all logged in users communicate with through the API project. 

# Projects

* API: This houses a dotnet API project with a single SingalR hub.
* Msal: The B2C authentication process for both Maui and WPF are identical. This project contains some shared resources between them
* Maui: This the new Xamarin cross-platform native project. I've removed the other platforms to keep it simple but it could support Android and other platforms
* Shared: Most of the Razor components live here. The web, maui and WPF projects can all reference these components
* Web: This is a Blazor WebAssembly (WASM) project.
* Wpf: As you might have guessed... This is a WPF project.


# Authentication

I have a demo B2C tenant setup and all projects make use of this. The API project *could* be locked down and check for an auth'd user. The web project was very easy to setup and VS can easily add B2C to a project. The Maui and WPF projects where a pain. I found most of the Masl code on a blog but had to make adjustments to it to actually get it to work.