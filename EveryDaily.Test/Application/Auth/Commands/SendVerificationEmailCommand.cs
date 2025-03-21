using System;
using System.Text.Json;
using System.Threading.Tasks;
using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Dtos.Auth;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace EveryDaily.Test.Application.Auth.Commands;

[TestFixture]
public class SendVerificationEmailCommandHandlerTests
{
    private Mock<IBusControl> _busControlMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<UserManager<UserEntity>> _userManagerMock;
    private Mock<ICacheService> _cacheServiceMock;


    [SetUp]
    public void Setup()
    {
        _busControlMock = new Mock<IBusControl>();
        _userServiceMock = new Mock<IUserService>();
        _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();
        _cacheServiceMock = new Mock<ICacheService>();

    }

   /* [Test]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

        var userId = Guid.NewGuid();

        _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new SendVerificationEmailCommandHandler(
            appDbContext,
            _busControlMock.Object,
            _userServiceMock.Object,
            _userManagerMock.Object,
            _cacheServiceMock.Object);

     
        // Act
        var result = await handler.Handle(new SendVerificationEmailCommand(), CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.messages, Is.EqualTo("User not found")); // Mesaj kontrolü
        });
    }
   */
    [Test]
    public async Task Handle_CacheExists_ReturnsExistingToken()
    {
        var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        var userEntity = new UserEntity
        {
            Name = "Test",
            Surname = "User",
            Email = "testuser@example.com",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.Now,
            EmailConfirmed = false,
        };
        await _appDbContext.Users.AddAsync(userEntity);
        await _appDbContext.SaveChangesAsync();

        _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);
        _cacheServiceMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        _cacheServiceMock.Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonSerializer.Serialize(new ConfirmEmailDto
            {
                EmailConfirmedDate = DateTimeOffset.UtcNow
            }));

        var handler = new SendVerificationEmailCommandHandler(
            _appDbContext,
            _busControlMock.Object,
            _userServiceMock.Object,
            _userManagerMock.Object,
            _cacheServiceMock.Object);


        var result = await handler.Handle(new SendVerificationEmailCommand(), CancellationToken.None);


        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Data.EmailConfirmedDate, Is.EqualTo(DateTimeOffset.UtcNow).Within(1).Seconds);
        });
    }

    [Test]
    public async Task Handle_CacheNotExists_GeneratesNewTokenAndSendsEmail()
    {
        var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        var userEntity = new UserEntity
        {
            Name = "Test",
            Surname = "User",
            Email = "testuser@example.com",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.Now,
            EmailConfirmed = false,
        };
        await _appDbContext.Users.AddAsync(userEntity);
        await _appDbContext.SaveChangesAsync();

        _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);
        _cacheServiceMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _userManagerMock.Setup(x => x.GenerateTwoFactorTokenAsync(
            It.IsAny<UserEntity>(),
            It.IsAny<string>()))
            .ReturnsAsync("123456");

        var handler = new SendVerificationEmailCommandHandler(
            _appDbContext,
            _busControlMock.Object,
            _userServiceMock.Object,
            _userManagerMock.Object,
            _cacheServiceMock.Object);

        var result = await handler.Handle(new SendVerificationEmailCommand(), CancellationToken.None);


        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(201));
            Assert.That(result.Data.EmailConfirmedDate, Is.GreaterThan(DateTimeOffset.UtcNow));
        });

    }
}