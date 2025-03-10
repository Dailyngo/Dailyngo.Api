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
        private Mock<DbSet<UserEntity>> _dbSetMock;
        private Mock<AppDbContext> _appDbContextMock;
        private Mock<SignInManager<UserEntity>> _signInManagerMock;
        private Mock<JwtTokenGenerator> _jwtTokenGeneratorMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private Mock<ILogger<JwtTokenGenerator>> _loggerMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<UserManager<UserEntity>> _userManagerMock;

        private LoginCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _dbSetMock = new Mock<DbSet<UserEntity>>();
            // Mock'lar oluşturuluyor
            _appDbContextMock = new Mock<AppDbContext>();

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

            _configurationMock = new Mock<IConfiguration>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _loggerMock = new Mock<ILogger<JwtTokenGenerator>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _jwtTokenGeneratorMock = new Mock<JwtTokenGenerator>(null, null, null, null, _userManagerMock.Object);

            // Handler oluşturuluyor
            _handler = new LoginCommandHandler(
                _appDbContextMock.Object,
                _signInManagerMock.Object,
                _jwtTokenGeneratorMock.Object
            );
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
                IsDeleted = false,
                CreatedAt = DateTimeOffset.Now
            };

            // Mock'ları setup et
            _userManagerMock
                .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<UserEntity>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
                .Returns("generated-token");

            _appDbContextMock.Setup(x => x.Users).Returns(_dbSetMock.Object);

            var users = new List<UserEntity>
        {
            new UserEntity { Email = "testuser@example.com", UserName = "testuser", Name = "Test", Surname = "User" }
        }.AsQueryable();

            // IQueryable için setup
            _dbSetMock.As<IQueryable<UserEntity>>()
                .Setup(m => m.Provider).Returns(users.Provider);
            _dbSetMock.As<IQueryable<UserEntity>>()
                .Setup(m => m.Expression).Returns(users.Expression);
            _dbSetMock.As<IQueryable<UserEntity>>()
                .Setup(m => m.ElementType).Returns(users.ElementType);
            _dbSetMock.As<IQueryable<UserEntity>>()
                .Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // FirstOrDefaultAsync metodunu manuel olarak mock'la
            _dbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(users.FirstOrDefault());

            var expectedResult = new Response<LoginResponse>
            {
                IsSuccessful = true,
                Data = new LoginResponse
                {
                    Token = "generated-token",
                    RefreshToken = "refresh-token",
                    IsSuccess = true,
                    ErrorMessage = "",
                    IsRegistered = true
                },
                StatusCode = 200
            };

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

