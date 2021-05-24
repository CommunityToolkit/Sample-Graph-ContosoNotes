# Contoso Notes

Contoso Notes is a simple note taking app infused with the power of Microsoft Graph!

Built for the [Enlighten your Windows App with Microsoft Graph](https://aka.ms/OD531) session at Microsoft Build.

In Contoso Notes you can:

- Sign in with your personal Microsoft Account (MSA) to get connected with Microsoft Graph APIs.
- Keep multiple note pages backed up to OneDrive so notes will roam across devices.
- Type the magic word `todo:` to create new tasks that are stored in Microsoft Todo and stay synchronized. 

## Demo Setup

The demo code can be easily built and deployed in Visual Studio 2019. However, for the WindowsProvider to work, you must associate the app with the Microsoft Store.
This creates the trust relationship that enables your app to authenticate and retrieve valid access tokens. No AAD configuration required for consumer MSAs!

Alternatively, if you do not wish to register with the Store, the MsalProvider can be used instead. However, you will still need to register the app with Azure AAD to get a clientId. This is the more traditional approach and supports both account types, consumer and organizational.

## Microsoft authentication and Microsoft Graph helpers in the Windows Community Toolkit

All of the Graph interaction in Contoso Notes is based on the new authentication providers and extensions in the Windows Community Toolkit's latest additions:

- `CommunityToolkit.Authentication` - A lightweight framework for managing user authentication events.
- `CommunityToolkit.Authentication.Msal` - An authentication provider based on the official Microsoft Authentication Library (MSAL).
- `CommunityToolkit.Authentication.Uwp` - An authentication provider based on the native Windows Account Manager (WAM) APIs.
- `CommunityToolkit.Graph` - Provider extensions for accessing the configured GraphServiceClient and getting tokens.

The new authentication providers in the Windows Community Toolkit make it EASY to get authenticated and make Graph requests, so you can focus more on 
building new Graph experiences and less on token management.

```
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;

// Create and set the global authentication provider.
IProvider provider = new MsalProvider(clientId, scopes);
ProviderManager.Instance.GlobalProvider = provider;

// Prompt for authentication.
await provider.SignInAsync();

// Once signed in, get the configured GraphServiceClient from the Graph SDK and it to make requests.
GraphServiceClient graphClient = provider.GetClient();
var me = await graphClient.Me.Request().GetAsync();

// Or get a token directly to make your own requests.
string token = await provider.GetTokenAsync();
```

## Built with the Windows Community Toolkit

Windows Community Toolkit is the easiest way to get started on Windows building first class applications with standardized and proven paradigms created by Microsoft and the community. In this demo we've leveraged WCT heavily for common needs such as converters, extensions, and UI controls.

This demo also leverages the new MVVM helpers in the `Microsoft.Toolkit.MVVM` package. We've integrated with RelayCommand and ObservableObject to support the View-ViewModel relationship.

Check out the Windows Community Toolkit docs for more details: [Introduction to the MVVM Toolkit](https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/introduction)

## Microsoft Graph Toolkit for Web
For web solutions, check out the [Microsoft Graph Toolkit](https://docs.microsoft.com/en-us/graph/toolkit/overview); A web component library for building Graph powered experiences in HTML and JavaScript.
