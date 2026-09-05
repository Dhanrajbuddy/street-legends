using UnityEngine;
using StreetLegends.Gameplay.Input;

namespace StreetLegends.Gameplay.Input
{
    public class TouchInputSource : MonoBehaviour, IInputSource
    {
        public Vector2 MoveInput { get; set; }
        public bool KickPressed { get; set; }
        public bool TacklePressed { get; set; }
        public bool SprintHeld { get; set; }
    }
}
