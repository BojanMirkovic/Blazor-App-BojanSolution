using Application_Layer.Commands.RegisterNewUser;
using AutoFixture;
using AutoMapper;
using Domain_Layer.Models.User;
using FakeItEasy;
using Infrastructure_Layer.Repositories.User;
using Shared_Layer.DTO_s.User;

namespace Test_Layer.UserTest.UnitTests.UserCommandTests
{
    [TestFixture]
    public class RegisterUserCommandHandlerTests
    {
        private IUserRepository _userRepository;
        private IMapper _mapper;
        private RegisterUserCommandHandler _handler;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _userRepository = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _handler = new RegisterUserCommandHandler(_userRepository, _mapper);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Handle_ValidNewUser_ReturnsRegisteredUser()
        {
            // Arrange
            var userDto = _fixture.Build<RegisterUserDTO>()
                                  .With(u => u.Email, "test@example.com")
                                  .With(u => u.Password, "Password123!")
                                  .With(u => u.ConfirmPassword, "Password123!")
                                  .With(u => u.Role, "Teacher")
                                  .Create();

            var command = new RegisterUserCommand(userDto);
            var newUser = _mapper.Map<UserModel>(userDto);
            var registeredUser = _fixture.Build<UserModel>()
                                         .With(u => u.Email, newUser.Email)
                                         .Create();
            // Mock the _userRepository.GetUserByEmailAsync to return null indicating no existing user
            A.CallTo(() => _userRepository.GetUserByEmailAsync(userDto.Email))!
                .Returns((UserModel?)null);

            A.CallTo(() => _mapper.Map<UserModel>(A<RegisterUserDTO>.That.IsEqualTo(userDto)))
                .Returns(newUser);
            A.CallTo(() => _userRepository.RegisterUserAsync(newUser, userDto.Password, userDto.Role))
                .Returns(registeredUser);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.That(result, Is.EqualTo(registeredUser));
            Assert.That(result.Id, Is.EqualTo(registeredUser.Id));
            Assert.That(result.FirstName, Is.EqualTo(registeredUser.FirstName));
            Assert.That(result.Email, Is.EqualTo(registeredUser.Email));
            Assert.That(result.Role, Is.EqualTo(registeredUser.Role));
            A.CallTo(() => _userRepository.RegisterUserAsync(A<UserModel>.That.Matches(u => u.Email == newUser.Email && u.FirstName == newUser.FirstName && u.LastName == newUser.LastName), userDto.Password, userDto.Role))
                .MustHaveHappenedOnceExactly();
        }
        [Test]
        public void Handle_InvalidNewUser_ThrowsArgumentNullException()
        {
            // Arrange
            var command = new RegisterUserCommand(null);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(command, default));
        }
        [Test]
        public void Handle_FailedRegistration_ThrowsException()
        {
            // Arrange
            var userDto = _fixture.Build<RegisterUserDTO>()
                              .With(u => u.Email, "test@example.com")
                              .With(u => u.Password, "wrongPasswordNotImplementingNumbersAndSpecCaracters")
                              .With(u => u.ConfirmPassword, "wrongPasswordNotImplementingNumbersAndSpecCaracters")
                              .With(u => u.Role, "Teacher")
                              .Create();
            var command = new RegisterUserCommand(userDto);
            var newUser = _mapper.Map<UserModel>(userDto);
            // Mock the _userRepository.GetUserByEmailAsync to return null indicating no existing user
            A.CallTo(() => _userRepository.GetUserByEmailAsync(userDto.Email))!
                .Returns((UserModel?)null);
            A.CallTo(() => _mapper.Map<UserModel>(A<RegisterUserDTO>.That.IsEqualTo(userDto)))
                .Returns(newUser);
            A.CallTo(() => _userRepository.RegisterUserAsync(newUser, userDto.Password, userDto.Role))
                .Throws<Exception>();

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, default));
        }
        [Test]
        public void Handle_FailedRegistration_ThrowsInvalidOperationException()
        {
            // Arrange
            var userDto = _fixture.Build<RegisterUserDTO>()
                              .With(u => u.Email, "test@example.com")
                              .With(u => u.Password, "Test123!")
                              .With(u => u.ConfirmPassword, "Test123!")
                              .With(u => u.Role, "Teacher")
                              .Create();
           
            var command = new RegisterUserCommand(userDto);
            var newUser = _mapper.Map<UserModel>(userDto);
            var existingUser = _fixture.Build<UserModel>()
                                       .With(u => u.Email, newUser.Email)
                                       .Create();
            // Mock the _userRepository.GetUserByEmailAsync to return existing user Email
            A.CallTo(() => _userRepository.GetUserByEmailAsync(userDto.Email))!
                .Returns(existingUser);
            A.CallTo(() => _mapper.Map<UserModel>(A<RegisterUserDTO>.That.IsEqualTo(userDto)))
                .Returns(newUser);
            A.CallTo(() => _userRepository.RegisterUserAsync(newUser, userDto.Password, userDto.Role))
                .Throws<InvalidOperationException>();

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, default));
        }
    }
}
