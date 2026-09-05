namespace StreetLegends.Gameplay.Match
{
    public class ScoreBoard
    {
        public int PlayerScore { get; private set; }
        public int AiScore { get; private set; }

        public void PlayerGoal()
        {
            PlayerScore++;
        }

        public void AiGoal()
        {
            AiScore++;
        }

        public void Reset()
        {
            PlayerScore = 0;
            AiScore = 0;
        }
    }
}
