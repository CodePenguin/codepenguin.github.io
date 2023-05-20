using System.Net;
using System.Net.Sockets;

namespace OpenIdConnectConsoleTest;

public class HttpCallbackListener : ICallbackListener
{
    private HttpListener? _listener;

    public void Listen(AuthenticationSettings authSettings, out string redirectUri)
    {
        int redirectUriPort = GetRedirectUriPort(authSettings.RedirectUriPort);

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{redirectUriPort}/");
        _listener.Start();

        redirectUri = $"http://127.0.0.1:{redirectUriPort}/callback";
    }

    private static int GetRedirectUriPort(int defaultRedirectUriPort)
    {
        // Use a specific port if configured since Auth0 does not support randomly assigned ports
        if (defaultRedirectUriPort != 0)
        {
            return defaultRedirectUriPort;
        }
        // Retrieve an assigned port from the operating system
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async Task<Uri?> WaitForResponseAsync(CancellationToken cancellationToken)
    {
        var context = await _listener!.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(5), cancellationToken);
        var request = context.Request;
        using (var writer = new StreamWriter(context.Response.OutputStream))
        {
            writer.WriteLine($"<HTML><BODY>Please return to the application.</BODY></HTML>");
            writer.Flush();
        }
        _listener.Stop();
        return request.Url;
    }
}
