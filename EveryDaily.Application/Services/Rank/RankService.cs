using EveryDaily.Domain.Entities.DailyHistory;
using EveryDaily.Domain.Entities.Rank;
using EveryDaily.Domain.Enums.Rank;
using EveryDaily.Persistence;
using Microsoft.EntityFrameworkCore;
using EveryDaily.Domain.Prefix.Rank.Helper;
using EveryDaily.Domain.Prefix.Rank;
using EveryDaily.Application.Services.Cache;
using EveryDaily.Core.Settings;
using EveryDaily.Domain.Prefix.Redis;
using EveryDaily.Application.Dtos.Rank;

namespace EveryDaily.Application.Services.Badge
{
    public class RankService(AppDbContext dbContext,ICacheService cacheService) : IRankService
    {
        public async Task ProcessActivityAsync(Guid userId, XpActivityType activityType, CancellationToken cancellationToken)
        {
            var cacheKey = RedisPrefix.GetUserRankActivityKey(userId, activityType);
            var userActivity = await cacheService.GetAsync(cacheKey);

            if (userActivity != null)
            {
                return;
            }

            await cacheService.SetAsync(cacheKey, $"{DateTime.UtcNow}", GetTimeUntilMidnight());

            var user = await dbContext.Users
                .Include(u => u.XpStatus)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return;
            }

            var today = DateTime.UtcNow.Date;
            var currentSeason = Seasons.GetCurrentSeason();

            var xpStatus = await dbContext.UserXpStatuses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Season == currentSeason);

            if (xpStatus == null)
            {
                xpStatus = new UserXpStatusEntity
                {
                    UserId = userId,
                    Season = currentSeason,
                    TotalXp = 0,
                    LoginStrike = 0,
                    PostStrike = 0,
                    Rank = RankEnum.bronze,
                    LastLoginDate = DateTime.MinValue,
                    LastPostDate = DateTime.MinValue
                };
                dbContext.UserXpStatuses.Add(xpStatus);
            }

            int baseXp = activityType == XpActivityType.login ? XpConstants.BaseLoginXp : XpConstants.BasePostXp;
            int strikeXp = 0;
            int strike = 0;
            bool alreadyDoneToday = false;

            if (activityType == XpActivityType.login)
            {
                alreadyDoneToday = await dbContext.UserDailyLoginHistories.AnyAsync(x => x.UserId == userId && x.LoginDate == today);

                if (!alreadyDoneToday)
                {
                    await dbContext.UserDailyLoginHistories.AddAsync(new UserDailyLoginHistoryEntity
                    {
                        UserId = userId,
                        LoginDate = today
                    });

                    if (xpStatus.LastLoginDate.Date == today.AddDays(-1))
                        xpStatus.LoginStrike++;
                    else
                        xpStatus.LoginStrike = 1;

                    strike = xpStatus.LoginStrike;
                    strikeXp = Math.Min(strike, XpConstants.MaxStrike) * XpConstants.StrikeBonusXp;
                    xpStatus.LastLoginDate = today;
                }
            }
            else if (activityType == XpActivityType.post)
            {
                alreadyDoneToday = await dbContext.UserDailyPostHistories.AnyAsync(x => x.UserId == userId && x.PostDate == today);

                if (!alreadyDoneToday)
                {
                    await dbContext.UserDailyPostHistories.AddAsync(new UserDailyPostHistoryEntity
                    {
                        UserId = userId,
                        PostDate = today
                    });

                    if (xpStatus.LastPostDate.Date == today.AddDays(-1))
                        xpStatus.PostStrike++;
                    else
                        xpStatus.PostStrike = 1;

                    strike = xpStatus.PostStrike;
                    strikeXp = Math.Min(strike, XpConstants.MaxStrike) * XpConstants.StrikeBonusXp;
                    xpStatus.LastPostDate = today;
                }
            }

            if (!alreadyDoneToday)
            {
                int gainedXp = baseXp + strikeXp;
                xpStatus.TotalXp += gainedXp;

                xpStatus.Rank = GetRank(xpStatus.TotalXp);

                await dbContext.UserXpHistories.AddAsync(new UserXpHistoryEntity
                {
                    UserId = userId,
                    Date = today,
                    XpGained = gainedXp,
                    Source = activityType
                });

                if (user.XpStatus == null)
                    dbContext.UserXpStatuses.Add(xpStatus);

                await dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<UserRankResponse>> GetUserRankAsync(Guid userId, bool old, CancellationToken cancellationToken)
        {
            List<UserRankResponse> userRankDtos = new List<UserRankResponse>();
            //
            if (old)
            {
                var xpStatuses = await dbContext.UserXpStatuses
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Season)
                    .ToListAsync(cancellationToken);

                var lastXpHistories = await dbContext.UserXpHistories
                    .Where(x => x.UserId == userId)
                    .GroupBy(x => x.Date.Year)
                    .Select(g => g.OrderByDescending(x => x.Date).FirstOrDefault())
                    .ToListAsync(cancellationToken);

                foreach (var xpStatus in xpStatuses)
                {
                    var lastXpHistory = lastXpHistories
                        .FirstOrDefault(x => x.Date.Year == xpStatus.Season);

                    var lastXp = lastXpHistory?.XpGained ?? 0;
                    var lastXpReason = lastXpHistory?.Source;

                    userRankDtos.Add(new UserRankResponse
                    {
                        Rank = xpStatus.Rank,
                        TotalXp = xpStatus.TotalXp,
                        LastXp = lastXp,
                        LastXpReason = lastXpReason ?? XpActivityType.None,
                        Season = xpStatus.Season
                    });
                }
            }
            else
            {
                var currentSeason = Seasons.GetCurrentSeason();

                var xpStatus = await dbContext.UserXpStatuses
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Season == currentSeason, cancellationToken);

                var lastXpHistory = await dbContext.UserXpHistories
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefaultAsync(cancellationToken);

                var lastXp = lastXpHistory?.XpGained ?? 0;
                var lastXpReason = lastXpHistory?.Source;

                userRankDtos.Add(new UserRankResponse
                {
                    Rank = xpStatus?.Rank ?? RankEnum.bronze,
                    TotalXp = xpStatus?.TotalXp ?? 0,
                    LastXp = lastXp,
                    LastXpReason = lastXpReason ?? XpActivityType.None,
                    Season = currentSeason
                });
            }

            return userRankDtos;
        }
        private RankEnum GetRank(int totalXp)
        {
            return totalXp switch
            {
                >= XpConstants.GoldRankThreshold => RankEnum.gold,
                >= XpConstants.SilverRankThreshold => RankEnum.silver,
                _ => RankEnum.bronze
            };
        }
        private TimeSpan GetTimeUntilMidnight()
        {
            var now = DateTime.UtcNow;
            var midnight = new DateTime(now.Year, now.Month, now.Day).AddDays(1); // Ertesi günün başlangıcı (00:00)
            return midnight - now; // Şu anki zaman ile gece 23:59 arasındaki fark
        }
    }
}
