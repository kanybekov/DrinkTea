using System.Net.Http; // Для HttpRequestMessage
using System.Net.Http.Headers; // Для AuthenticationHeaderValue
using System.Threading; // Для CancellationToken
using System.Threading.Tasks; // Для Task
using Microsoft.JSInterop;

namespace DrinkTea.Client.Infrastructure;

public class AuthHeaderHandler(IJSRuntime js) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthHandler Error]: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
