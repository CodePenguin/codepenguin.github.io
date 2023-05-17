namespace OpenIdConnectConsoleTest;

public class AuthenticationSettings
{
    public string Audience { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUriPath { get; set; } = "callback";
    public int RedirectUriPort { get; set; } = 0;
    public string RoleClaimType { get; set; } = "roles";
}
