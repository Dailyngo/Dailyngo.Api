using EveryDaily.Core.Entity;
using MongoDB.Driver;

namespace EveryDaily.Persistence.BaseRepositories;

internal interface IMongoDbRepository<TDocument,in TKey>
    where TDocument : IEntityBase<TKey>
    where TKey : struct
{
    Task<TDocument> GetByIdAsync(TKey id);
    Task<IEnumerable<TDocument>> GetAllAsync();
    Task InsertAsync(TDocument entity);
    Task UpdateAsync(TKey id, UpdateDefinition<TDocument> update);
    Task DeleteAsync(TKey id);
}