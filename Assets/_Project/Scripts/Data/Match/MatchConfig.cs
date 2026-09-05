using UnityEngine;

namespace StreetLegends.Data.Match
{
    [CreateAssetMenu(fileName = "MatchConfig", menuName = "Street Legends/Match Config")]
    public class MatchConfig : ScriptableObject
    {
        [Min(10f)] public float matchDurationSeconds = 120f;
        [Min(1f)] public float countdownDurationSeconds = 3f;
        [Min(0.5f)] public float goalCelebrationDurationSeconds = 2f;
        [Min(0)] public int maxGoals = 0;
        public OvertimePolicy overtimePolicy = OvertimePolicy.GoldenGoal;
        [Min(0f)] public float overtimeDurationSeconds = 60f;
    }

    public enum OvertimePolicy
    {
        None,
        GoldenGoal,
        ExtraTime
    }
}
