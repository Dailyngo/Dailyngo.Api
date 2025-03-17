using EveryDaily.Application.Services.ControllerCommands.User.Queries;
using EveryDaily.Domain.Entities;
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
    internal class SearchUsersTest
    {
        private Mock<UserManager<UserEntity>> _userManagerMock;
        private SearchUsersQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            // Mock UserManager
            _userManagerMock = SetupDefaultMoq.CreateUserManagerMock();
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

            _handler = new SearchUsersQueryHandler(_appDbContext);
            // Arrange
            var request = new SearchUsersQuery { SearchTerm = "Test" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task Handle_SearchTermMatchesSurname_ReturnsMatchingUsers()
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

            _handler = new SearchUsersQueryHandler(_appDbContext);

            var request = new SearchUsersQuery { SearchTerm = "User" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(3));
                
            });
        }
        [Test]
        public async Task Handle_SearchTermMatchesUsername_ReturnsMatchingUsers()
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

            _handler = new SearchUsersQueryHandler(_appDbContext);

            var request = new SearchUsersQuery { SearchTerm = "user2" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(1));
                
            });
        }
        [Test]
        public async Task Handle_SearchTermDoesNotMatch_ReturnsEmptyList()
        {

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                UserName = "existinguser",
                Email = ""
            };
            await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            await _appDbContext.Users.AddAsync(userEntity);
            await _appDbContext.SaveChangesAsync();

            _handler = new SearchUsersQueryHandler(_appDbContext);

            var request = new SearchUsersQuery { SearchTerm = "NonExistent" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(0));
            });
        }
        [Test]
        public async Task Handle_SearchTermEmpty_ReturnsEmptyList()
        {

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                UserName = "existinguser",
                Email = ""
            };
            await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            await _appDbContext.Users.AddAsync(userEntity);
            await _appDbContext.SaveChangesAsync();

            _handler = new SearchUsersQueryHandler(_appDbContext);

            var request = new SearchUsersQuery { SearchTerm = "" };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(0));
               
            });
        }
        [Test]
        public async Task Handle_SearchTermNull_ReturnsEmptyList()
        {

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                UserName = "existinguser",
                Email = ""
            };
            await using var _appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            await _appDbContext.Users.AddAsync(userEntity);
            await _appDbContext.SaveChangesAsync();

            _handler = new SearchUsersQueryHandler(_appDbContext);

            var request = new SearchUsersQuery { SearchTerm = null };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.Data.Count, Is.EqualTo(0));
            });
        }
     
    }
}
