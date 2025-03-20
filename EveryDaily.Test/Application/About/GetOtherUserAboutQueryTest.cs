using EveryDaily.Application.Services.ControllerCommands.About.Queries;
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

namespace EveryDaily.Test.Application.About
{
    internal class GetOtherUserAboutQueryTest
    {

        private GetOtherUserAboutQueryHandler _handler;

        [SetUp]
        public void SetUp()
        {

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

            var query = new GetOtherUserAboutQuery(user.Id);


            // Act
            _handler = new GetOtherUserAboutQueryHandler(appDbContext);
            var users = appDbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(result.Data.BirthDate.Value.Date, birthDate.Date);
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

            var query = new GetOtherUserAboutQuery(user.Id);

            // Act
            _handler = new GetOtherUserAboutQueryHandler(appDbContext);
            var users = appDbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.IsNull(result.Data);

        }
    }
}
