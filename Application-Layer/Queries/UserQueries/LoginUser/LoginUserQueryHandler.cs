using Domain_Layer.Models.User;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application_Layer.Queries.LoginUser
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, LoginResult>
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly IUserRepository _userRepository;

        public LoginUserQueryHandler(SignInManager<UserModel> signInManager, UserManager<UserModel> userManager, IUserRepository userRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<LoginResult> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.LoginUserDTO.Email);
            if (user == null)
            {
                return CreateLoginResult(false, "User not found.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.LoginUserDTO.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var token = await _userRepository.GenerateJwtTokenAsync(user);
                return CreateLoginResult(true, null, token);
            }
            else
            {
                return CreateLoginResult(false, "Invalid login attempt.");
            }
        }

        private LoginResult CreateLoginResult(bool successful, string? error, (string,string)? token = null)
        {
            var (accessToken, refreshToken) = token.Value;
            return new LoginResult { Successful = successful, Error = error, AccessToken = accessToken, RefreshToken = refreshToken };
        }
    }
}
