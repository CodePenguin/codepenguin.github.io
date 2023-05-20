using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;

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
        AuthorizationCallbackResponse response = await RequestAuthorizationCodeAsync(codeVerifier, openIdConfiguration, stoppingToken);
        OpenIdConnectMessage tokenResponse = await RequestTokenAsync(response.RedirectUrl, response.AuthorizationCode, codeVerifier, openIdConfiguration, stoppingToken);
        ClaimsPrincipal claimsPrincipal = ValidateToken(tokenResponse.IdToken, _authSettings.ClientId, openIdConfiguration);
        return claimsPrincipal;
    }

    private async Task<OpenIdConnectConfiguration> RetrieveOpenIdConnectConfigurationAsync(CancellationToken cancellationToken)
    {
        var uri = _authSettings.Authority.TrimEnd('/') + "/.well-known/openid-configuration";
        _logger.LogInformation("Retrieving configuration from {endpoint}...", uri);
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            uri,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());
        return await configurationManager.GetConfigurationAsync(cancellationToken);
    }

    private async Task<AuthorizationCallbackResponse> RequestAuthorizationCodeAsync(string codeVerifier, OpenIdConnectConfiguration openIdConfiguration, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Requesting authorization code from {endpoint}...", openIdConfiguration.AuthorizationEndpoint);

        // Generate values used to protect and verify the request
        var requestState = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(16));
        var codeChallenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

        // Start the listener and generate the Redirect URI
        ICallbackListener listener = CallbackListenerFactory.GetCallbackListener();
        listener.Listen(_authSettings, out var redirectUri);

        // Build the authorization request URL
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

        string url = BuildUrl(openIdConfiguration.AuthorizationEndpoint, parameters);

        // Open System Browser and wait for the redirect response URI
        _logger.LogInformation("Opening default browser...");
        _logger.LogDebug("Request URL: {request}", url);

        OpenSystemBrowser(url);

        _logger.LogInformation("Waiting for response from browser...");
        var responseUri = await listener.WaitForResponseAsync(stoppingToken);

        _logger.LogDebug("Response URL: {url}", responseUri);

        string? error = null;
        string? errorDescription = null;

        // Verify the redirect response URI exactly matches the Redirect URI sent in the authorization request
        if (!responseUri!.ToString().StartsWith(redirectUri))
        {
            error = "Invalid Response";
            errorDescription = $"The response does not match the redirect URI: {redirectUri}";
        }

        // Check response for errors
        var queryString = HttpUtility.ParseQueryString(responseUri?.Query ?? string.Empty);
        error ??= queryString["error"];
        if (error != null)
        {
            errorDescription ??= queryString["error_description"] ?? "Unknown Error";
            _logger.LogError("Authorization request failed: {error} - {description}", error, errorDescription);
            throw new Exception($"Authorization request failed: {error} - {errorDescription}");
        }

        var callbackResponse = new AuthorizationCallbackResponse
        {
            AuthorizationCode = queryString["code"] ?? string.Empty,
            RedirectUrl = redirectUri,
            State = queryString["state"] ?? string.Empty
        };

        // Verify the state matches what was sent in the authorization request
        if ((requestState != callbackResponse.State) || string.IsNullOrEmpty(callbackResponse.AuthorizationCode))
        {
            throw new Exception("Invalid authorization state");
        }
        return callbackResponse;
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

    private void OpenSystemBrowser(string url)
    {
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
    }

    private async Task<OpenIdConnectMessage> RequestTokenAsync(string redirectUri, string authorizationCode, string codeVerifier, OpenIdConnectConfiguration openIdConfiguration, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Requesting access token from {endpoint}...", openIdConfiguration.TokenEndpoint);
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", _authSettings.ClientId },
            { "code", authorizationCode },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier }
        };

        if (!string.IsNullOrEmpty(_authSettings.ClientSecret))
        {
            parameters.Add("client_secret", _authSettings.ClientSecret);
        }

        var request = new HttpRequestMessage(HttpMethod.Post, openIdConfiguration.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        _logger.LogDebug("Request: {request}, Form Parameters: {parameters}", request, parameters);
        var response = _httpClient.Send(request, stoppingToken);
        var json = await response.Content.ReadAsStringAsync(stoppingToken);
        _logger.LogDebug("Response: {json}", json);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token request failed: {response.ReasonPhrase}");
        }
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
            RoleClaimType = _authSettings.RoleClaimType,
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
