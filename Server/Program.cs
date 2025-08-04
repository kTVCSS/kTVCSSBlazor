using Blazored.LocalStorage;
using kTVCSSBlazor.Client.Authorization;
using kTVCSSBlazor.Client.Extensions;
using kTVCSSBlazor.Client.Services;
using kTVCSSBlazor.Server.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using RadzenBlazorDemos.Services;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
#if RELEASE
    WebRootPath = "/home/aspnet/ktvcss.ru/wwwroot"
#endif
};

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient("default", c => c.Timeout = TimeSpan.FromMinutes(1));
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());

builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<ChatHubService>();
builder.Services.AddScoped<MMService>();
builder.Services.AddSingleton<WindowSizeService>(); // @inject WindowSizeService WindowSize
builder.Services.AddScoped<IMobileDetectionService, MobileDetectionService>(); //@inject IMobileDetectionService MobileDetectionService

var app = builder.Build();

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveWebAssemblyRenderMode()
   .AddAdditionalAssemblies(typeof(kTVCSSBlazor.Client._Imports).Assembly);

app.Run();
