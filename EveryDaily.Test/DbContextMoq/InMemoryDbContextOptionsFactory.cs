using EveryDaily.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EveryDaily.Test.DbContextMoq;

public static class InMemoryDbContextOptionsFactory
{
    public static DbContextOptions<AppDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }
}