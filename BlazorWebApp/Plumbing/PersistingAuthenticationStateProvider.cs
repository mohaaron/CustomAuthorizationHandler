using System.Diagnostics;
using BlazorWebApp.Client;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWebApp.Plumbing;

// This is a server-side AuthenticationStateProvider that uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingAuthenticationStateProvider : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState persistentComponentState;
    private readonly IUserTokenStore tokenStore;
    private readonly PersistingComponentStateSubscription subscription;
    private Task<AuthenticationState>? authenticationStateTask;

    public PersistingAuthenticationStateProvider(PersistentComponentState state, IUserTokenStore store)
    {
        tokenStore = store;
        persistentComponentState = state;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => authenticationStateTask ??
            throw new InvalidOperationException($"Do not call {nameof(GetAuthenticationStateAsync)} outside of the DI scope for a Razor component. Typically, this means you can call it only within a Razor component or inside another DI service that is resolved for a Razor component.");

    public void SetAuthenticationState(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        var authenticationState = await GetAuthenticationStateAsync();
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var token = await tokenStore.GetTokenAsync(principal);

            if (token.AccessToken is null)
            {
                throw new UnreachableException("The access token should not be null!");
            }

            persistentComponentState.PersistAsJson(nameof(UserInfo), UserInfo.FromClaimsPrincipal(principal, token.AccessToken));
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
    }
}
