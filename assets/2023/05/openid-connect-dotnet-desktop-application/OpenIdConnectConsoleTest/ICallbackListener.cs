namespace OpenIdConnectConsoleTest;

public interface ICallbackListener
{
    public void Listen(AuthenticationSettings authSettings, out string redirectUri);
    public Task<Uri?> WaitForResponseAsync(CancellationToken cancellationToken);
}
