using System.Threading;
using System.Threading.Tasks;
using EveryDaily.Application.Dtos.About.Response;
using EveryDaily.Application.Services.ControllerCommands.About.Queries;
using EveryDaily.Application.Services.UserService;
using EveryDaily.Core.Dtos;
using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MassTransit;
using EveryDaily.Test.DefaultMoq;

namespace EveryDaily.Test.Application.About
{
    [TestFixture]
    public class GetAboutQueryTest
    {
        private Mock<IUserService> _userServiceMock;
        private GetAboutQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();

        }

        [Test]
        public async Task Handle_UserHasAboutInfo_ReturnsSuccess()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var user = new UserEntity
            {
                Name = "Test ",
                Surname = "User",
                UserName = "Test User",
                
            };
            await appDbContext.Users.AddAsync(user);

            var university = new UniversityEntity
            {
                Name = "Test University",
                Adress = "Test Adress",
            };
            var faculty = new FacultyEntity
            {
                Name = "Test Faculty",
                University = university,
            };
            var department = new DepartmentEntity
            {
                Name = "Test Department",
                Faculty = faculty,
            };

            var birthDate = DateTimeOffset.UtcNow;
            var about = new AboutEntity
            {
                Department = department,
                UserId = user.Id,
                BirthDate = birthDate.Date
            };



            await appDbContext.Universities.AddAsync(university);
            await appDbContext.Faculties.AddAsync(faculty);
            await appDbContext.Departments.AddAsync(department);
            await appDbContext.Abouts.AddAsync(about);
            await appDbContext.SaveChangesAsync();

            var query = new GetAboutQuery();
            
            _userServiceMock.Setup(x => x.GetUserId()).Returns(user.Id);
            // Act
            _handler = new GetAboutQueryHandler(appDbContext, _userServiceMock.Object);
            var users = appDbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(result.Data.BirthDate.Value, birthDate);
            Assert.That(result.Data.Department.Faculty.University.Name, Is.EqualTo("Test University"));
            Assert.That(result.Data.Department.Faculty.Name, Is.EqualTo("Test Faculty"));
            Assert.That(result.Data.Department.Name, Is.EqualTo("Test Department"));

        }
        [Test]
        public async Task Handle_UserHasNoAboutInfo_ReturnsFail()
        {
            var appDbContext = new AppDbContext(SetupDefaultMoq.CreateDbContextOptions());

            var user = new UserEntity
            {
                Name = "Test ",
                Surname = "User",
                UserName = "Test User",

            };
            await appDbContext.Users.AddAsync(user);
            await appDbContext.SaveChangesAsync();

            var query = new GetAboutQuery();

            _userServiceMock.Setup(x => x.GetUserId()).Returns(user.Id);
            // Act
            _handler = new GetAboutQueryHandler(appDbContext, _userServiceMock.Object);
            var users = appDbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.IsNull(result.Data);
        }


    }
}
