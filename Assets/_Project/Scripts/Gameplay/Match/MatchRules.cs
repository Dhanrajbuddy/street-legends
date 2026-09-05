using StreetLegends.Data.Match;

namespace StreetLegends.Gameplay.Match
{
    public static class MatchRules
    {
        public static MatchResult DetermineResult(int playerScore, int aiScore)
        {
            if (playerScore > aiScore) return MatchResult.PlayerWin;
            if (aiScore > playerScore) return MatchResult.AiWin;
            return MatchResult.Draw;
        }

        public static bool IsScoreCapReached(int playerScore, int aiScore, MatchConfig config)
        {
            if (config == null || config.maxGoals <= 0) return false;
            return playerScore >= config.maxGoals || aiScore >= config.maxGoals;
        }

        public static bool ShouldEnterOvertime(int playerScore, int aiScore, MatchConfig config)
        {
            if (config == null) return false;
            return config.overtimePolicy != OvertimePolicy.None && playerScore == aiScore;
        }

        public static float GetOvertimeDuration(MatchConfig config)
        {
            if (config == null) return 0f;
            return config.overtimePolicy == OvertimePolicy.ExtraTime ? config.overtimeDurationSeconds : 0f;
        }
    }
}
