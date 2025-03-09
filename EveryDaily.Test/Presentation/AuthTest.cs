using System.Runtime.InteropServices.JavaScript;
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

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
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
            
            var _controller = new AuthController(_mediatorMock.Object); // Mock objeden gerçek nesne oluşturulmalı

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

    }
}