namespace EveryDaily.Domain.Prefix.Rank.Helper
{
    public static class Seasons
    {
        public static int GetCurrentSeason()
        {
            var now = DateTime.UtcNow;
            return now.Year;
        }
    }
}
