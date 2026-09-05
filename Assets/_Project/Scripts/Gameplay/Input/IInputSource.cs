using UnityEngine;

namespace StreetLegends.Gameplay.Input
{
    public interface IInputSource
    {
        Vector2 MoveInput { get; }
        bool KickPressed { get; }
        bool TacklePressed { get; }
        bool SprintHeld { get; }
    }
}
