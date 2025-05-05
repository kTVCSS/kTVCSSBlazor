using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

using kTVCSSBlazor.Client;
using Microsoft.AspNetCore.Components.Authorization;
using kTVCSSBlazor.Client.Authorization;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using kTVCSSBlazor.Client.Extensions;

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

var host = builder.Build();
await host.RunAsync();