using EveryDaily.Core.Entity;
using EveryDaily.Core.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace EveryDaily.Persistence.BaseRepositories;

public abstract class MongoDocContextFactory(IOptions<MongoDbSettings> options)
{
    protected DocSet<TDocument> Create<TDocument>() where TDocument : IEntityBase<ObjectId>
    {
        return new DocSet<TDocument>(options);
    }
}