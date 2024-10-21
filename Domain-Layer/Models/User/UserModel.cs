using Microsoft.AspNetCore.Identity;

namespace Domain_Layer.Models.User
{
    public class UserModel : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public bool IsDeleted { get; set; }
        //adding new properties for refreshing a jwt token
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
