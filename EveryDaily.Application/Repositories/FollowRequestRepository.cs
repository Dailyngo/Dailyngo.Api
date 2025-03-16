using EveryDaily.Application.Repositories;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Domain.Enums.Fallow;
using EveryDaily.Persistence.BaseRepositories;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Application.Repositories
{
    public class FollowRequestRepository(IOptions<MongoDbSettings> options) : MongoDbRepository<FollowRequestEntity, ObjectId>(options),
          IMongoDbRepository<FollowRequestEntity, ObjectId>
    {
        public override async Task DeleteAsync(ObjectId id)
        {
            await Collection.DeleteOneAsync(f => f.Id == id);
        }

        public override async Task<IEnumerable<FollowRequestEntity>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync();
        }

        public override async Task<FollowRequestEntity> GetByIdAsync(ObjectId id)
        {
            return await Collection.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

    }
}
