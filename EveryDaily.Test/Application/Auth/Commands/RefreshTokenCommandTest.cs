using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Enums;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EveryDaily.Test.Application.Auth.Commands
{
    internal class RefreshTokenCommandTest
    {
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private Mock<JwtTokenGenerator> _jwtTokenGeneratorMock;
        private Mock<ILogger<RefreshTokenHandler>> _loggerMock;
        private RefreshTokenHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();
            _jwtTokenGeneratorMock = new Mock<JwtTokenGenerator>(null, null, null, null, _userManagerMock.Object);
            _loggerMock = new Mock<ILogger<RefreshTokenHandler>>();

            
        }

        [Test]
        public async Task Handle_ValidRefreshToken_ReturnsNewToken()
        {
           var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "testuser@example.com",
                UserName = "testuser",
                Name = "Test",
                Surname = "User"
            };
            _appDbContext.Users.Add(userEntity);
            _appDbContext.SaveChanges();

            _jwtTokenGeneratorMock
                .Setup(x => x.VerifyToken(command.RefreshToken, JwtTokenType.RefreshToken))
                .ReturnsAsync(new ValidateTokenResult(true, string.Empty, userEntity.Id.ToString(), "valid-token"));

            _jwtTokenGeneratorMock
                .Setup(x => x.GetClaim(command.RefreshToken, JwtRegisteredClaimNames.Sub))
                .Returns(userEntity.Id.ToString());


            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
                .ReturnsAsync("new-access-token");

            _jwtTokenGeneratorMock
                .Setup(x => x.GenerateRefreshToken(It.IsAny<UserEntity>()))
                .ReturnsAsync("new-refresh-token");

            var _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            _handler = new RefreshTokenHandler(
                _userManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _loggerMock.Object
            );


            _userManagerMock.Setup(x => x.Users).Returns(_appDbContext.Users);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

        
            var result = await _handler.Handle(command, CancellationToken.None);

   
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Data.Token, Is.EqualTo("new-access-token"));
            Assert.That(result.Data.RefreshToken, Is.EqualTo("new-refresh-token"));
        }

        [Test]
        public async Task Handle_InvalidRefreshToken_ReturnsUnauthorized()
        {
            var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var command = new RefreshTokenCommand
            {
                RefreshToken = "invalid-refresh-token"
            };

            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "testuser@example.com",
                UserName = "testuser",
                Name = "Test",
                Surname = "User"
            };
            _appDbContext.Users.Add(userEntity);
            _appDbContext.SaveChanges();

            _jwtTokenGeneratorMock
                .Setup(x => x.VerifyToken(command.RefreshToken, JwtTokenType.RefreshToken))
                .ReturnsAsync(new ValidateTokenResult(false, "Invalid token"));

            var _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            _handler = new RefreshTokenHandler(
                _userManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _loggerMock.Object
            );

            _userManagerMock.Setup(x => x.Users).Returns(_appDbContext.Users);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(401));

        }

        [Test]
        public async Task Handle_ExpiredToken_ReturnsUnauthorized()
        {
            var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var command = new RefreshTokenCommand
            {
                RefreshToken = "expired-refresh-token"
            };

            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "testuser@example.com",
                UserName = "testuser",
                Name = "Test",
                Surname = "User"
            };
            _appDbContext.Users.Add(userEntity);
            _appDbContext.SaveChanges();

            _jwtTokenGeneratorMock
                .Setup(x => x.VerifyToken(command.RefreshToken, JwtTokenType.RefreshToken))
                .ReturnsAsync(new ValidateTokenResult(false, "Token has expired! Please login to get a new token!"));

            var _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            _handler = new RefreshTokenHandler(
                _userManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _loggerMock.Object
            );

            _userManagerMock.Setup(x => x.Users).Returns(_appDbContext.Users);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(401));
        }

        [Test]
        public async Task Handle_TokenAndUserIdMismatch_ReturnsUnauthorized()
        {
            var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var command = new RefreshTokenCommand
            {
                RefreshToken = "mismatched-refresh-token"
            };

            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "testuser@example.com",
                UserName = "testuser",
                Name = "Test",
                Surname = "User"
            };

            _appDbContext.Users.Add(userEntity);
            _appDbContext.SaveChanges();

            _jwtTokenGeneratorMock
                .Setup(x => x.VerifyToken(command.RefreshToken, JwtTokenType.RefreshToken))
                .ReturnsAsync(new ValidateTokenResult(true, string.Empty, "invalid-user-id", "valid-token"));

            var _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            _handler = new RefreshTokenHandler(
                _userManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _loggerMock.Object
            );

            _userManagerMock.Setup(x => x.Users).Returns(_appDbContext.Users);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(userEntity);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(401));
        }
       /* [Test]
        public async Task Handle_UserNotFound_ReturnsNotFound()
        {

            var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var nonExistingUserId = Guid.NewGuid();

            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token",
            };

            _jwtTokenGeneratorMock
                .Setup(x => x.VerifyToken(command.RefreshToken, JwtTokenType.RefreshToken))
                .ReturnsAsync(new ValidateTokenResult(true, string.Empty, nonExistingUserId.ToString(), "valid-token"));

            var _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();

            _handler = new RefreshTokenHandler(
                _userManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _loggerMock.Object
            );


            var users = new List<UserEntity>();
            _userManagerMock.Setup(x => x.Users).Returns(_appDbContext.Users);


            _userManagerMock
                .Setup(x => x.FindByIdAsync(nonExistingUserId.ToString()))
                .ReturnsAsync((UserEntity)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            // Sonuçları kontrol ediyoruz
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));

        }*/




    }
}
