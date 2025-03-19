using EveryDaily.Core.Settings;
using EveryDaily.Domain.Documents;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using Microsoft.Extensions.Options;

namespace EveryDaily.Persistence.BaseRepositories;

public class MongoDocContext(IOptions<MongoDbSettings> options) 
    : MongoDocContextFactory(options)
{
    // Asagidaki gibi bir property olusturarak her bir model icin bir DocSet olusturabiliriz.
    public DocSet<TestModel> TestModels => Create<TestModel>();
    public DocSet<NotificationEntity> Notifications => Create<NotificationEntity>();
    public DocSet<FollowRequestEntity> FollowRequests => Create<FollowRequestEntity>();
}