using Blazored.LocalStorage;
using kTVCSSBlazor.Client;
using kTVCSSBlazor.Client.Authorization;
using kTVCSSBlazor.Client.Extensions;
using kTVCSSBlazor.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();

#if DEBUG

var apiEndpoints = new List<string>()
{
    "http://localhost:3000",
};

builder.Services.AddSingleton(new ApiServerSelector(apiEndpoints.ToArray()));

#endif

#if RELEASE 

var apiEndpoints = new List<string>()
{
    "https://api.ktvcss.ru",
};

builder.Services.AddSingleton(new ApiServerSelector(apiEndpoints.ToArray()));

#endif

builder.Services.AddScoped(sp =>
{
    var serverSelector = sp.GetRequiredService<ApiServerSelector>();
    var serverUrl = serverSelector.GetNextServer();
    return new HttpClient { BaseAddress = new Uri(serverUrl), Timeout = TimeSpan.FromSeconds(30) };
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();

builder.Services.AddScoped<ChatHubService>();
builder.Services.AddSingleton<WindowSizeService>(); // @inject WindowSizeService WindowSize
builder.Services.AddScoped<IMobileDetectionService, MobileDetectionService>(); //@inject IMobileDetectionService MobileDetectionService

var host = builder.Build();
await host.RunAsync();