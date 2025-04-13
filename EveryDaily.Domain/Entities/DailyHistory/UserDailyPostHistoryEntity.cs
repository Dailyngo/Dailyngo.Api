using EveryDaily.Core.Entity;

namespace EveryDaily.Domain.Entities.DailyHistory
{
    public class UserDailyPostHistoryEntity : EntityBase
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public DateTime PostDate { get; set; }
    }
}
