using EveryDaily.Core.Entity;
using EveryDaily.Core.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EveryDaily.Persistence.BaseRepositories;

public sealed class DocSet<TDocument> where TDocument : IEntityBase<ObjectId>
{
    public readonly IMongoCollection<TDocument> Collection;
    public DocSet(IOptions<MongoDbSettings> options)
    {
        var configuration = options.Value;
        var connectionString = configuration.ConnectionString;
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(configuration.DatabaseName);
        Collection = database.GetCollection<TDocument>(typeof(TDocument).Name);
    }
}