using System.Net.Http.Json;
using Domain_Layer.Models.User;
using Shared_Layer.DTO_s.User;

namespace Shared_Layer.ApiServices.UserCRUD
{
    public class UserServices : IUserServices
    {
        private readonly HttpClient _httpClient;

        public UserServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public Task<bool> DeleteUserByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<UserModel>>("api/User/GetAllUsers");
        }

        public Task<UserModel> GetUserByEmailAsync(string email)
        {
            return _httpClient.GetFromJsonAsync<UserModel>($"api/User/by-email/{email}");
        }

        public Task<UserModel> GetUserByIdAsync(string userId)
        {
            return _httpClient.GetFromJsonAsync<UserModel>($"api/User/{userId}");
        }

        public async Task RegisterNewUserAsync(RegisterUserDTO newUser)
        {
            var data = new
            {
                newUser.Role,
                newUser.FirstName,
                newUser.LastName,
                newUser.Email,
                newUser.Password,
                newUser.ConfirmPassword
            };
            using (var response = await _httpClient.PostAsJsonAsync("api/User/register", data))
            {
                if (response.IsSuccessStatusCode == false)
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task UpdateUserAsync(UpdatingUserDTO userToUpdate)
        {
            var data = new
            {
                userToUpdate.Role,
                userToUpdate.FirstName,
                userToUpdate.LastName,
                userToUpdate.Email,
                userToUpdate.CurrentPassword,
                userToUpdate.NewPassword,
            };
            using (var response = await _httpClient.PutAsJsonAsync("api/User/updateUser", data))
            {
                if (response.IsSuccessStatusCode == false)
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}

