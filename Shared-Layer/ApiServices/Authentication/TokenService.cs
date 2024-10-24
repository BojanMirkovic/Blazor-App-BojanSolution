using Shared_Layer.DTO_s.User;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Shared_Layer.ApiServices.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _config;
        private string authTokenStorageKey;
        private string? _refreshToken; // Store the refresh token temporarily

        public TokenService(HttpClient httpClient, ILocalStorageService localStorage, IConfiguration config)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _config = config;
            authTokenStorageKey = _config["authTokenStorageKey"]!;
        }
        public async Task<string> RefreshAccessTokenAsync()
        {
            string apiEndpoint = _config["AppRefreshTokenEndpoint"]!;
            // Send the Refresh Token to the backend to get a new Access Token
            var refreshTokenRequest = new RefreshTokenRequestDTO
            {
                RefreshToken = _refreshToken
            };
            var response = await _httpClient.PostAsJsonAsync(apiEndpoint, refreshTokenRequest);
            /*********************************************************/
            if (response.IsSuccessStatusCode)
            {
                var newToken = await response.Content.ReadFromJsonAsync<LoginUserResponseDTO>();
                if (newToken != null && !string.IsNullOrEmpty(newToken.AccessToken))
                {
                    // Save the new Access Token
                    await _localStorage.SetItemAsync(authTokenStorageKey, newToken.AccessToken);

                    // Update the Refresh Token 
                    if (!string.IsNullOrEmpty(newToken.RefreshToken))
                    {
                        _refreshToken = newToken.RefreshToken;
                    }

                    //_authStateProvider.MarkUserAsAuthenticated(newToken.AccessToken);

                    //// Set the Authorization header for future requests
                    //_httpClient.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", newToken.AccessToken);

                    return newToken.AccessToken;
                }
            }

            // Return empty if refresh fails
            return string.Empty; 
        }
    }
}
