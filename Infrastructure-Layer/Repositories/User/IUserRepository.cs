using Domain_Layer.Models.User;

namespace Infrastructure_Layer.Repositories.User
{
    public interface IUserRepository
    {
        Task<UserModel> RegisterUserAsync(UserModel newUser, string password, string role);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserByEmailAsync(string email);
        Task<UserModel> GetUserByIdAsync(string userId);
        Task<UserModel> UpdateUserAsync(UserModel userToUpdate, string currentPassword, string newPassword);
        Task<bool> DeleteUserByIdAsync(string userId);
        Task<(string accessToken, string refrshToken)> GenerateJwtTokenAsync(UserModel user);
        Task<UserModel> FindUserByRefreshToken(string? refreshToken);
    }
}
