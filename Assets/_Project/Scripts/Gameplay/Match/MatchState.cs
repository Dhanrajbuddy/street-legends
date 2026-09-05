namespace StreetLegends.Gameplay.Match
{
    public enum MatchState
    {
        Countdown,
        Playing,
        GoalScored,
        Overtime,
        Finished
    }

    public enum MatchResult
    {
        None,
        PlayerWin,
        AiWin,
        Draw
    }

    public enum GoalSide
    {
        PlayerGoal,
        AiGoal
    }
}
