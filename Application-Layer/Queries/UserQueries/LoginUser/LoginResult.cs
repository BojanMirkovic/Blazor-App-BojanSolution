
namespace Application_Layer.Queries.LoginUser
{
    public class LoginResult
    {
        public bool Successful { get; set; }
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

    }
}
