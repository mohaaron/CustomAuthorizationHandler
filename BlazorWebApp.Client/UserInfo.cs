using System.Security.Claims;

namespace BlazorWebApp.Client
{
    // Add properties to this class and update the server and client AuthenticationStateProviders
    // to expose more information about the authenticated user to the client.
    public sealed class UserInfo
    {
        public required string UserId { get; init; }
        public required string Name { get; init; }
        public required string AccessToken { get; init; }

        public const string UserIdClaimType = "sub";
        public const string NameClaimType = "name";
        public const string AccessTokenClaimType = "access_token";

        public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal, string accessToken) =>
            new()
            {
                UserId = GetRequiredClaim(principal, UserIdClaimType),
                Name = GetRequiredClaim(principal, NameClaimType),
                AccessToken = accessToken,
            };

        public ClaimsPrincipal ToClaimsPrincipal() =>
            new(new ClaimsIdentity(
                [new(UserIdClaimType, UserId), new(NameClaimType, Name), new(AccessTokenClaimType, AccessToken)],
                authenticationType: nameof(UserInfo),
                nameType: NameClaimType,
                roleType: null));

        private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
            principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");
    }
}
