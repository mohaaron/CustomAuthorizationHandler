using Microsoft.AspNetCore.Authentication;

namespace BlazorWebApp;

internal sealed class ServerTokenHandler(IHttpContextAccessor contextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = contextAccessor.HttpContext ??
            throw new InvalidOperationException("No HttpContext available from the IHttpContextAccessor!");

        var accessToken = await httpContext.GetTokenAsync("access_token") ??
            throw new InvalidOperationException("No access_token was saved");

        request.Headers.Authorization = new("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
