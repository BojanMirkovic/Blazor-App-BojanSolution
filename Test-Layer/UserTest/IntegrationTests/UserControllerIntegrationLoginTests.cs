using Application_Layer.Commands.RegisterNewUser;
using Application_Layer.Controllers;
using Application_Layer.Queries.LoginUser;
using Domain_Layer.Models.User;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared_Layer.DTO_s.User;

namespace Test_Layer.UserTest.IntegrationTests
{
    [TestFixture]
    public class UserControllerIntegrationLoginTests
    {
        private IMediator _mediator;
        private UserController _userController;

        [SetUp]
        public void SetUp()
        {
            _mediator = A.Fake<IMediator>();
            _userController = new UserController(_mediator);

            // Registrera en användare som används i inloggnings- och andra tester
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Role = "Student"
            };

            var expectedUser = new UserModel
            {
                UserName = registerUserDTO.Email,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName,
                Role = registerUserDTO.Role
            };

            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(expectedUser);

            _userController.Register(registerUserDTO);
        }

        [Test]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Använda den redan registrerade användaren för inloggningsförsöket
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var expectedLoginResult = new LoginResult
            {
                Successful = true,
                AccessToken = "fake_token"
            };

            A.CallTo(() => _mediator.Send(A<LoginUserQuery>._, A<CancellationToken>._))
                .Returns(expectedLoginResult);

            // Act
            var actionResult = await _userController.Login(loginUserDTO) as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(actionResult);

            // Kontrollera att svaret innehåller en token
            var resultObject = actionResult.Value;
            var tokenProperty = resultObject.GetType().GetProperty("token");
            var tokenValue = tokenProperty.GetValue(resultObject, null) as string;
            Assert.NotNull(tokenValue);

            // Validera token-värdet
            Assert.That(tokenValue, Is.EqualTo(expectedLoginResult.AccessToken));
        }


        [Test]
        public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            // Arrange
            var mediator = A.Fake<IMediator>();
            var userController = new UserController(mediator);
            var loginUserDTO = new LoginUserDTO
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            };

            var loginResult = new LoginResult
            {
                Successful = false,
                Error = "Invalid login attempt.",
                AccessToken = null
            };

            A.CallTo(() => mediator.Send(A<LoginUserQuery>._, A<CancellationToken>._)).Returns(loginResult);

            // Act
            var actionResult = await userController.Login(loginUserDTO);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            var badRequestResult = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.Value, Is.EqualTo(loginResult.Error));
        }
        [Test]
        public async Task Login_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            // Konfigurera mediator att kasta ett undantag när LoginUserQuery skickas
            A.CallTo(() => _mediator.Send(A<LoginUserQuery>._, A<CancellationToken>._))
                .Throws(new Exception("Simulated exception"));

            // Act
            var actionResult = await _userController.Login(loginUserDTO);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            var badRequestResult = actionResult as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            // Kontrollera att felmeddelandet är det som förväntas
            Assert.That(badRequestResult.Value.ToString(), Is.EqualTo("Simulated exception"));
        }

    }
}
