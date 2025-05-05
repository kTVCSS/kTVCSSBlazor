using Radzen;
using kTVCSSBlazor.Server.Components;
using kTVCSSBlazor.Client.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using kTVCSSBlazor.Client.Extensions;

// TODO �� ������
// � �������������� ������� ������ ����������� ���� ����� ������� �� �������� ������ �������� +
// ������� �������� ������ � ��������� ������� - ������� ������ ����� nginx �� ��������
// �������� �� ����� + 
// ������� ���� ������� + 
// ������� - ���� � ������ ���������� � ��������� ����� +
// ��������� ������ �� �����
// api ����� �� ������, ������� ����� ��������� ��� �������, �������� �� ��� +

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
builder.Services.AddHttpClient("default", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StateProvider>());

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