using EveryDaily.Core.Entity;
using EveryDaily.Domain.Enums.Rank;

namespace EveryDaily.Domain.Entities.Rank
{
    public class UserXpStatusEntity : EntityBase
    {
        public Guid UserId {  get; set; }
        public UserEntity User { get; set; }
        public int Season { get; set; }
        public RankEnum Rank { get; set; }
        public int TotalXp { get; set; }
        public int LoginStrike { get; set; }
        public int PostStrike { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPostDate { get; set; }
    }
}
