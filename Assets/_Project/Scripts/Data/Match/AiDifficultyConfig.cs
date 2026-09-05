using UnityEngine;

namespace StreetLegends.Data.Match
{
    [CreateAssetMenu(fileName = "AiDifficultyConfig", menuName = "Street Legends/AI Difficulty Config")]
    public class AiDifficultyConfig : ScriptableObject
    {
        [Range(0f, 1f)] public float skillLevel = 0.5f;
        [Min(0.05f)] public float decisionIntervalSeconds = 0.2f;
        [Min(0f)] public float reactionDelaySeconds = 0.1f;
        [Range(0f, 1f)] public float errorRate = 0.15f;
        [Range(0f, 1f)] public float aggression = 0.6f;
        [Range(0f, 1f)] public float defenseAwareness = 0.5f;
        [Min(0.1f)] public float positionErrorMargin = 1.5f;
    }
}
