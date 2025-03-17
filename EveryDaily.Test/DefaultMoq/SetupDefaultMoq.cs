using EveryDaily.Domain.Entities;
using EveryDaily.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EveryDaily.Test.DefaultMoq;

public static class SetupDefaultMoq
{
    public static DbContextOptions<AppDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public static Mock<UserManager<UserEntity>> CreateUserManagerMock()
    {
        return new Mock<UserManager<UserEntity>>(
            Mock.Of<IUserStore<UserEntity>>(),
            null, null, null, null, null, null, null, null);
    }
}