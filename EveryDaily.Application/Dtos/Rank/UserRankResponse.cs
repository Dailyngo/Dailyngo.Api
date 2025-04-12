using EveryDaily.Domain.Enums.Rank;

namespace EveryDaily.Application.Dtos.Rank
{
    public class UserRankResponse
    {
        public int Season { get; set; }
        public RankEnum Rank { get; set; }
        public int TotalXp { get; set; }
        public int LastXp { get; set; }
        public XpActivityType LastXpReason { get; set; }

    }
}
