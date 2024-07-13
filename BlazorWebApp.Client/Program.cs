using BlazorWebApp.Client;
using BlazorWebApp.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddTransient<CustomAuthorizationMessageHandler>();
builder.Services.AddTransient<ClientTokenHandler>();
builder.Services.AddTransient<RemoteApiService>();

builder.Services.AddHttpClient<RemoteApiService>(client =>
{
    client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
}).AddHttpMessageHandler<ClientTokenHandler>();

await builder.Build().RunAsync();
