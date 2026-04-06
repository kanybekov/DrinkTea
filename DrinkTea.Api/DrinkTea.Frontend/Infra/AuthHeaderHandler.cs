using System.Net;
using System.Net.Http.Headers;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace DrinkTea.Client.Infrastructure;

public class AuthHeaderHandler(IJSRuntime js, NavigationManager nav) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Достаем токен и прикрепляем к запросу
        var token = await js.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 2. Выполняем запрос
        var response = await base.SendAsync(request, cancellationToken);

        // 3. ПРОВЕРКА: Если сервер ответил 401 (Unauthorized)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Очищаем локальное хранилище, чтобы не пытаться использовать дохлый токен снова
            await js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await js.InvokeVoidAsync("localStorage.removeItem", "userName");

            // Редирект на логин
            nav.NavigateTo("/login");
        }

        return response;
    }
}
