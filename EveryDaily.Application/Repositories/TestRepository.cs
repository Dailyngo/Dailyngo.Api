using EveryDaily.Core.Entity;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Documents;
using EveryDaily.Persistence.BaseRepositories;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Repositories;
public class TestRepository(IOptions<MongoDbSettings> options) : MongoDbRepository<TestModel,ObjectId>(options)
{
    public override async Task<TestModel> GetByIdAsync(ObjectId id)
    {
        return await Collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public override async Task<IEnumerable<TestModel>> GetAllAsync()
    {
        return await Collection.Find(r => true).ToListAsync();
    }

    public override async Task DeleteAsync(ObjectId id)
    {
        await Collection.DeleteOneAsync(r => r.Id == id);
    }
}
