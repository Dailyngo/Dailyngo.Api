using EveryDaily.Application.Services.ControllerCommands.Auth.Queries;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Test.Application.Auth.Queries
{
    [TestFixture]
    public class GetUserLoginInfoQueryTest
    {
        private Mock<IUserService> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
        }

        [Test]
        public async Task Handle_UserWithAboutAndConfirmedEmail_ReturnsCorrectStatus()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var department = new DepartmentEntity 
            {
                Name = "Test Department" 
            };
            await appDbContext.Departments.AddAsync(department);

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                EmailConfirmed = true,
                IsDeleted = false
            };
            await appDbContext.Users.AddAsync(userEntity);

            var about = new AboutEntity
            {
                UserId = userEntity.Id,
                BirthDate = DateTime.Now,
                DepartmentId = department.Id
            };
            await appDbContext.Abouts.AddAsync(about);

            await appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var handler = new GetUserLoginInfoQueryHandler(appDbContext, _userServiceMock.Object);
            var result = await handler.Handle(new GetUserLoginInfoQuery(), CancellationToken.None);

            Assert.True(result.IsSuccessful);
            Assert.AreEqual(200, result.StatusCode);
            Assert.True(result.Data.IsRegistered);
        }

        [Test]
        public async Task Handle_UserWithoutAboutAndUnconfirmedEmail_ReturnsFalseFlags()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                EmailConfirmed = false,
                IsDeleted = false
            };
            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var handler = new GetUserLoginInfoQueryHandler(appDbContext, _userServiceMock.Object);

            // Act
            var result = await handler.Handle(new GetUserLoginInfoQuery(), CancellationToken.None);

         
                Assert.That(result.IsSuccessful, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(result.Data.IsRegistered, Is.False);
                Assert.That(result.Data.IsEmailConfirmed, Is.False);
           
        }
        [Test]
        public async Task Handle_UserWithAboutAndUnConfirmedEmail_ReturnsCorrectStatus()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var department = new DepartmentEntity
            {
                Name = "Test Department"
            };
            await appDbContext.Departments.AddAsync(department);

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                EmailConfirmed = false,
                IsDeleted = false
            };
            await appDbContext.Users.AddAsync(userEntity);

            var about = new AboutEntity
            {
                UserId = userEntity.Id,
                BirthDate = DateTime.Now,
                DepartmentId = department.Id
            };
            await appDbContext.Abouts.AddAsync(about);

            await appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var handler = new GetUserLoginInfoQueryHandler(appDbContext, _userServiceMock.Object);
            var result = await handler.Handle(new GetUserLoginInfoQuery(), CancellationToken.None);

            Assert.True(result.IsSuccessful);
            Assert.AreEqual(200, result.StatusCode);
          
        }

        [Test]
        public async Task Handle_UserWithoutAboutAndConfirmedEmail_ReturnsCorrectStatus()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                EmailConfirmed = true,
                IsDeleted = false
            };
            await appDbContext.Users.AddAsync(userEntity);

            await appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var handler = new GetUserLoginInfoQueryHandler(appDbContext, _userServiceMock.Object);
            var result = await handler.Handle(new GetUserLoginInfoQuery(), CancellationToken.None);

            Assert.True(result.IsSuccessful);
            Assert.AreEqual(200, result.StatusCode);

        }

       
    }
}
