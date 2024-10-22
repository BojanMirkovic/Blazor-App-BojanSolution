
using Application_Layer.Queries.LoginUser;
using MediatR;
using Shared_Layer.DTO_s.User;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<LoginResult>
    { 
        public RefreshTokenRequestDTO Token {  get; set; }
        public RefreshTokenCommand(RefreshTokenRequestDTO refreshToken) { Token = refreshToken; }
    }
}
