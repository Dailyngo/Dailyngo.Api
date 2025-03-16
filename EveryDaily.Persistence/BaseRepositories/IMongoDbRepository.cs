using EveryDaily.Core.Entity;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EveryDaily.Persistence.BaseRepositories;

public interface IMongoDbRepository<TDocument, TKey>
{
    Task<TDocument> GetByIdAsync(TKey id);
    Task<IEnumerable<TDocument>> GetAllAsync();
    Task InsertAsync(TDocument entity);
    Task UpdateAsync(TKey id, UpdateDefinition<TDocument> update);
    Task DeleteAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<TDocument, bool>> filter);
}