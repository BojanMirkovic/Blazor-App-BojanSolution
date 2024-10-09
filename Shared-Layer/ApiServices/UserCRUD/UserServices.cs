using System.Net.Http.Json;
using System.Text.Json;
using Domain_Layer.Models.User;
using Microsoft.Extensions.Configuration;
using Shared_Layer.DTO_s.Error;
using Shared_Layer.DTO_s.User;

namespace Shared_Layer.ApiServices.UserCRUD
{
    public class UserServices : IUserServices
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public UserServices(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        public async Task<HttpResponseMessage> DeleteUserByIdAsync(string userId)
        {
            string apiEndpoint = _config["DeleteUserEndpoint"]!;
            return await _httpClient.DeleteAsync($"{apiEndpoint}/{userId}");
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            string apiEndpoint = _config["GetAllUsersEndpoint"]!;
            return await _httpClient.GetFromJsonAsync<List<UserModel>>(apiEndpoint);
        }

        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            string apiEndpoint = _config["GetUserByEmailEndpoint"]!;
            return await _httpClient.GetFromJsonAsync<UserModel>($"{apiEndpoint}/{email}");
        }

        public async Task<UserModel> GetUserByIdAsync(string userId)
        {
            string apiEndpoint = _config["GetUserByIdEndpiont"]!;
            return await _httpClient.GetFromJsonAsync<UserModel>($"{apiEndpoint}/{userId}");
        }

        public async Task RegisterNewUserAsync(RegisterUserDTO newUser)
        {
            string apiEndpoint = _config["RegisterNewUserEndpoint"]!;
            var data = new
            {
                newUser.Role,
                newUser.FirstName,
                newUser.LastName,
                newUser.Email,
                newUser.Password,
                newUser.ConfirmPassword
            };
            using (var response = await _httpClient.PostAsJsonAsync(apiEndpoint, data))
            { 
                if (response.IsSuccessStatusCode == false)
                {
                    // Read the response content as a string
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Throw an exception with the detailed error message
                    throw new Exception(errorContent);
                }
            }
        }

        public async Task UpdateUserAsync(UpdatingUserDTO userToUpdate)
        {
            string apiEndpoint = _config["UpdateUserEndpoint"]!;
            var data = new
            {
                userToUpdate.Role,
                userToUpdate.FirstName,
                userToUpdate.LastName,
                userToUpdate.Email,
                userToUpdate.CurrentPassword,
                userToUpdate.NewPassword,
            };
            using (var response = await _httpClient.PutAsJsonAsync(apiEndpoint, data))
            {
                if (response.IsSuccessStatusCode == false)
                {
                    // Read the response content as a string
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Throw an exception with the detailed error message
                    throw new Exception(errorContent);
                }
            }
        }
    }
}

