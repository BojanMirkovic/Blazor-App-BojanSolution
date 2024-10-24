

namespace Shared_Layer.ApiServices.Authentication
{
    public interface ITokenService
    {
        Task<string> RefreshAccessTokenAsync();
    }
}
