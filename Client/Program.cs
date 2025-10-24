using Blazored.LocalStorage;
using kTVCSSBlazor;
using kTVCSSBlazor.Client;
using kTVCSSBlazor.Client.Authorization;
using kTVCSSBlazor.Client.Extensions;
using kTVCSSBlazor.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Radzen;
using RadzenBlazorDemos.Services;

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
    "https://api.ktvcss.com",
};

builder.Services.AddSingleton(new ApiServerSelector(apiEndpoints.ToArray()));

#endif

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "kTVCSSTheme"; // The name of the cookie
    options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
});

builder.Services.AddScoped(sp =>
{
    var serverSelector = sp.GetRequiredService<ApiServerSelector>();
    var serverUrl = serverSelector.GetNextServer();
    return new HttpClient { BaseAddress = new Uri(serverUrl), Timeout = TimeSpan.FromMinutes(1) };
});

builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<KeepAliveService>();
builder.Services.AddScoped<ChatHubService>();
builder.Services.AddScoped<MMService>();
builder.Services.AddSingleton<WindowSizeService>(); // @inject WindowSizeService WindowSize
builder.Services.AddScoped<IMobileDetectionService, MobileDetectionService>(); //@inject IMobileDetectionService MobileDetectionService
builder.Services.AddScoped<PasswordCheckService>();
builder.Services.AddScoped<CryptoService>();

var host = builder.Build();

DateTimeExtensions.Initialize(host.Services.GetRequiredService<IJSRuntime>());

await host.RunAsync();