using UnityEngine;

namespace StreetLegends.Gameplay.Court
{
    public class CourtBehaviour : MonoBehaviour
    {
        [SerializeField] private float courtLength = 20f;
        [SerializeField] private float courtWidth = 12f;
        [SerializeField] private float wallHeight = 2f;
        [SerializeField] private float goalWidth = 4f;
        [SerializeField] private float goalDepth = 1f;

        public float Length => courtLength;
        public float Width => courtWidth;
        public float GoalWidth => goalWidth;
        public float GoalDepth => goalDepth;

        public Bounds PlayBounds => new Bounds(
            Vector3.zero,
            new Vector3(courtWidth, 0f, courtLength)
        );

        public Vector3 PlayerStart => new Vector3(0f, 0f, -courtLength * 0.25f);
        public Vector3 AiStart => new Vector3(0f, 0f, courtLength * 0.25f);
        public Vector3 BallStart => Vector3.zero;

        public Vector3 PlayerGoalCenter => new Vector3(0f, 0f, -courtLength * 0.5f);
        public Vector3 AiGoalCenter => new Vector3(0f, 0f, courtLength * 0.5f);

        public bool IsInPlayerGoalArea(Vector3 pos)
        {
            return pos.z < -courtLength * 0.5f + goalDepth &&
                   Mathf.Abs(pos.x) < goalWidth * 0.5f;
        }

        public bool IsInAiGoalArea(Vector3 pos)
        {
            return pos.z > courtLength * 0.5f - goalDepth &&
                   Mathf.Abs(pos.x) < goalWidth * 0.5f;
        }

        public Vector3 ClampToCourt(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, -courtWidth * 0.5f, courtWidth * 0.5f);
            pos.z = Mathf.Clamp(pos.z, -courtLength * 0.5f, courtLength * 0.5f);
            pos.y = 0f;
            return pos;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(courtWidth, 0.1f, courtLength);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = Color.red;
            Vector3 playerGoalSize = new Vector3(goalWidth, wallHeight, goalDepth);
            Vector3 playerGoalPos = center + new Vector3(0f, wallHeight * 0.5f, -courtLength * 0.5f - goalDepth * 0.5f);
            Gizmos.DrawWireCube(playerGoalPos, playerGoalSize);

            Gizmos.color = Color.blue;
            Vector3 aiGoalSize = new Vector3(goalWidth, wallHeight, goalDepth);
            Vector3 aiGoalPos = center + new Vector3(0f, wallHeight * 0.5f, courtLength * 0.5f + goalDepth * 0.5f);
            Gizmos.DrawWireCube(aiGoalPos, aiGoalSize);
        }
    }
}
