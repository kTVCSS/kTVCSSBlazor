using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

using kTVCSSBlazor.Client;
using Microsoft.AspNetCore.Components.Authorization;
using kTVCSSBlazor.Client.Authorization;
using Microsoft.JSInterop;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri("http://localhost:3000") });
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();

var host = builder.Build();
await host.RunAsync();