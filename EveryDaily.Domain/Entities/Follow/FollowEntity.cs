using EveryDaily.Core.Entity;

namespace EveryDaily.Domain.Entities.Follow
{
    public class FollowEntity : EntityBase
    {
        public Guid FollowerId { get; set; } 
        public Guid FollowingId { get; set; } 
        public UserEntity Follower { get; set; }
        public UserEntity Following { get; set; }
    }
}
