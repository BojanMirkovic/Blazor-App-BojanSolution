using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Shared_Layer.DTO_s.User;

namespace Shared_Layer.ApiServices.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        private readonly IConfiguration _config;
        private string authTokenStorageKey;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, CustomAuthenticationStateProvider authStateProvider, IConfiguration config)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _config = config;
            authTokenStorageKey = _config["authTokenStorageKey"]!;
        }

        public async Task<bool> Login(LoginUserDTO loginRequest)
        {
            string apiEndpoint = _config["AppLoginEndpoint"]!;
            var response = await _httpClient.PostAsJsonAsync(apiEndpoint, loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginUserResponseDTO>();

                await _localStorage.SetItemAsync(authTokenStorageKey, loginResponse.Token);

                _authStateProvider.MarkUserAsAuthenticated(loginResponse.Token);

                // Set the Authorization header for future requests
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", loginResponse.Token);

                return true;
            }

            return false;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync(authTokenStorageKey);

            _authStateProvider.MarkUserAsLoggedOut();

            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

}

