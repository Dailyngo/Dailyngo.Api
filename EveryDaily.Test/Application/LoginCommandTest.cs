using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EveryDaily.Api.Controllers;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using EveryDaily.Test.DbContextMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace EveryDaily.Test.Application
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private Mock<SignInManager<UserEntity>> _signInManagerMock;
        private Mock<JwtTokenGenerator> _jwtTokenGeneratorMock;
        private Mock<UserManager<UserEntity>> _userManagerMock;

        private LoginCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            // Mock'lar oluşturuluyor

            // UserManager mock'lama
            _userManagerMock = new Mock<UserManager<UserEntity>>(
                Mock.Of<IUserStore<UserEntity>>(),
                null, null, null, null, null, null, null, null);

            // SignInManager mock'lama
            _signInManagerMock = new Mock<SignInManager<UserEntity>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<UserEntity>>(),
                null, null, null, null);

            _jwtTokenGeneratorMock = new Mock<JwtTokenGenerator>(null, null, null, null, _userManagerMock.Object);

        }

        [Test]
        public async Task Handle_ValidLoginCommand_ReturnsToken()
        {
            // Arrange
            var command = new LoginCommand
            {
                EmailOrUserName = "testuser@example.com",
                Password = "testpassword"
            };

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "testuser@example.com",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.Now
            };

            await using var appDbContext = new AppDbContext(InMemoryDbContextOptionsFactory.CreateDbContextOptions());
            // Handler oluşturuluyor
            _handler = new LoginCommandHandler(
                appDbContext,
                _signInManagerMock.Object,
                _jwtTokenGeneratorMock.Object
            );
            
            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
                .Returns("generated-token");
            
            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateRefreshToken(It.IsAny<UserEntity>()))
                .ReturnsAsync("refresh-token");
            
            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(result.Data);
            Assert.That(result.Data.Token, Is.EqualTo("generated-token"));
            Assert.That(result.Data.RefreshToken, Is.EqualTo("refresh-token"));
        }
    }

}

