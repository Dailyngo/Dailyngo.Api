using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EveryDaily.Api.Controllers;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Dtos.Auth.Response;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using MassTransit;
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

namespace EveryDaily.Test.Application.Auth
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private Mock<SignInManager<UserEntity>> _signInManagerMock;
        private Mock<JwtTokenGenerator> _jwtTokenGeneratorMock;
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private Mock<IBusControl> _busControlMock;
        private Mock<ICacheService> _cacheServiceMock;

        private LoginCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            // Mock'lar oluşturuluyor

            // UserManager mock'lama
            _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            // SignInManager mock'lama
            _signInManagerMock = new Mock<SignInManager<UserEntity>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<UserEntity>>(),
                null, null, null, null);

            _jwtTokenGeneratorMock = new Mock<JwtTokenGenerator>(null, null, null, null, _userManagerMock.Object);

            _busControlMock = new Mock<IBusControl>();
            _cacheServiceMock = new Mock<ICacheService>();



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

            await using var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
            // Handler oluşturuluyor
            _handler = new LoginCommandHandler(
                appDbContext,
                _signInManagerMock.Object,
                _jwtTokenGeneratorMock.Object

            );

            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _busControlMock
                .Setup(x => x.Publish(It.IsAny<EmailSendingMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cacheServiceMock
                .Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _cacheServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
                .ReturnsAsync("generated-token");

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
        [Test]
        public async Task Handle_WrongPassword_Returns401()
        {
            var command = new LoginCommand
            {
                EmailOrUserName = "testuser@example.com",
                Password = "wrongpassword"
            };

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "testuser@example.com",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.Now
            };

            await using var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
            // Handler oluşturuluyor
            _handler = new LoginCommandHandler(
                 appDbContext,
                 _signInManagerMock.Object,
                 _jwtTokenGeneratorMock.Object


             );

            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _busControlMock
               .Setup(x => x.Publish(It.IsAny<EmailSendingMessage>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            _cacheServiceMock
                .Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _cacheServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.AreEqual(result.messages, AuthErrorMessage.InvalidPassword);


        }

        [Test]
        public async Task Handle_WrongUsername_Returns400()
        {
            var command = new LoginCommand
            {
                EmailOrUserName = "wronguser@example.com",
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

            await using var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
            // Handler oluşturuluyor
            _handler = new LoginCommandHandler(
                 appDbContext,
                 _signInManagerMock.Object,
                 _jwtTokenGeneratorMock.Object

             );

            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _busControlMock
               .Setup(x => x.Publish(It.IsAny<EmailSendingMessage>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            _cacheServiceMock
                .Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _cacheServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.AreEqual(result.messages, AuthErrorMessage.UserNotFound);



        }
    }
}

