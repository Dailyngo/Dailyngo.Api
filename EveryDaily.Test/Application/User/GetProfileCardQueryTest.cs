using EveryDaily.Application.Services.ControllerCommands.User.Queries;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Domain.Prefix.ErrorMessage;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Test.Application.User
{
    internal class GetProfileCardQueryTest
    {
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private Mock<IUserService> _userServiceMock;
        private GetProfileCardQueryHandler _handler;


        [SetUp]
        public void Setup()
        {
            // Mock UserManager
            _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();
            _userServiceMock = new Mock<IUserService>();
            
        }

        [Test]
        public async Task Handle_SearchTermMatchesName_ReturnsMatchingUsers()
        {
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Name = "Test",
                    Surname = "User",
                    UserName = "existinguser",

                },
                new UserEntity
                {
                    Name = "Test2",
                    Surname = "User2",
                    UserName = "existinguser2",

                },
                new UserEntity
                {
                    Name = "Test3",
                    Surname = "User3",
                    UserName = "existinguser3",

                }
            };

            
            await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            await _appDbContext.Users.AddRangeAsync(users);
            await _appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(users[1].Id);

            _handler = new GetProfileCardQueryHandler(_appDbContext,_userServiceMock.Object);
            var request = new GetProfileCardQuery();
            var result = await _handler.Handle(request, CancellationToken.None);

            

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.GetUserResponse.FullName, Is.EqualTo("Test2 User2"));
                Assert.That(result.Data.GetUserResponse.UserName, Is.EqualTo("existinguser2"));
            });
        }
        [Test]
        public async Task Handle_UserNotFound_ReturnsFail()
        {
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Name = "Test",
                    Surname = "User",
                    UserName = "existinguser",

                },
                new UserEntity
                {
                    Name = "Test2",
                    Surname = "User2",
                    UserName = "existinguser2",

                },
                new UserEntity
                {
                    Name = "Test3",
                    Surname = "User3",
                    UserName = "existinguser3",

                }
            };

            await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            await _appDbContext.Users.AddRangeAsync(users);
            await _appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid);

            _handler = new GetProfileCardQueryHandler(_appDbContext, _userServiceMock.Object);
            var request = new GetProfileCardQuery();
            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.False);
                Assert.AreEqual(result.messages, UserErrorMessage.ProfileDetailNotFound);
            });
        }
    }
}
