using DrinkTea.Client;
using DrinkTea.Client.Infrastructure;
using DrinkTea.Frontend;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var options = new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Чтобы не падать на разнице Role vs role
    };
    // Добавляем поддержку строк для Enum
    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

    return options;
});


// 1. Регистрируем наш обработчик (Infrastructure/AuthHeaderHandler.cs)
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddAuthorizationCore(options =>{});
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();


// 2. Настраиваем HttpClient через Factory
builder.Services.AddHttpClient("DrinkTeaAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7282/"); // ПРОВЕРЬ ПОРТ СВОЕГО API!
})
.AddHttpMessageHandler<AuthHeaderHandler>();

// 3. ГЛАВНОЕ: Перезаписываем стандартный HttpClient
// Теперь везде, где написано @inject HttpClient, будет прилетать клиент с токеном
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("DrinkTeaAPI");
});

await builder.Build().RunAsync();
