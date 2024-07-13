using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWebApp.Client;

internal sealed class ClientTokenHandler(AuthenticationStateProvider authStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            var accessToken = authState.User.Claims.Single(c => c.Type == UserInfo.AccessTokenClaimType).Value;

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
