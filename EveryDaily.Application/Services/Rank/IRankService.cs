using EveryDaily.Application.Dtos.Rank;
using EveryDaily.Domain.Enums.Rank;

namespace EveryDaily.Application.Services.Badge
{
    public interface IRankService
    {
        public Task ProcessActivityAsync(Guid userId, XpActivityType activityType,CancellationToken cancellationToken);
        public Task<List<UserRankResponse>> GetUserRankAsync(Guid userId, bool old, CancellationToken cancellationToken);
    }
}
