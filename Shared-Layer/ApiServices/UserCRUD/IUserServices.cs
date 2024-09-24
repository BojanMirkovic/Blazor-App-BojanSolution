using Domain_Layer.Models.User;
using Shared_Layer.DTO_s.User;

namespace Shared_Layer.ApiServices.UserCRUD
{
    public interface IUserServices
    {
        Task RegisterNewUserAsync(RegisterUserDTO newUser);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserByEmailAsync(string email);
        Task<UserModel> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UpdatingUserDTO userToUpdate);
        Task<bool> DeleteUserByIdAsync(string userId);
    }
}
