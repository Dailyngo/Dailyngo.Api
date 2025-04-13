using EveryDaily.Core.Entity;

namespace EveryDaily.Domain.Entities.DailyHistory
{
    public class UserDailyLoginHistoryEntity : EntityBase
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public DateTimeOffset LoginDate { get; set; }
    }
}
