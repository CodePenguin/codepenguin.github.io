using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace OpenIdConnectConsoleTest;

public class Worker : BackgroundService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly static HttpClient _httpClient = new();
    private readonly ILogger<Worker> _logger;

    public Worker(
        ILogger<Worker> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IConfiguration configuration)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>() ?? new AuthenticationSettings();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            ClaimsPrincipal user = await AuthenticateExternalUserAsync(stoppingToken);
            var userId = user.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = user.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;

            _logger.LogInformation("Authenticated as Id: {userId} Name: {name} Email: {email} Claims: {claims}",
                userId,
                user.Identity?.Name,
                email,
                user.Claims);

            _logger.LogInformation("Is in role Admin: {response}", user.IsInRole("Admin"));
            _logger.LogInformation("Is in role Guest: {response}", user.IsInRole("Guest"));

            // Perform other actions here

            _hostApplicationLifetime.StopApplication();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {message}", ex.Message);
            Environment.Exit(1);
        }
    }

    private async Task<ClaimsPrincipal> AuthenticateExternalUserAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Authenticating with {authority}...", _authSettings.Authority);

        OpenIdConnectConfiguration openIdConfiguration = await RetrieveOpenIdConnectConfigurationAsync(stoppingToken);
        string codeVerifier = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        int redirectUriPort = GetRedirectUriPort();
        string authorizationCode = await RequestAuthorizationCodeAsync(redirectUriPort, codeVerifier, openIdConfiguration, stoppingToken);
        OpenIdConnectMessage tokenResponse = await RequestTokenAsync(redirectUriPort, authorizationCode, codeVerifier, openIdConfiguration, stoppingToken);
        ClaimsPrincipal claimsPrincipal = ValidateToken(tokenResponse.IdToken, _authSettings.ClientId, openIdConfiguration);
        return claimsPrincipal;
    }

    private async Task<OpenIdConnectConfiguration> RetrieveOpenIdConnectConfigurationAsync(CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(_authSettings.Authority)
        {
            Path = "/.well-known/openid-configuration"
        };
        var uri = uriBuilder.ToString();
        _logger.LogInformation("Retrieving configuration from {endpoint}...", uri);
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            uri,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());
        return await configurationManager.GetConfigurationAsync(cancellationToken);
    }

    private int GetRedirectUriPort()
    {
        // Use a specific port if configured since Auth0 does not support randomly assigned ports
        if (_authSettings.RedirectUriPort != 0)
        {
            return _authSettings.RedirectUriPort;
        }
        // Retrieve an assigned port from the operating system
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private string GetRedirectUri(int port)
    {
        return $"http://{IPAddress.Loopback}:{port}/{_authSettings.RedirectUriPath}";
    }

    private async Task<string> RequestAuthorizationCodeAsync(int redirectUriPort, string codeVerifier, OpenIdConnectConfiguration openIdConfiguration, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Requesting authorization code from {endpoint}...", openIdConfiguration.AuthorizationEndpoint);

        var authorizationCode = string.Empty;
        var requestState = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(16));
        var codeChallenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

        _logger.LogInformation("Starting HTTP Listener...");
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://{IPAddress.Loopback}:{redirectUriPort}/");
        listener.Start();

        var redirectUri = GetRedirectUri(redirectUriPort);
        var parameters = new Dictionary<string, string>
        {
            { "response_type", "code" },
            { "client_id", _authSettings.ClientId },
            { "redirect_uri", redirectUri },
            { "scope", "openid email profile" },
            { "state", requestState },
            { "code_challenge", codeChallenge },
            { "code_challenge_method", "S256" },
        };

        if (!string.IsNullOrEmpty(_authSettings.Audience))
        {
            parameters.Add("audience", _authSettings.Audience);
        }

        var url = BuildUrl(openIdConfiguration.AuthorizationEndpoint, parameters);

        _logger.LogInformation("Opening default browser...", url);
        _logger.LogDebug("Request URL: {request}", url);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            throw new NotSupportedException("Cannot open default browser");
        }

        _logger.LogInformation("Waiting for response from browser...");
        var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromMinutes(2), stoppingToken);
        var request = context.Request;
        _logger.LogDebug("Response URL: {url}", request.Url);

        string? error = null;
        string? errorDescription = null;
        if (!redirectUri.Equals($"http://{request.Url?.Host}:{request.Url?.Port}{request.Url?.AbsolutePath}"))
        {
            error = "Invalid Response";
            errorDescription = "The response does not match the redirect URI";
        }

        string responseText;
        error ??= request.QueryString["error"];
        if (error != null)
        {
            errorDescription ??= request.QueryString["error_description"] ?? "Unknown Error";
            _logger.LogError("Authorization request failed: \"{error}\": {description}", error, errorDescription);
            responseText = "Authorization request failed";
        }
        else
        {
            authorizationCode = request.QueryString["code"] ?? string.Empty;
            var responseState = request.QueryString["state"];

            if ((requestState != responseState) || string.IsNullOrEmpty(authorizationCode))
            {
                responseText = "Authorization state was not valid";
                _logger.LogError("{response}", responseText);
            }
            else
            {
                responseText = "Authorization code received";
                _logger.LogInformation("{response}", responseText);
            }
        }

        var response = context.Response;
        using (var writer = new StreamWriter(response.OutputStream))
        {
            writer.WriteLine($"<HTML><BODY>{responseText}</BODY></HTML>");
            writer.Flush();
        }

        return !string.IsNullOrEmpty(authorizationCode)
            ? authorizationCode
            : throw new Exception(responseText);
    }

    private static string BuildUrl(string baseUrl, Dictionary<string, string> queryParameters)
    {
        var queryBuilder = new StringBuilder();
        foreach (var pair in queryParameters)
        {
            queryBuilder.Append('&');
            queryBuilder.Append(UrlEncoder.Default.Encode(pair.Key));
            queryBuilder.Append('=');
            queryBuilder.Append(UrlEncoder.Default.Encode(pair.Value));
        }
        var uri = new UriBuilder(baseUrl)
        {
            Query = queryBuilder.ToString().Trim('&')
        };
        return uri.ToString();
    }

    private async Task<OpenIdConnectMessage> RequestTokenAsync(int redirectUriPort, string authorizationCode, string codeVerifier, OpenIdConnectConfiguration openIdConfiguration, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Requesting access token from {endpoint}...", openIdConfiguration.TokenEndpoint);
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", _authSettings.ClientId },
            { "code", authorizationCode },
            { "redirect_uri", GetRedirectUri(redirectUriPort) },
            { "code_verifier", codeVerifier }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, openIdConfiguration.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        _logger.LogDebug("Request: {request}, Form Parameters: {parameters}", request, parameters);
        var response = _httpClient.Send(request, stoppingToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token request failed: {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync(stoppingToken);
        _logger.LogDebug("Response: {json}", json);
        return new OpenIdConnectMessage(json);
    }

    private ClaimsPrincipal ValidateToken(string token, string audience, OpenIdConnectConfiguration openIdConfiguration)
    {
        _logger.LogInformation("Validating token...");
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.InboundClaimTypeMap.Clear();
        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKeys = openIdConfiguration.SigningKeys,
            NameClaimType = "name",
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            RoleClaimType = $"{_authSettings.ClientId}/roles",
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidAudience = audience,
            ValidIssuer = _authSettings.Authority,
        };
        ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        _logger.LogDebug("Validated token: {token}", validatedToken);
        return claimsPrincipal;
    }
}
