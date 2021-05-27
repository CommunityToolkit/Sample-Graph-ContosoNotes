# Enlighten your Windows App with Microsoft Graph resources

Thank you for watching our session at Build 2021. If you haven't seen it yet, [watch it here](https://aka.ms/OD531)

This page contains resources for getting started with Microsoft Graph in your Windows apps

## Demos

- **Contoso Notes UWP app** is located in the `ContosoNotes` folder in this repository - learn more about the app by reading the [README](./README.md)
- **Contoso Notes Web app** is located in the `ContosoNotesWeb` folder in this repository - learn more about the app by reading the [README](./ContosoNotesWeb/README.md)

## UWP/WPF/.NET

- **[Preview] [Windows Community Toolkit authentication and Microsoft Graph helpers](https://aka.ms/wgt)** - couple lines of code to authenticate users and call Microsoft Graph, built on top of MSAL and the Microsoft Graph libraries for .NET
  - Get started with [WindowsProvider](https://docs.microsoft.com/windows/communitytoolkit/graph/authentication/windowsprovider) for UWP - [Sample](https://github.com/windows-toolkit/Graph-Controls/tree/main/Samples/UwpWindowsProviderSample)
  - Get started with [MsalProvider](https://docs.microsoft.com/windows/communitytoolkit/graph/authentication/msalprovider) for UWP - [Sample](https://github.com/windows-toolkit/Graph-Controls/tree/main/Samples/UwpMsalProviderSample)
  - Get started with [MsalProvider](https://docs.microsoft.com/windows/communitytoolkit/graph/authentication/msalprovider) for WPF - [Sample](https://github.com/windows-toolkit/Graph-Controls/tree/main/Samples/WpfMsalProviderSample)

### Alternatively, use the .NET libraries directly

- [**MSAL.NET**](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) - Microsoft Authentication Library for .NET, UWP, NetCore, and Xamarin - it enables you to acquire security tokens to call protected APIs such as Microsoft Graph. 
  - [Samples for desktop and mobile public client apps](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code#desktop-and-mobile-public-client-apps) 

- [**Microsoft Graph .NET SDK**](https://github.com/microsoftgraph/msgraph-sdk-dotnet) - .NET Client library that targets .NetStandard 2.0 and .Net Framework 4.6.1.

## PWA (Progressive Web Apps) and Electron

- **[Microsoft Graph Toolkit](https://aka.ms/mgt)** - couple lines of code to authenticate users, call Microsoft Graph, and use prebuilt Microsoft Graph connected components. Built on top of MSAL and the Microsoft Graph client sdk
  - [Component playground](https://aka.ms/mgt)
  - [Get started (vanilla JS)](https://docs.microsoft.com/graph/toolkit/get-started/build-a-web-app)
  - [Get started (React)](https://docs.microsoft.com/graph/toolkit/get-started/use-toolkit-with-react)
  - [Get started (Angular)](https://docs.microsoft.com/graph/toolkit/get-started/use-toolkit-with-angular)
  - [Get started for Electron](https://docs.microsoft.com/graph/toolkit/get-started/build-an-electron-app)

### Alternatively, use the JS/TS libraries directly

- [**MSAL.js**](https://github.com/AzureAD/microsoft-authentication-library-for-js) - Microsoft Authentication Library for JavaScript - it enables you to acquire security tokens to call protected APIs such as Microsoft Graph. 

- [**Microsoft Graph JavaScript SDK**](https://github.com/microsoftgraph/msgraph-sdk-javascript) - JavaScript Client library tha can be used server-side and in the browser

