using Application_Layer.Queries.LoginUser;
using Domain_Layer.Models.User;
using Infrastructure_Layer.Repositories.User;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
    {
        private readonly SignInManager<UserModel> _signInManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly IUserRepository _userRepository;

        public RefreshTokenCommandHandler(SignInManager<UserModel> signInManager, UserManager<UserModel> userManager, IUserRepository userRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
        }
        public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // Step 2: Find the user by the refresh token in the database
            var user = await _userRepository.FindUserByRefreshToken(request.Token.RefreshToken);
           
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return RefreshLoginResult( false, "Invalid or expired refresh token" );
            }

            // Step 3: Generate a new AccessToken and optionally a new RefreshToken
            var token = await _userRepository.GenerateJwtTokenAsync(user);
            return RefreshLoginResult(true, null, token);

        }
        private LoginResult RefreshLoginResult(bool successful, string? error, (string, string)? token = null)
        {
            var (newAccessToken, newRefreshToken) = token.Value;
            return new LoginResult { Successful = successful, Error = error, AccessToken = newAccessToken, RefreshToken = newRefreshToken };
        }
    }
}
