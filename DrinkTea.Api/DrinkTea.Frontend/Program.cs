using DrinkTea.Client;
using DrinkTea.Client.Infrastructure;
using DrinkTea.Frontend;
using DrinkTea.Frontend.Services.Api;
using DrinkTea.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- НАСТРОЙКА АДРЕСА API ---
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    // Если в appsettings.json пусто, берем адрес самого фронта (для Docker/Prod)
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
}
// ----------------------------

builder.Services.AddScoped(sp =>
{
    var options = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    return options;
});

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddAuthorizationCore(options => { });
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

// Настраиваем именованный клиент с динамическим адресом
builder.Services.AddHttpClient("DrinkTeaAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("DrinkTeaAPI");
});

// Регистрация твоих сервисов
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IUsersApiService, UsersApiService>();
builder.Services.AddScoped<IVisitsApiService, VisitsApiService>();
builder.Services.AddScoped<IBrewingApiService, BrewingApiService>();
builder.Services.AddScoped<ITeaApiService, TeaApiService>();
builder.Services.AddScoped<ISalesApiService, SalesApiService>();
builder.Services.AddScoped<IReportsApiService, ReportsApiService>();

await builder.Build().RunAsync();
