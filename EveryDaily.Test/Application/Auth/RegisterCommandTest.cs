using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EveryDaily.Application.Dtos.Auth.Request;
using EveryDaily.Application.Services.ControllerCommands.Auth.Commands;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace EveryDaily.Test.Application.Auth;

[TestFixture]
public class RegisterCommandHandlerTests
{

    private Mock<UserManager<UserEntity>> _userManagerMock;
    private RegisterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {

        // Mock UserManager
        _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();


    }

    [Test]
    public async Task Handle_EmailDomainInvalid_ReturnsFailureWithMessage()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            UserName = "testuser",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Name = "Test",
            Surname = "User"
        };
        var command = new RegisterCommand { Data = request };

        await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        _handler = new RegisterCommandHandler(_appDbContext, _userManagerMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.messages, Is.EqualTo(AuthErrorMessage.InvalidEmailAddress));
        });
    }

    [Test]
    public async Task Handle_InvalidEmailFormat_ReturnsFailureWithMessage()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "invalid-email.edu.tr",
            UserName = "testuser",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Name = "Test",
            Surname = "User"
        };
        var command = new RegisterCommand { Data = request };

        await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        _handler = new RegisterCommandHandler(_appDbContext, _userManagerMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.messages, Is.EqualTo(AuthErrorMessage.InvalidEmailFormat));
        });
    }

    [Test]
    public async Task Handle_PasswordsDoNotMatch_ReturnsFailureWithMessage()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@edu.tr",
            UserName = "testuser",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            Name = "Test",
            Surname = "User"
        };
        var command = new RegisterCommand { Data = request };

        await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        _handler = new RegisterCommandHandler(_appDbContext, _userManagerMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.messages, Is.EqualTo(AuthErrorMessage.PasswordsDoNotMatch));
        });
    }

    [Test]
    public async Task Handle_UsernameExists_ReturnsFailureWithMessage()
    {
        var userEntity = new UserEntity
        {
            Name = "Test",
            Surname = "User",
            UserName = "existinguser",
            Email = "testuser@example.com",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.Now
        };

        await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

        await _appDbContext.Users.AddAsync(userEntity);
        await _appDbContext.SaveChangesAsync();

        _handler = new RegisterCommandHandler(_appDbContext, _userManagerMock.Object);
        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@edu.tr",
            UserName = "existinguser",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Name = "Test",
            Surname = "User"
        };
        var command = new RegisterCommand { Data = request };



        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.messages, Is.EqualTo(AuthErrorMessage.UserNameAlreadyExists));
        });
    }


    [Test]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {

        // Arrange
        var request = new RegisterRequest
        {
            Email = "valid@edu.tr",
            UserName = "newuser",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            Name = "Test",
            Surname = "User"
        };
        var command = new RegisterCommand { Data = request };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);


        await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
        _handler = new RegisterCommandHandler(_appDbContext, _userManagerMock.Object);
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {

            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));

        });


    }
}