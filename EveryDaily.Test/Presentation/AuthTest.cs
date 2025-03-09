using EveryDaily.Api.Controllers;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Core.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EveryDaily.Test.Presentation
{
    [TestFixture]
    public class AuthTest
    {
        private Mock<IMediator> _mediatorMock;
        private AuthController _controller;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object);
        }

        public async Task Login_ValidRequest_ReturnsToken()
        {

            // Arrange
            var request = new LoginRequest
            {
                EmailOrUserName = "testuser",
                Password = "testpassword"
            };

            var command = new LoginCommand
            {
                EmailOrUserName = request.EmailOrUserName,
                Password = request.Password
            };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = true,
                Data = new LoginResponse
                {
                    ErrorMessage = null,
                    IsRegistered = true,
                    RefreshToken = "refresh",
                    IsSuccess = true,
                    Token = "token"
                }
            };

            var cancellationToken = new CancellationToken();
            _mediatorMock
                .Setup(m => m.Send(command, default))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request) as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(result.Data);
            Assert.That(result.Data.Token, Is.EqualTo("token"));
            Assert.That(result.Data.RefreshToken, Is.EqualTo("refresh"));
            Assert.IsTrue(result.IsSuccessful);
        }
    }
}
