using EveryDaily.Core.Entity;
using EveryDaily.Core.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EveryDaily.Persistence.BaseRepositories;

public abstract class MongoDbRepository<TDocument, TKey> : IMongoDbRepository<TDocument, TKey>
    where TDocument : IEntityBase<TKey>
    where TKey : struct
{
    protected readonly IMongoCollection<TDocument> Collection;

    protected MongoDbRepository(IOptions<MongoDbSettings> options)
    {
        var configuration = options.Value;
        var connectionString = configuration.ConnectionString;
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(configuration.DatabaseName);
        Collection = database.GetCollection<TDocument>(typeof(TDocument).Name);
    }

    public abstract Task<TDocument> GetByIdAsync(TKey id);
    public abstract Task<IEnumerable<TDocument>> GetAllAsync();

    public virtual Task InsertAsync(TDocument entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        return Collection.InsertOneAsync(entity);
    }

    public virtual Task UpdateAsync(TKey id, UpdateDefinition<TDocument> update)
    {
        var withUpdatedAt = Builders<TDocument>.Update.Set(r => r.UpdatedAt, DateTimeOffset.UtcNow);
        Builders<TDocument>.Update.Combine(update, withUpdatedAt);
        return Collection.UpdateOneAsync(r => r.Id.Equals(id), update);
    }

    public abstract Task DeleteAsync(TKey id);

    public async Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> filter)
    {
        return await Collection.Find(filter).AnyAsync();
    }
}