using EveryDaily.Application.Dtos.About.Request;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Application.Services.ControllerCommands.About.Commands;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using EveryDaily.Test.DefaultMoq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveryDaily.Test.Application.About.Commands
{
    internal class UpdateAboutCommandTest
    {

        private Mock<IUserService> _userServiceMock;
        

        private UpdateAboutCommand _handler;

        [SetUp]
        public void SetUp()
        {

            _userServiceMock = new Mock<IUserService>();
            

        }
        [Test]
        public async Task UpdateAbout_ValidUpdateAboutData_AboutUpdated()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com"
            };
            await appDbContext.Users.AddAsync(userEntity);

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var department = new DepartmentEntity
            {
                Name = "Test Department"
            };
            await appDbContext.Departments.AddAsync(department);

            var about = new AboutEntity
            {
                UserId = userEntity.Id,
                BirthDate = DateTime.Now,
                DepartmentId = department.Id
            };
            await appDbContext.Abouts.AddAsync(about);

            await appDbContext.SaveChangesAsync();

            var request = new UpdateAboutCommand
            {
                Data = new UpdateAboutRequest
                {
                    Id = about.Id,
                    BirthDate = DateTime.Now,
                    DepartmentId = department.Id 
                }
            };

            var handler = new UpdateAboutHandler(appDbContext, _userServiceMock.Object);
            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.AreEqual(200, result.StatusCode);

        }

        [Test]
        public async Task UpdateAbout_AboutNotFound_Returns404()
        {

            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var userEntity = new UserEntity
            {
                Name = "Test",
                Surname = "User",
                Email = "notfound@example.com"
            };
            await appDbContext.Users.AddAsync(userEntity);
            await appDbContext.SaveChangesAsync();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userEntity.Id);

            var nonExistingAboutId = Guid.NewGuid();

            var request = new UpdateAboutCommand
            {
                Data = new UpdateAboutRequest
                {
                    Id = nonExistingAboutId,
                    BirthDate = DateTime.Now,
                    DepartmentId = Guid.NewGuid()
                }
            };

            var handler = new UpdateAboutHandler(appDbContext, _userServiceMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.AreEqual(404, result.StatusCode);
        }

    }
}
