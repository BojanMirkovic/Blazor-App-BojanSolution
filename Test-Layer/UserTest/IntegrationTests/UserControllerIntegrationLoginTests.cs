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
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful_1()
        {
            // Arrange: use the already registered user for login attempt
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
            Assert.IsNotNull(actionResult); // Ensure the action result is not null

            // Check that the response contains a token
            var resultObject = actionResult.Value;
            Assert.IsNotNull(resultObject, "The response object is null."); // Ensure the response object is not null

            // Ensure the object has a property named "AccessToken"
            var accessTokenProperty = resultObject.GetType().GetProperty("AccessToken");
            Assert.IsNotNull(accessTokenProperty, "The response object does not contain an 'AccessToken' property.");

            // Retrieve and check the token value
            var tokenValue = accessTokenProperty.GetValue(resultObject, null) as string;
            Assert.IsNotNull(tokenValue, "The 'AccessToken' property is null.");
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
