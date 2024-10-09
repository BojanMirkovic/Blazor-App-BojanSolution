using AutoMapper;
using Domain_Layer.Models.User;
using Infrastructure_Layer.Repositories.User;
using MediatR;

namespace Application_Layer.Commands.RegisterNewUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserModel>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }
        public async Task<UserModel> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (request.NewUser == null)
            {
                throw new ArgumentNullException("Invalid user data. FirstName,LastName,Email and Password are required.");
            }
            var existingUser = await _userRepository.GetUserByEmailAsync(request.NewUser.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }
            try
            {
                var userToCreate = _mapper.Map<UserModel>(request.NewUser);

                var createdUser = await _userRepository.RegisterUserAsync(userToCreate, request.NewUser.Password, request.NewUser.Role);

                return createdUser;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
