namespace EveryDaily.Domain.Prefix.Rank
{
    public static class XpConstants
    {
        // XP Sabitleri
        public const int BaseLoginXp = 10;
        public const int BasePostXp = 15;
        public const int BaseCommentXp = 5;
        public const int BaseLikeXp = 2;

        // Strike Sabitleri
        public const int MaxStrike = 7;
        public const int StrikeBonusXp = 5;

        // Rank Xp Limitleri
        public const int SilverRankThreshold = 170000;
        public const int GoldRankThreshold = 360000;
    }
}
