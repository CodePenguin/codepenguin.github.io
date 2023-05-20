namespace OpenIdConnectConsoleTest;

public class AuthorizationCallbackResponse
{
    public string AuthorizationCode { get; init; } = string.Empty;
    public string RedirectUrl { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}
