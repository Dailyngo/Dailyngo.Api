using System.Runtime.InteropServices.JavaScript;
using EveryDaily.Api.Controllers;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.ControllerCommands.Auth.Queries;
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
            _controller = new AuthController(_mediatorMock.Object); // Mock objeden gerçek nesne oluşturulmalı
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
            var resut = await _controller.Login(request) as ObjectResult;
            var response = resut.Value as Response<LoginResponse>;
            // Assert
            Assert.IsNotNull(response);
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(response.Data);
            Assert.That(response.Data.Token, Is.EqualTo("token"));
            Assert.That(response.Data.RefreshToken, Is.EqualTo("refresh"));
            Assert.IsTrue(response.IsSuccessful);
        }

        [Test]
        public async Task RefreshToken_ValidRequest_ReturnsToken()
        {
            // Arrange
            var command = new RefreshTokenCommand { RefreshToken = "refresh" };

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = true,
                Data = new LoginResponse
                {
                    Token = "new_access_token",
                    RefreshToken = "new_refresh_token",
                    IsSuccess = true
                },
                StatusCode = 200
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.RefreshToken(command) as ObjectResult;
            var response = result.Value as Response<LoginResponse>;

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsTrue(response.IsSuccessful);
            Assert.AreEqual("new_access_token", response.Data.Token);
            Assert.AreEqual("new_refresh_token", response.Data.RefreshToken);
        }

        [Test]
        public async Task EmailConfirmation_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            string email = "test@example.com";
            string token = "verification_token";

            var expectedResult = new Response<NoContent>
            {
                IsSuccessful = true,
                StatusCode = 200
            };


            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EmailVerifyQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.EmailConfirmation(email, token) as ObjectResult;
            var response = result.Value as Response<bool>;

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(response.Data);
        }


    }
}