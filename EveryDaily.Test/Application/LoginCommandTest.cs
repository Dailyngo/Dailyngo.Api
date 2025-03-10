using System.Threading;
using System.Threading.Tasks;
using EveryDaily.Api.Controllers;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Core.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EveryDaily.Tests.Auth
{
    [TestFixture]
    public class LoginControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private AuthController _controller;
        

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object);
        }

        [Test]
        public async Task Login_ValidRequest_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "testuser",
                Password = "testpassword"
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = true,
                Data = new LoginResponse
                {
                    ErrorMessage = "",
                    IsRegistered = true,
                    RefreshToken = "refresh",
                    IsSuccess = true,
                    Token = "token",
                },
                StatusCode = 200
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.Token, Is.EqualTo("token"));
            Assert.That(response.Data.RefreshToken, Is.EqualTo("refresh"));
            Assert.IsTrue(response.IsSuccessful);
        }
        [Test]
        public async Task Login_InValidPassword_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "testuser",
                Password = "testpassword"
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = false,
                Data = new LoginResponse
                {
                    ErrorMessage = "Invalid password",
                    IsRegistered = false,
                    RefreshToken = null,
                    IsSuccess = false,
                    Token = null,
                },
                StatusCode = 400
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(400));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.ErrorMessage, Is.EqualTo("Invalid password"));
            Assert.That(response.Data.IsRegistered, Is.False);
            Assert.That(response.Data.Token, Is.Null);
            Assert.That(response.Data.RefreshToken, Is.Null);
            Assert.IsFalse(response.IsSuccessful);
        }
        [Test]
        public async Task Login_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "testuser",
                Password = "testpassword"
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = false,
                Data = new LoginResponse
                {
                    ErrorMessage = "User not found",
                    IsRegistered = false,
                    RefreshToken = null,
                    IsSuccess = false,
                    Token = null,
                },
                StatusCode = 400
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(400));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.ErrorMessage, Is.EqualTo("User not found"));
            Assert.That(response.Data.IsRegistered, Is.False);
            Assert.That(response.Data.Token, Is.Null);
            Assert.That(response.Data.RefreshToken, Is.Null);
            Assert.IsFalse(response.IsSuccessful);
        }


    }

}
