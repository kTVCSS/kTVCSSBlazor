using Radzen;
using kTVCSSBlazor.Server.Components;
using kTVCSSBlazor.Client.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using kTVCSSBlazor.Client.Extensions;

// TODO на завтра
// в редактировании профиля кнопки неадекватно себя ведут которые за загрузку файлов отвечают +
// остался открытым вопрос с конфигами игроков - заливка файлов через nginx не работает
// аватарки на стене + 
// таймаут хттп клиента + 
// репорты - тоже в сервис оповещений и создавать тикет +
// остальное дальше по плану
// api метод на фронте, который будет обновлять кеш игроков, ссылаясь на апи +

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