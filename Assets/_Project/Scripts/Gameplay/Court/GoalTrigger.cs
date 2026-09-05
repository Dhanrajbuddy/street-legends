using UnityEngine;
using StreetLegends.Gameplay.Match;

namespace StreetLegends.Gameplay.Court
{
    [RequireComponent(typeof(Collider))]
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private GoalSide side;
        public GoalSide Side => side;

        public event System.Action<GoalSide> OnGoalScored;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ball"))
            {
                OnGoalScored?.Invoke(side);
            }
        }
    }
}
