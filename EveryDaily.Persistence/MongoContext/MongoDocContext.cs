using EveryDaily.Core.Settings;
using EveryDaily.Domain.Documents;
using EveryDaily.Domain.Documents.Post;
using EveryDaily.Domain.Entities.Follow;
using EveryDaily.Domain.Entities.Notification;
using Microsoft.Extensions.Options;

namespace EveryDaily.Persistence.MongoContext;

public class MongoDocContext(IOptions<MongoDbSettings> options) 
    : MongoDocContextFactory(options)
{
    // Asagidaki gibi bir property olusturarak her bir model icin bir DocSet olusturabiliriz.
    public DocSet<PostDoc> Posts => Create<PostDoc>();
    public DocSet<CommentDoc> Comments => Create<CommentDoc>();
    public DocSet<LikeDoc> Likes => Create<LikeDoc>();
    public DocSet<TestModel> TestModels => Create<TestModel>();
    public DocSet<NotificationEntity> Notifications => Create<NotificationEntity>();
    public DocSet<FollowRequestEntity> FollowRequests => Create<FollowRequestEntity>();
}