using Application_Layer.Queries.LoginUser;
using Domain_Layer.Models.User;
using FakeItEasy;
using Infrastructure_Layer.Repositories.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shared_Layer.DTO_s.User;

namespace Test_Layer.UserTest.UnitTests.UserQueryTests
{
    [TestFixture]
    public class LoginUserQueryHandlerTests
    {
        private UserManager<UserModel> _userManager;
        private SignInManager<UserModel> _signInManager;
        private IUserRepository _userRepository;
        private LoginUserQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            var userStore = A.Fake<IUserStore<UserModel>>();
            _userManager = A.Fake<UserManager<UserModel>>(
                options => options.WithArgumentsForConstructor(() => new UserManager<UserModel>(
                    userStore, null, null, null, null, null, null, null, null)));
            var claimsFactory = A.Fake<IUserClaimsPrincipalFactory<UserModel>>();
            _signInManager = A.Fake<SignInManager<UserModel>>(
                options => options.WithArgumentsForConstructor(() => new SignInManager<UserModel>(
                    _userManager, A.Fake<IHttpContextAccessor>(), claimsFactory, null, null, null, null)));
            _userRepository = A.Fake<IUserRepository>();

            _handler = new LoginUserQueryHandler(_signInManager, _userManager, _userRepository);
        }

        [Test]
        public async Task Handle_SuccessfulLogin_ReturnsSuccessToken()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO { Email = "user@example.com", Password = "Password123" };
            var query = new LoginUserQuery(loginUserDTO);

            var user = new UserModel { Email = loginUserDTO.Email };

            _handler = new LoginUserQueryHandler(_signInManager, _userManager, _userRepository);

            A.CallTo(() => _userManager.FindByEmailAsync(A<string>.Ignored))
                .Returns(user);
            A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user, A<string>.Ignored, false))
                .Returns(SignInResult.Success);
            A.CallTo(() => _userRepository.GenerateJwtTokenAsync(A<UserModel>.Ignored))
                .Returns("TestToken");

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Successful);
            Assert.IsNotNull(result.AccessToken);
            Assert.IsNull(result.Error);
        }

        [Test]
        public async Task Handle_InvalidCredentials_ReturnsFailure()
        {
            var loginUserDTO = new LoginUserDTO { Email = "user@example.com", Password = "WrongPassword" };
            var query = new LoginUserQuery(loginUserDTO);

            var user = new UserModel { Email = loginUserDTO.Email };

            A.CallTo(() => _userManager.FindByEmailAsync(A<string>.Ignored))
                .Returns(user);
            A.CallTo(() => _signInManager.CheckPasswordSignInAsync(user, A<string>.Ignored, false))
                .Returns(SignInResult.Failed);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Successful);
            Assert.IsNull(result.AccessToken);
            Assert.That(result.Error, Is.EqualTo("Invalid login attempt."));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO { Email = "nonexistent@example.com", Password = "Password123" };
            var query = new LoginUserQuery(loginUserDTO);

            A.CallTo(() => _userManager.FindByEmailAsync(A<string>.Ignored))
                .Returns((UserModel)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.IsNull(result.AccessToken);
            Assert.That(result.Error, Is.EqualTo("User not found."));
        }
    }
}
