using EveryDaily.Core.Settings;
using EveryDaily.Domain.Entities.Notification;
using EveryDaily.Persistence.BaseRepositories;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EveryDaily.Application.Repositories
{
    public class NotificationRepository(IOptions<MongoDbSettings> options) : MongoDbRepository<NotificationEntity, ObjectId>(options)
    {
        /// <summary>
        /// Yeni bir bildirimi ekler.
        /// </summary>
        public async Task InsertOneAsync(NotificationEntity notification, CancellationToken cancellationToken)
        {
            await Collection.InsertOneAsync(notification, cancellationToken);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip bildirimi getirir.
        /// </summary>
        public override async Task<NotificationEntity> GetByIdAsync(ObjectId id)
        {
            return await Collection.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tüm bildirimleri getirir.
        /// </summary>
        public override async Task<IEnumerable<NotificationEntity>> GetAllAsync()
        {
            return await Collection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Belirtilen ID'ye sahip bildirimi siler.
        /// </summary>
        public override async Task DeleteAsync(ObjectId id)
        {
            await Collection.DeleteOneAsync(n => n.Id == id);
        }

        /// <summary>
        /// Belirtilen filtreye göre bildirim sayısını getirir.
        /// </summary>
        public async Task<long> CountDocumentsAsync(Expression<Func<NotificationEntity, bool>> filter)
        {
            return await Collection.CountDocumentsAsync(filter);
        }

        /// <summary>
        /// Belirtilen filtreye göre bildirimleri getirir.
        /// </summary>
        public async Task<List<NotificationEntity>> GetManyAsync(Expression<Func<NotificationEntity, bool>> filter)
        {
            return await Collection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Belirtilen filtreye göre birden fazla bildirimi günceller.
        /// </summary>
        public async Task UpdateManyAsync(Expression<Func<NotificationEntity, bool>> filter, UpdateDefinition<NotificationEntity> update)
        {
            await Collection.UpdateManyAsync(filter, update);
        }
    }
}
