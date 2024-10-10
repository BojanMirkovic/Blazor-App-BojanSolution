using System.Security.Claims;
using Application_Layer.Commands.DeleteUser;
using Application_Layer.Controllers;
using Application_Layer.Queries.GetUserByEmail;
using Domain_Layer.Models.User;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Test_Layer.UserTest.IntegrationTests
{
    [TestFixture]
    public class UserControllerIntegrationDeleteUserTest
    {
        private IMediator _mediator;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            _mediator = A.Fake<IMediator>();
            _controller = new UserController(_mediator);

            var expiration = new DateTimeOffset(DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds();


            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.Email, "logged-in-user@example.com"),
             new Claim(ClaimTypes.Role, "Admin"),
             new Claim("exp", expiration.ToString()),
             new Claim("iss", "your-issuer"),
             new Claim("aud", "your-audience")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var expectedUser = new UserModel
            {
                Id = "logged-in-user-id",
                UserName = "logged-in-user@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = "Admin"
            };

            A.CallTo(() => _mediator.Send(A<GetUserByEmailQuery>.That.Matches(c => c.Email == "logged-in-user@example.com"), A<CancellationToken>._))
                .Returns(expectedUser);
        }

        [Test]
        public async Task DeleteUser_ReturnsOk_WhenUserSuccessfullyDeleted()
        {
            // Arrange
            var userId = "logged-in-user-id";
            A.CallTo(() => _mediator.Send(A<GetUserByEmailQuery>.That.Matches(c => c.Email == "logged-in-user@example.com"), A<CancellationToken>._))
                .Returns(new UserModel { Id = userId, Email = "logged-in-user@example.com" });

            A.CallTo(() => _mediator.Send(A<DeleteUserCommand>.That.Matches(c => c.UserId == userId), A<CancellationToken>._))
                .Returns(true);

            // Act
            var actionResult = await _controller.DeleteUser(userId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<OkObjectResult>(actionResult);
            Assert.That(actionResult.Value, Is.EqualTo("User's account is successfuly deleted"));
        }


        [Test]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserDeletionFails()
        {
            // Arrange
            var userId = "logged-in-user-id";
            A.CallTo(() => _mediator.Send(A<GetUserByEmailQuery>.That.Matches(c => c.Email == "logged-in-user@example.com"), A<CancellationToken>._))
                .Returns(new UserModel { Id = userId, Email = "logged-in-user@example.com" });

            A.CallTo(() => _mediator.Send(A<DeleteUserCommand>.That.Matches(c => c.UserId == userId), A<CancellationToken>._))
                .Returns(false);

            // Act
            var actionResult = await _controller.DeleteUser(userId) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(actionResult);
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            Assert.That(actionResult.Value, Is.EqualTo("Failed to delete user's account."));
        }




        [Test]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserIdIsEmpty()
        {
            // Act
            var actionResult = await _controller.DeleteUser(string.Empty) as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            Assert.That(actionResult.Value, Is.EqualTo("The user ID is missing or invalid. Please ensure you are authenticated properly."));
        }


        [Test]
        public async Task DeleteUser_ReturnsForbidden_WhenUserDoesNotHavePermission()
        {
            // Arrange
            var userId = "existing-user-id";
            A.CallTo(() => _mediator.Send(A<GetUserByEmailQuery>.That.Matches(c => c.Email == "logged-in-user@example.com"), A<CancellationToken>._))
                .Returns(new UserModel { Id = "logged-in-user-id", Role = "User" });

            // Act
            var actionResult = await _controller.DeleteUser(userId) as ForbidResult;

            // Assert
            Assert.IsInstanceOf<ForbidResult>(actionResult);
        }

    }
}
