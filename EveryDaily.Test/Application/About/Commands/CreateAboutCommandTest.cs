using EveryDaily.Application.Dtos.About.Request;
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
    [TestFixture]
    public class CreateAboutCommandTest
    {
        private Mock<IUserService> _userServiceMock;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IUserService>();
        }

        [Test]
        public async Task CreateAbout_ValidData_ReturnsSuccess()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());
            // Arrange
            var department = new DepartmentEntity { Name = "Test Department" };
            await appDbContext.Departments.AddAsync(department);
            await appDbContext.SaveChangesAsync();

            var userId = Guid.NewGuid();
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetUserEmail()).Returns("test@example.com");

            var request = new CreateAboutCommand
            {
                Data = new CreateAboutRequest
                {
                    DepartmentId = department.Id,
                    BirthDate = DateTime.Now,
                }
            };

            var handler = new CreateAboutCommadHandler(appDbContext, _userServiceMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.AreEqual(200, result.StatusCode);

        }

        [Test]
        public async Task CreateAbout_DepartmentNotFound_Returns404()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var invalidDepartmentId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

            var request = new CreateAboutCommand
            {
                Data = new CreateAboutRequest
                {
                    DepartmentId = invalidDepartmentId,
                    BirthDate = DateTime.Now,

                }
            };

            var handler = new CreateAboutCommadHandler(appDbContext, _userServiceMock.Object);


            var result = await handler.Handle(request, CancellationToken.None);

            Assert.False(result.IsSuccessful);
            Assert.AreEqual(404, result.StatusCode);

        }


    }
}
