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

var apiEndpoints = new List<string>()
{
    "http://localhost:3000",
};

builder.Services.AddSingleton(new ApiServerSelector(apiEndpoints.ToArray()));

builder.Services.AddScoped(sp =>
{
    var serverSelector = sp.GetRequiredService<ApiServerSelector>();
    var serverUrl = serverSelector.GetNextServer();
    return new HttpClient { BaseAddress = new Uri(serverUrl) };
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();

var host = builder.Build();
await host.RunAsync();