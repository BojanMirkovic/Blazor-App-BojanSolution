using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Shared_Layer.ApiServices.Authentication;


public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ITokenService _tokenService;
    private string authTokenStorageKey;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient, IConfiguration config, ITokenService tokenService)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _config = config;
        authTokenStorageKey = _config["authTokenStorageKey"]!;
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(authTokenStorageKey);
        /**************************************************************************/
        // Check if the token exists and is expired
        if (string.IsNullOrEmpty(token))
        {
            // Token is missing, return an anonymous user
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        if (IsTokenExpired(token))
        {
            // If the token is expired, attempt to refresh it
            token = await TryRefreshTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                // If the refresh fails, return an anonymous user
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        /**************************************************************************/


        //if (string.IsNullOrWhiteSpace(token))
        //{
        //    // If there is no token, return anonymous user
        //    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //}

        // If a token exists, set up claims from the JWT token
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "jwtAuthType");
        var user = new ClaimsPrincipal(identity);

        // Set the Authorization header for future requests
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(user);
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var claims = jwtToken.Claims.ToList();
        var identity = new ClaimsIdentity(claims, "jwtAuthType");
        var user = new ClaimsPrincipal(identity);

        // Notify Blazor that the authentication state has changed
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        // Notify Blazor that the user has logged out
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }
    private async Task<string?> TryRefreshTokenAsync()
    {
        try
        {
            // Attempt to refresh the access token
            var newToken = await _tokenService.RefreshAccessTokenAsync();

            if (!string.IsNullOrEmpty(newToken))
            {
                // If successful, save the new token to local storage
                await _localStorage.SetItemAsync(authTokenStorageKey, newToken);
                // Update HTTP client authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                // Notify that the authentication state has changed
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return newToken;
            }
        }
        catch (Exception ex)
        {
            // Log the error for debugging purposes
            Console.WriteLine($"Token refresh failed: {ex.Message}");
        }

        return null; // Return null if the refresh fails
    }

    private bool IsTokenExpired(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return true; // Consider token as expired if it's null or empty
        }

        // Create a JwtSecurityTokenHandler to parse the JWT token
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        // Read the JWT token
        var jwtToken = jwtTokenHandler.ReadJwtToken(token);

        // Get the expiry date of the token
        var expiryDate = jwtToken.ValidTo;

        // Check if the current UTC time is after the token's expiration time
        // Add a buffer (e.g., 1 minute) to avoid expiration issues due to latency
        return expiryDate < DateTime.UtcNow.AddMinutes(-1);
    }
}

