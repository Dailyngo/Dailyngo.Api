using EveryDaily.Application.Dtos.Auth;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.ControllerCommands.Auth.Queries;
using EveryDaily.Application.Services.Jwt;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EveryDaily.Test.Application.Auth
{
    internal class EmailVerifyQueryTest
    {

        private Mock<IUserService> _userServiceMock;
        private Mock<ICacheService> _cacheServiceMock;

        private EmailVerifyQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {

            _userServiceMock = new Mock<IUserService>();
            _cacheServiceMock = new Mock<ICacheService>();

        }
        [Test]
        public async Task EmailVerify_ValidVerifyCode_UserEmailVerifyFlagTrue()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "testuser@example.com",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.Now,
                EmailConfirmed = false,
            };

            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();
            var confirmEmail = new ConfirmEmailDto
            {
                ConfirmatinToken = "123456",
            };
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);
            _cacheServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(confirmEmail));
            _cacheServiceMock.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _handler = new EmailVerifyQueryHandler(appDbContext, _cacheServiceMock.Object, _userServiceMock.Object);
            var result = await _handler.Handle(new EmailVerifyQuery { VerificationCode = "123456" }, CancellationToken.None);
            var user = appDbContext.Users.FirstOrDefault(x => x.Id == userEntity.Id);

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(user.EmailConfirmed, Is.True);




        }
        [Test]
        public async Task EmailVerify_InValidVerifyCode_UserEmailVerifyFlagTrue()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "testuser@example.com",
                IsDeleted = false,
                CreatedAt = DateTimeOffset.Now,
                EmailConfirmed = false,
            };

            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();
            var confirmEmail = new ConfirmEmailDto
            {
                ConfirmatinToken = "123456",
            };
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);
            _cacheServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(JsonSerializer.Serialize(confirmEmail));
            _cacheServiceMock.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            _handler = new EmailVerifyQueryHandler(appDbContext, _cacheServiceMock.Object, _userServiceMock.Object);
            var result = await _handler.Handle(new EmailVerifyQuery { VerificationCode = "123876" }, CancellationToken.None);
            var user = appDbContext.Users.FirstOrDefault(x => x.Id == userEntity.Id);

            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(user.EmailConfirmed, Is.False);




        }


    }
}
