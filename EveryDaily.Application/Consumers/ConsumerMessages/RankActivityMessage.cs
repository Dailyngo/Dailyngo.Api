using EveryDaily.Domain.Enums.Rank;

namespace EveryDaily.Application.Consumers.ConsumerMessages
{
    public class RankActivityMessage
    {
        public Guid UserId { get; set; }
        public XpActivityType ActivityType { get; set; }
    }
}
