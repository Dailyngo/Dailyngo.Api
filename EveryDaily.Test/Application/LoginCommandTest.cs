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
        public async Task Login_InvalidUser_ReturnsError()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "invaliduser",
                Password = "testpassword"
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = false,
                StatusCode = 401,
                messages = "Invalid username"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(401));
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual("Invalid username", response.messages);
        }

        [Test]
        public async Task Login_InvalidPassword_ReturnsError()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "testuser",
                Password = "wrongpassword"
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = false,
                StatusCode = 401,
                messages = "Invalid password"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(401));
            Assert.IsFalse(response.IsSuccessful);
            Assert.AreEqual("Invalid password", response.messages);
        }
    }
}
